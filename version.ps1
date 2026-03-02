#!/usr/bin/pwsh

# ------------------------
# Update version.json
# ------------------------

# Get script directory
$SCRIPT_DIR = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition

# Fetch the latest tags with more git history
git fetch --prune --unshallow --tags 2>$null

# Get the most recent tag with fallback options
$GIT_TAG = $null
$GIT_TAG = git describe --tags --abbrev=0 2>$null
if (-not $GIT_TAG) {
    # Try getting tag from GitHub Actions environment variable
    $GIT_TAG = $env:GITHUB_REF
    if ($GIT_TAG -and $GIT_TAG.StartsWith("refs/tags/")) {
        $GIT_TAG = $GIT_TAG.Substring(10)
    } else {
        $GIT_TAG = "v0.0.0"
    }
}
# Get short commit hash
$COMMIT_HASH = git rev-parse --short HEAD

# Build date info
$NOW = Get-Date -UFormat %s
$MIDNIGHT = (Get-Date -Date (Get-Date).Date -UFormat %s)
$MINUTES_SINCE_MIDNIGHT = [math]::Floor(($NOW - $MIDNIGHT) / 60)
$BUILD_TICK = $MINUTES_SINCE_MIDNIGHT

# Format build date
$BUILD_DATE = Get-Date -Format "yyyy-MM-ddTHH:mm:ssK"

# Build number (YYYYMMDD.Tick)
$DATE = Get-Date -Format "yyyyMMdd"
$BUILD_NUMBER = "$DATE.$BUILD_TICK"

# Get branch and full commit hash
$GIT_BRANCH = git rev-parse --abbrev-ref HEAD
$GIT_HEAD = git rev-parse HEAD

# Print build number
Write-Host "Build Number: $BUILD_NUMBER"

# Create JSON object
$JSON = @{
    BuildNumber = $BUILD_NUMBER
    BuildDate   = $BUILD_DATE
    CommitHash  = $COMMIT_HASH
    GitBranch   = $GIT_BRANCH
    GitHead     = $GIT_HEAD
    GitTag      = $GIT_TAG
} | ConvertTo-Json -Depth 10 -Compress

# Write version.json to Api
$OUTPUT_PATH = Join-Path -Path $SCRIPT_DIR -ChildPath "Api/version.json"
New-Item -ItemType Directory -Path (Split-Path -Parent $OUTPUT_PATH) -Force | Out-Null
$JSON | Set-Content -Path $OUTPUT_PATH -Encoding UTF8
Write-Host "✅ version.json written to $OUTPUT_PATH"

