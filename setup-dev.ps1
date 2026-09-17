#Requires -Version 5.1
<#
    Castle Attack - developer setup (Windows)

    Run once, from the repo root, after cloning:
        .\setup-dev.ps1

    Configures the two things that CANNOT be committed to the repo
    and therefore have to be set up on each machine:

      1. Unity's SmartMerge (UnityYAMLMerge) as the merge driver for
         scene/prefab files. Without this, git silently falls back to
         a line-based merge and mangles them.
      2. Git LFS. Without this, art assets clone as text pointer files
         instead of real content.

    Both failure modes are silent, which is why this script exists.
#>

$ErrorActionPreference = 'Stop'
$ok   = @()
$fail = @()

Write-Host ""
Write-Host "Castle Attack - developer setup" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

# --- sanity: are we in the repo root? -------------------------------
if (-not (Test-Path ".git")) {
    Write-Host "ERROR: no .git folder here." -ForegroundColor Red
    Write-Host "Run this from the repository root." -ForegroundColor Red
    exit 1
}

# --- 1. Unity SmartMerge --------------------------------------------
Write-Host "[1/2] Unity SmartMerge (scene + prefab merging)" -ForegroundColor White

$searchPaths = @(
    "$env:ProgramFiles\Unity\Hub\Editor\*\Editor\Data\Tools\UnityYAMLMerge.exe",
    "${env:ProgramFiles(x86)}\Unity\Hub\Editor\*\Editor\Data\Tools\UnityYAMLMerge.exe",
    "$env:ProgramFiles\Unity\Editor\Data\Tools\UnityYAMLMerge.exe"
)

$mergeTool = $null
foreach ($p in $searchPaths) {
    $hit = Get-ChildItem -Path $p -ErrorAction SilentlyContinue |
           Sort-Object FullName -Descending |
           Select-Object -First 1
    if ($hit) { $mergeTool = $hit.FullName; break }
}

if (-not $mergeTool) {
    Write-Host "      Could not find UnityYAMLMerge.exe automatically." -ForegroundColor Yellow
    Write-Host "      It normally lives at:" -ForegroundColor Yellow
    Write-Host "        C:\Program Files\Unity\Hub\Editor\<version>\Editor\Data\Tools\" -ForegroundColor Yellow
    $mergeTool = Read-Host "      Paste the full path (or press Enter to skip)"
}

if ($mergeTool -and (Test-Path $mergeTool)) {
    # Forward slashes: git's config parser dislikes backslashes here.
    $gitPath = $mergeTool -replace '\\', '/'
    git config merge.unityyamlmerge.name "Unity SmartMerge"
    git config merge.unityyamlmerge.driver "`"$gitPath`" merge -p --force --fallback none %O %B %A %A"
    git config merge.unityyamlmerge.recursive binary

    $check = git config --get merge.unityyamlmerge.driver
    if ($check) {
        Write-Host "      OK - registered" -ForegroundColor Green
        Write-Host "      $mergeTool" -ForegroundColor DarkGray
        $ok += "SmartMerge"
    } else {
        Write-Host "      FAILED - config did not stick" -ForegroundColor Red
        $fail += "SmartMerge"
    }
} else {
    Write-Host "      SKIPPED - scene merges will NOT be handled properly." -ForegroundColor Red
    $fail += "SmartMerge"
}

Write-Host ""

# --- 2. Git LFS -----------------------------------------------------
Write-Host "[2/2] Git LFS (art assets)" -ForegroundColor White

$lfsVersion = (git lfs version 2>&1)
if ($LASTEXITCODE -ne 0) {
    Write-Host "      NOT INSTALLED." -ForegroundColor Red
    Write-Host "      Git for Windows normally bundles it. Install from:" -ForegroundColor Yellow
    Write-Host "        https://git-lfs.com" -ForegroundColor Yellow
    Write-Host "      Then re-run this script." -ForegroundColor Yellow
    $fail += "Git LFS"
} else {
    git lfs install | Out-Null
    Write-Host "      OK - $lfsVersion" -ForegroundColor Green
    $ok += "Git LFS"

    # If assets were cloned before LFS was set up, they're pointer files.
    Write-Host "      Fetching any LFS content not yet pulled..." -ForegroundColor DarkGray
    git lfs pull 2>&1 | Out-Null
}

# --- summary --------------------------------------------------------
Write-Host ""
Write-Host "===============================" -ForegroundColor Cyan
if ($fail.Count -eq 0) {
    Write-Host "All set. You're good to work." -ForegroundColor Green
} else {
    Write-Host "Configured: $($ok -join ', ')" -ForegroundColor Green
    Write-Host "STILL NEEDED: $($fail -join ', ')" -ForegroundColor Red
    Write-Host ""
    Write-Host "Do not skip these - both fail silently and corrupt work." -ForegroundColor Red
}
Write-Host ""
Write-Host "Reminder: don't edit the same scene as someone else at the" -ForegroundColor DarkGray
Write-Host "same time. SmartMerge makes conflicts survivable, not fun." -ForegroundColor DarkGray
Write-Host ""
