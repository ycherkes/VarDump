# Low-noise run of all VarDump benchmarks (about 20-30 minutes).
# Usage (from the repository root):  powershell -ExecutionPolicy Bypass -File perf/run-precise-benchmarks.ps1
# Before running: plug in the charger, close browsers/IDEs/antivirus scans, do not touch the PC while it runs.
param(
    # Hex affinity mask. On Core Ultra 9 275HX the 8 P-cores are logical processors 0, 1, 10-13, 22, 23
    # (EfficiencyClass 1 in GetSystemCpuSetInformation), so C03C03 pins to P-cores only, avoiding P/E-core migration.
    [string]$Affinity = "C03C03",
    [string]$Framework = "net10.0"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$out  = Join-Path $root ("perf/benchmark-results-" + (Get-Date -Format "yyyy-MM-dd") + "-precise")
# Not "BenchmarkRun-*.log": BenchmarkDotNet writes its own log with that name into the artifacts folder.
$log  = Join-Path $out ("console-" +(Get-Date -Format "yyyyMMdd-HHmmss") + ".log")
New-Item -ItemType Directory -Force -Path $out | Out-Null

$env:VARDUMP_BENCH_AFFINITY = $Affinity
powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c 2>$null   # High performance plan

dotnet run -c Release -f $Framework `
    --project (Join-Path $root "perf/VarDump.Performance/VarDump.Performance.csproj") `
    -- --filter "*" --artifacts $out 2>&1 | Tee-Object -FilePath $log

Write-Host "Done. Results: $out"
