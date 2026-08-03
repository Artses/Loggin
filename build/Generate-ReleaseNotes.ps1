param(
    [Parameter(Mandatory = $true)][string]$Version,
    [string]$PreviousTag = '',
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
$range = if ([string]::IsNullOrWhiteSpace($PreviousTag)) { 'HEAD' } else { "$PreviousTag..HEAD" }
$pretty = "- %s ([%h](https://github.com/$($env:GITHUB_REPOSITORY)/commit/%H)) - %an"
$commits = @(git log $range "--pretty=format:$pretty")
$rawAuthors = @(git log $range --format='%aN|%aE%n%(trailers:key=Co-authored-by,valueonly)')
$commitShas = @(git rev-list $range)
$pullRequestLines = @()

$contributors = [ordered]@{}
foreach ($entry in $rawAuthors) {
    if ([string]::IsNullOrWhiteSpace($entry)) { continue }
    $clean = $entry.Trim() -replace '^.*:\s*', ''
    $name = ($clean -split '[|<]')[0].Trim()
    $email = if ($clean -match '<([^>]+)>') { $Matches[1] } elseif ($clean -match '\|(.+)$') { $Matches[1] } else { '' }
    if ($name -match '(?i)\[bot\]|dependabot|github-actions|renovate') { continue }
    $key = if ($email) { $email.ToLowerInvariant() } else { $name.ToLowerInvariant() }
    if (-not $contributors.Contains($key)) { $contributors[$key] = $name }
}

$gh = Get-Command gh -ErrorAction SilentlyContinue
if ($gh -and $env:GITHUB_REPOSITORY) {
    $prJson = & $gh.Source pr list --repo $env:GITHUB_REPOSITORY --state merged --base main --limit 200 `
        --json number,title,url,author,mergeCommit 2>$null
    if ($LASTEXITCODE -eq 0 -and $prJson) {
        foreach ($pr in ($prJson | ConvertFrom-Json)) {
            if (-not $pr.mergeCommit.oid -or $commitShas -notcontains $pr.mergeCommit.oid) { continue }
            $login = $pr.author.login
            $pullRequestLines += "- [#$($pr.number) $($pr.title)]($($pr.url)) - @$login"
            if ($login -and $login -notmatch '(?i)\[bot\]|dependabot|github-actions|renovate') {
                $alreadyKnown = @($contributors.Values | Where-Object {
                    ($_ -replace '[^a-zA-Z0-9]', '').Equals(
                        ($login -replace '[^a-zA-Z0-9]', ''), [StringComparison]::OrdinalIgnoreCase)
                }).Count -gt 0
                if (-not $alreadyKnown) { $contributors["github:$($login.ToLowerInvariant())"] = "@$login" }
            }
        }
    }
}

$contributorLines = @($contributors.Values | Sort-Object | ForEach-Object { "- $_" })
if ($contributorLines.Count -eq 0) { $contributorLines = @('- Artses') }
if ($commits.Count -eq 0) { $commits = @('- Initial Loggin Desktop release.') }
if ($pullRequestLines.Count -eq 0) { $pullRequestLines = @('- No merged pull requests were identified for this range.') }

$date = Get-Date -Format 'yyyy-MM-dd'
$previousText = if ($PreviousTag) { $PreviousTag } else { 'the beginning of the project' }
$content = @"
# Loggin Desktop $Version

Published on **$date**.

## Summary

This release contains the changes merged into ``main`` since $previousText.

## Changes and references

$($commits -join "`n")

Issues and pull requests mentioned by the commits above are linked automatically by GitHub.

## Included pull requests

$($pullRequestLines -join "`n")

## Credits

**Primary author:** Artses

### Contributors

$($contributorLines -join "`n")
"@

$directory = Split-Path -Parent $OutputPath
if ($directory) { New-Item -ItemType Directory -Path $directory -Force | Out-Null }
Set-Content -LiteralPath $OutputPath -Value $content -Encoding utf8
