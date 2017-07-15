#addin nuget:?package=Cake.XCode&version=2.0.13
#addin nuget:?package=Cake.FileHelpers&version=1.0.4
#addin nuget:?package=Cake.Xamarin&version=1.3.0.15
#addin nuget:?package=Cake.Xamarin.Build&version=1.1.14

#tool nuget:?package=XamarinComponent&version=1.1.0.60
#tool nuget:?package=NUnit.ConsoleRunner&version=3.6.1
#tool nuget:?package=NUnit.Extension.NUnitV2ResultWriter&version=3.5.0

using System.Runtime.InteropServices;

internal static class MacPlatformDetector
{
    internal static readonly Lazy<bool> IsMac = new Lazy<bool>(IsRunningOnMac);

    [DllImport("libc")]
    static extern int uname(IntPtr buf);

    static bool IsRunningOnMac()
    {
        IntPtr buf = IntPtr.Zero;
        try {
            buf = Marshal.AllocHGlobal(8192);
            // This is a hacktastic way of getting sysname from uname()
            if (uname(buf) == 0) {
                string os = Marshal.PtrToStringAnsi(buf);
                if (os == "Darwin")
                    return true;
            }
        } catch {
        } finally {
            if (buf != IntPtr.Zero)
                Marshal.FreeHGlobal(buf);
        }
        return false;
    }
}

bool IsRunningOnMac()
{
    return System.Environment.OSVersion.Platform == PlatformID.MacOSX || MacPlatformDetector.IsMac.Value;
}

bool IsRunningOnLinux()
{
    return IsRunningOnUnix() && !IsRunningOnMac();
}

var RunProcess = new Action<FilePath, ProcessSettings>((process, settings) =>
{
    var result = StartProcess(process, settings);
    if (result != 0) {
        throw new Exception("Process '" + process + "' failed with error: " + result);
    }
});

var GetSNToolPath = new Func<string, FilePath>((possible) =>
{
    if (string.IsNullOrEmpty(possible)) {
        if (IsRunningOnLinux()) {
            possible = "/usr/lib/mono/4.5/sn.exe";
        } else if (IsRunningOnMac()) {
            possible = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/sn.exe";
        } else if (IsRunningOnWindows()) {
            // search through all the SDKs to find the latest
            var snExes = new List<string>();
            var arch = Environment.Is64BitOperatingSystem ? "x64" : "";
            var progFiles = (DirectoryPath)Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var dirPath = progFiles.Combine("Microsoft SDKs/Windows").FullPath + "/v*A";
            var dirs = GetDirectories(dirPath).OrderBy(d => {
                var version = d.GetDirectoryName();
                return double.Parse(version.Substring(1, version.Length - 2));
            });
            foreach (var dir in dirs) {
                var path = dir.FullPath + "/bin/*/" + arch + "/sn.exe";
                var files = GetFiles(path).Select(p => p.FullPath).ToList();
                files.Sort();
                snExes.AddRange(files);
            }

            possible = snExes.LastOrDefault();
        }
    }
    return possible;
});

var SNToolPath = GetSNToolPath(EnvironmentVariable("SN_EXE"));

var RunSNVerify = new Action<FilePath>((assembly) =>
{
    RunProcess(SNToolPath, new ProcessSettings {
        Arguments = string.Format("-vf \"{0}\"", MakeAbsolute(assembly)),
    });
});

var RunSNReSign = new Action<FilePath, FilePath>((assembly, key) =>
{
    RunProcess(SNToolPath, new ProcessSettings {
        Arguments = string.Format("-R \"{0}\" \"{1}\"", MakeAbsolute(assembly), MakeAbsolute(key)),
    });
});

var SignAssemblies = new Action<string, string>((files, key) => 
{
    foreach (var f in GetFiles(files)) {
        SignAssembly(f, key);
    }
});

var SignAssembly = new Action<FilePath, FilePath>((file, key) => 
{
    Information("Making sure that '{0}' is signed.", file);
    RunSNReSign(MakeAbsolute(file), MakeAbsolute(key));
    RunSNVerify(MakeAbsolute(file));
});
