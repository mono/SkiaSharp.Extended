import groovy.transform.Field

@Field def isPr = false
@Field def branchName = null
@Field def commitHash = null
@Field def githubStatusSha = null

@Field def stashes = []
@Field def customEnv = []

properties([
    compressBuildLog()
])

// ============================================================================
// Stages

node("ubuntu-1604-amd64") {
    stage("Prepare") {
        timestamps {
            checkout scm
            commitHash = cmdResult("git rev-parse HEAD").trim()

            isPr = env.ghprbPullId && !env.ghprbPullId.empty
            branchName = isPr ? "pr" : env.BRANCH_NAME
            githubStatusSha = isPr ? env.ghprbActualCommit : commitHash

            echo "Building SHA1: ${commitHash}..."
            echo " - PR: ${isPr}"
            echo " - Branch Name: ${branchName}"
            echo " - GitHub Status SHA1: ${githubStatusSha}"

            customEnv.push("GIT_SHA=${commitHash}")
        }
    }

    stage("Build") {
        parallel([
            failFast: true,

            windows: createBuilder("Windows", "components-windows"),
            macos:   createBuilder("macOS",   "components"),
        ])
    }

    stage("Package") {
        parallel([
            failFast: true,

            package: createPackager(),
        ])
    }

    stage("Clean Up") {
        timestamps {
            cleanWs()
        }
    }
}

// ============================================================================
// Functions

def createBuilder(host, label) {
    def githubContext = "Build - ${host}"
    host = host.toLowerCase()

    reportGitHubStatus(githubContext, "PENDING", "Building...")

    return {
        stage(githubContext) {
            node(label) {
                timestamps {
                    withEnv(customEnv + ["NODE_LABEL=${label}"]) {
                        ws("${getWSRoot()}/build-${host}") {
                            try {
                                checkout scm

                                pwsh("build.ps1")

                                step([
                                    $class: "XUnitPublisher",
                                    testTimeMargin: "3000",
                                    thresholdMode: 1,
                                    thresholds: [[
                                        $class: "FailedThreshold",
                                        failureNewThreshold: "0",
                                        failureThreshold: "0",
                                        unstableNewThreshold: "0",
                                        unstableThreshold: "0"
                                    ]],
                                    tools: [[
                                        $class: "XUnitDotNetTestType",
                                        deleteOutputFiles: true,
                                        failIfNotNew: true,
                                        pattern: "output/**/TestResult.xml",
                                        skipNoTestFiles: false,
                                        stopProcessingIfError: true
                                    ]]
                                ])

                                stashes.push(host)
                                stash(name: host, includes: "output/**/*", allowEmpty: false)

                                cleanWs()
                                reportGitHubStatus(githubContext, "SUCCESS", "Build complete.")
                            } catch (Exception e) {
                                reportGitHubStatus(githubContext, "FAILURE", "Build failed.")
                                throw e
                            }
                        }
                    }
                }
            }
        }
    }
}

def createPackager() {
    def githubContext = "Packing"
    def host = "linux"
    def label = "ubuntu-1604-amd64"

    reportGitHubStatus(githubContext, "PENDING", "Packing...")

    return {
        stage(githubContext) {
            node(label) {
                timestamps{
                    withEnv(customEnv + ["NODE_LABEL=${label}"]) {
                        ws("${getWSRoot()}/pack-${host}") {
                            try {
                                checkout scm
                                stashes.each { unstash(it) }

                                uploadBlobs()

                                cleanWs()
                                reportGitHubStatus(githubContext, "SUCCESS", "Pack complete.")
                            } catch (Exception e) {
                                reportGitHubStatus(githubContext, "FAILURE", "Pack failed.")
                                throw e
                            }
                        }
                    }
                }
            }
        }
    }
}

def uploadBlobs() {
    fingerprint("output/**/*")
    step([
        $class: "WAStoragePublisher",
        allowAnonymousAccess: true,
        cleanUpContainer: false,
        cntPubAccess: true,
        containerName: "skiasharp-extended-public-artifacts",
        doNotFailIfArchivingReturnsNothing: false,
        doNotUploadIndividualFiles: false,
        doNotWaitForPreviousBuild: true,
        excludeFilesPath: "",
        filesPath: "output/**/*",
        storageAccName: "credential for xamjenkinsartifact",
        storageCredentialId: "fbd29020e8166fbede5518e038544343",
        uploadArtifactsOnlyIfSuccessful: false,
        uploadZips: false,
        virtualPath: "ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/",
    ])
}

def reportGitHubStatus(context, statusResult, statusResultMessage) {
    step([
        $class: "GitHubCommitStatusSetter",
        commitShaSource: [
            $class: "ManuallyEnteredShaSource",
            sha: githubStatusSha
        ],
        contextSource: [
            $class: "ManuallyEnteredCommitContextSource",
            context: context + (isPr ? " (PR)" : "")
        ],
        statusBackrefSource: [
            $class: "ManuallyEnteredBackrefSource",
            backref: env.BUILD_URL
        ],
        statusResultSource: [
            $class: "ConditionalStatusResultSource",
            results: [[
                $class: "AnyBuildResult",
                state: statusResult,
                message: statusResultMessage
            ]]
        ]
    ])
}

def pwsh(script) {
    if (isUnix()) {
        return sh("pwsh " + script)
    } else {
        return bat("powershell " + script)
    }
}

def cmd(script) {
    if (isUnix()) {
        return sh(script)
    } else {
        return bat(script)
    }
}

def cmdResult(script) {
    if (isUnix()) {
        return sh(script: script, returnStdout: true)
    } else {
        return bat(script: script, returnStdout: true)
    }
}

def getWSRoot() {
    def cleanBranch = branchName.replace("/", "_").replace("\\", "_")
    def wsRoot = isUnix() ? "workspace" : "C:/bld"
    return "${wsRoot}/SkiaExd/${cleanBranch}"
}
