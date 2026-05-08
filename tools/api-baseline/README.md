# API Baseline

This directory contains the reference assemblies used to verify API compatibility
of `SkiaSharp.Extended.Drawing.Common` against `System.Drawing.Common`.

## Contents

| File | Purpose |
|------|---------|
| `netstandard2.0/System.Drawing.Common.dll` | Reference assembly from the official `System.Drawing.Common` NuGet package, used as the **left side** (baseline) in API compatibility checks. |
| `api-compat-suppressions.xml` | Known API differences that are intentionally suppressed. |

## How the Check Works

The CI pipeline (`api_compat` job in `azure-pipelines-public.yml`) builds
`SkiaSharp.Extended.Drawing.Common` and runs:

```
dotnet apicompat \
  --left  tools/api-baseline/netstandard2.0/System.Drawing.Common.dll \
  --right source/SkiaSharp.Extended.Drawing.Common/bin/Release/netstandard2.0/System.Drawing.Common.dll \
  --strict-mode \
  --suppression-file tools/api-baseline/api-compat-suppressions.xml
```

This ensures that every public API surface in the official `System.Drawing.Common`
is also present in our SkiaSharp-backed replacement.

## Updating the Baseline

1. Download the desired version of the `System.Drawing.Common` NuGet package.
2. Extract the `netstandard2.0` reference assembly from the package.
3. Replace `netstandard2.0/System.Drawing.Common.dll` with the new assembly.
4. Run the API compatibility check locally to identify any new differences.
5. If there are intentional gaps, add entries to `api-compat-suppressions.xml`.
