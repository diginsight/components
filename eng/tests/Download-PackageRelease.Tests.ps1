#requires -Version 7.0

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot '..' 'Download-PackageRelease.ps1')

$script:Passed = 0
$script:Failed = 0

function Invoke-Test {
    param(
        [Parameter(Mandatory)]
        [string] $Name,

        [Parameter(Mandatory)]
        [scriptblock] $Body
    )

    try {
        & $Body
        $script:Passed++
        Write-Host "PASS: $Name"
    }
    catch {
        $script:Failed++
        Write-Host "FAIL: $Name - $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Assert-Equal {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [object] $Expected,

        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [object] $Actual
    )

    if ([string] $Expected -cne [string] $Actual) {
        throw "Expected '$Expected', got '$Actual'."
    }
}

function Assert-Throws {
    param(
        [Parameter(Mandatory)]
        [scriptblock] $Body,

        [string] $MessageLike = '*'
    )

    try {
        & $Body
    }
    catch {
        if ($_.Exception.Message -notlike $MessageLike) {
            throw "Expected error like '$MessageLike', got '$($_.Exception.Message)'."
        }
        return
    }
    throw 'Expected an exception, but the operation succeeded.'
}

function New-TestNupkg {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [string] $Id,

        [Parameter(Mandatory)]
        [string] $Version
    )

    $null = New-Item -ItemType Directory -Path (Split-Path -Parent $Path) -Force
    $fileStream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    try {
        $archive = [System.IO.Compression.ZipArchive]::new($fileStream, [System.IO.Compression.ZipArchiveMode]::Create, $true)
        try {
            $entry = $archive.CreateEntry("$Id.nuspec")
            $entryStream = $entry.Open()
            try {
                $writer = [System.IO.StreamWriter]::new($entryStream, [System.Text.UTF8Encoding]::new($false), 1024, $true)
                try {
                    $writer.Write("<?xml version=`"1.0`"?><package><metadata><id>$Id</id><version>$Version</version><authors>t</authors><description>t</description></metadata></package>")
                }
                finally { $writer.Dispose() }
            }
            finally { $entryStream.Dispose() }
        }
        finally { $archive.Dispose() }
    }
    finally { $fileStream.Dispose() }
}

function New-ReleaseFixture {
    param(
        [Parameter(Mandatory)]
        [string] $Root,

        [string] $Name = 'release',

        [string] $Tag = 'v3.8.0.1',

        [string] $Version = '3.8.0.1',

        [string] $Repository = 'diginsight/telemetry',

        [string[]] $Ids = @('Diginsight.Core', 'Diginsight.Diagnostics')
    )

    $path = Join-Path $Root $Name
    $null = New-Item -ItemType Directory -Path $path -Force

    $assets = foreach ($id in $Ids) {
        $fileName = "$id.$Version.nupkg"
        $packagePath = Join-Path $path $fileName
        New-TestNupkg -Path $packagePath -Id $id -Version $Version
        [ordered]@{
            fileName       = $fileName
            role           = 'package'
            packageId      = $id
            packageVersion = $Version
            sha256         = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
            size           = [long] (Get-Item -LiteralPath $packagePath).Length
        }
    }

    $manifest = [ordered]@{
        schemaVersion  = 1
        repository     = $Repository
        sourceTag      = $Tag
        packageVersion = $Version
        packages       = @($Ids | ForEach-Object { [ordered]@{ id = $_; version = $Version; symbolsRequired = $true } })
        assets         = @($assets)
    }

    $utf8 = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText((Join-Path $path 'release-manifest.json'), (($manifest | ConvertTo-Json -Depth 10) + "`n"), $utf8)
    [System.IO.File]::WriteAllText((Join-Path $path 'SHA256SUMS'), ((@($assets | ForEach-Object { "$($_.sha256)  $($_.fileName)" }) -join "`n") + "`n"), $utf8)

    return $path
}

function New-UpstreamManifest {
    param(
        [Parameter(Mandatory)]
        [string] $Path,

        [Parameter(Mandatory)]
        [object] $Content
    )

    $null = New-Item -ItemType Directory -Path (Split-Path -Parent $Path) -Force
    $utf8 = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($Path, (($Content | ConvertTo-Json -Depth 10) + "`n"), $utf8)
    return $Path
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "diginsight-download-tests-$([guid]::NewGuid().ToString('N'))"
$null = New-Item -ItemType Directory -Path $tempRoot -Force
try {
    Invoke-Test 'repository urls are parsed into owner/repo' {
        Assert-Equal 'diginsight/telemetry' (ConvertTo-RepositorySlug -Url 'https://github.com/diginsight/telemetry')
        Assert-Equal 'diginsight/telemetry' (ConvertTo-RepositorySlug -Url 'https://github.com/diginsight/telemetry/')
        Assert-Equal 'diginsight/telemetry' (ConvertTo-RepositorySlug -Url 'https://github.com/diginsight/telemetry.git')
        Assert-Equal 'diginsight/telemetry' (ConvertTo-RepositorySlug -Url 'diginsight/telemetry')
    }

    Invoke-Test 'a non-GitHub url is rejected' {
        Assert-Throws -MessageLike '*is not a GitHub repository URL*' -Body {
            ConvertTo-RepositorySlug -Url 'https://example.com/foo/bar/baz'
        }
    }

    Invoke-Test 'tag candidates account for the dropped zero component' {
        Assert-Equal 'v3.8.0' (Resolve-ReleaseTag -Repository 'x/y' -PackageVersion '3.8.0' -Offline)
        Assert-Equal 'v3.8.0.1' (Resolve-ReleaseTag -Repository 'x/y' -PackageVersion '3.8.0.1' -Offline)
    }

    Invoke-Test 'the shipped upstream inventory lists telemetry and smartcache' {
        $upstreams = @(Get-UpstreamConfiguration -Path (Join-Path $PSScriptRoot '..' 'upstream-releases.json'))
        Assert-Equal 2 $upstreams.Count
        Assert-Equal 'diginsight/telemetry' $upstreams[0].Repository
        Assert-Equal 'DiginsightCoreVersion' $upstreams[0].VersionProperty
        Assert-Equal 'diginsight/smartcache' $upstreams[1].Repository
        Assert-Equal 'DiginsightSmartCacheVersion' $upstreams[1].VersionProperty
    }

    Invoke-Test 'an upstream inventory with a duplicate repository is rejected' {
        $path = New-UpstreamManifest -Path (Join-Path $tempRoot 'dup' 'upstream-releases.json') -Content ([ordered]@{
            schemaVersion = 1
            upstreams     = @(
                [ordered]@{ repository = 'diginsight/telemetry'; versionProperty = 'A' }
                [ordered]@{ repository = 'diginsight/telemetry'; versionProperty = 'B' }
            )
        })
        Assert-Throws -MessageLike '*duplicate repository*' -Body { Get-UpstreamConfiguration -Path $path }
    }

    Invoke-Test 'no repository argument selects every configured upstream' {
        $path = Join-Path $PSScriptRoot '..' 'upstream-releases.json'
        $selection = @(Get-UpstreamSelection -ManifestPath $path)
        Assert-Equal 2 $selection.Count
    }

    Invoke-Test 'an explicit repository selects its configured version property' {
        $path = Join-Path $PSScriptRoot '..' 'upstream-releases.json'
        $selection = @(Get-UpstreamSelection -Url @('https://github.com/diginsight/smartcache') -ManifestPath $path)
        Assert-Equal 1 $selection.Count
        Assert-Equal 'DiginsightSmartCacheVersion' $selection[0].VersionProperty
    }

    Invoke-Test 'an unknown repository without an override is rejected' {
        $path = Join-Path $PSScriptRoot '..' 'upstream-releases.json'
        Assert-Throws -MessageLike '*is not listed in*' -Body {
            Get-UpstreamSelection -Url @('someone/else') -ManifestPath $path
        }
    }

    Invoke-Test 'a valid release verifies' {
        $release = New-ReleaseFixture -Root (Join-Path $tempRoot 'valid')
        $count = Test-ReleaseDownload -Path $release -Repository 'diginsight/telemetry' -Tag 'v3.8.0.1' -PackageVersion '3.8.0.1'
        Assert-Equal 2 $count
    }

    Invoke-Test 'tampered package bytes are rejected' {
        $release = New-ReleaseFixture -Root (Join-Path $tempRoot 'tampered')
        $target = Join-Path $release 'Diginsight.Core.3.8.0.1.nupkg'
        $bytes = [System.IO.File]::ReadAllBytes($target)
        $bytes[$bytes.Length - 1] = $bytes[$bytes.Length - 1] -bxor 0xFF
        [System.IO.File]::WriteAllBytes($target, $bytes)
        Assert-Throws -MessageLike '*SHA-256*' -Body {
            Test-ReleaseDownload -Path $release -Repository 'diginsight/telemetry' -Tag 'v3.8.0.1' -PackageVersion '3.8.0.1'
        }
    }

    Invoke-Test 'a release from another repository is rejected' {
        $release = New-ReleaseFixture -Root (Join-Path $tempRoot 'repo') -Repository 'someone/else'
        Assert-Throws -MessageLike '*is not*' -Body {
            Test-ReleaseDownload -Path $release -Repository 'diginsight/telemetry' -Tag 'v3.8.0.1' -PackageVersion '3.8.0.1'
        }
    }

    Invoke-Test 'a version mismatch is rejected' {
        $release = New-ReleaseFixture -Root (Join-Path $tempRoot 'version')
        Assert-Throws -MessageLike '*is not the pinned*' -Body {
            Test-ReleaseDownload -Path $release -Repository 'diginsight/telemetry' -Tag 'v3.8.0.1' -PackageVersion '3.8.0.2'
        }
    }

    Invoke-Test 'a missing declared package is rejected' {
        $release = New-ReleaseFixture -Root (Join-Path $tempRoot 'missing')
        Remove-Item -LiteralPath (Join-Path $release 'Diginsight.Diagnostics.3.8.0.1.nupkg')
        Assert-Throws -MessageLike '*missing declared package*' -Body {
            Test-ReleaseDownload -Path $release -Repository 'diginsight/telemetry' -Tag 'v3.8.0.1' -PackageVersion '3.8.0.1'
        }
    }

    Invoke-Test 'an undeclared package is rejected' {
        $release = New-ReleaseFixture -Root (Join-Path $tempRoot 'undeclared')
        New-TestNupkg -Path (Join-Path $release 'Rogue.Package.3.8.0.1.nupkg') -Id 'Rogue.Package' -Version '3.8.0.1'
        Assert-Throws -MessageLike '*undeclared package*' -Body {
            Test-ReleaseDownload -Path $release -Repository 'diginsight/telemetry' -Tag 'v3.8.0.1' -PackageVersion '3.8.0.1'
        }
    }

    Invoke-Test 'two upstreams merge into a single set of packages' {
        $root = Join-Path $tempRoot 'merge'
        $telemetry = New-ReleaseFixture -Root $root -Name 'telemetry'
        $smartcache = New-ReleaseFixture -Root $root -Name 'smartcache' -Repository 'diginsight/smartcache' -Ids @('Diginsight.SmartCache')
        $merged = Join-Path $root 'merged'
        $count = Merge-UpstreamStaging -StagedPath @($telemetry, $smartcache) -MergedPath $merged
        Assert-Equal 3 $count
        Assert-Equal 3 (@(Get-ChildItem -LiteralPath $merged -File -Filter '*.nupkg')).Count
        if (Test-Path -LiteralPath (Join-Path $merged 'SHA256SUMS')) {
            throw 'Release metadata must not reach the local source.'
        }
    }

    Invoke-Test 'a package claimed by two upstreams is rejected' {
        $root = Join-Path $tempRoot 'collision'
        $first = New-ReleaseFixture -Root $root -Name 'first'
        $second = New-ReleaseFixture -Root $root -Name 'second' -Repository 'diginsight/smartcache'
        Assert-Throws -MessageLike '*more than one upstream*' -Body {
            Merge-UpstreamStaging -StagedPath @($first, $second) -MergedPath (Join-Path $root 'merged')
        }
    }

    Invoke-Test 'a failure on the second upstream leaves the local source untouched' {
        $root = Join-Path $tempRoot 'atomic'
        $destination = Join-Path $root 'packages'
        $null = New-Item -ItemType Directory -Path $destination -Force
        '' | Set-Content -Path (Join-Path $destination '.gitkeep')
        New-TestNupkg -Path (Join-Path $destination 'Existing.Package.1.0.0.nupkg') -Id 'Existing.Package' -Version '1.0.0'
        $before = @(Get-ChildItem -LiteralPath $destination -File | Sort-Object Name | ForEach-Object { "$($_.Name):$((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash)" })

        $good = New-ReleaseFixture -Root $root -Name 'good'
        $bad = New-ReleaseFixture -Root $root -Name 'bad' -Repository 'someone/else'

        # Mirrors the script body: validate every upstream before publishing any of them.
        try {
            $staged = [System.Collections.Generic.List[string]]::new()
            foreach ($case in @(@{ Path = $good; Repository = 'diginsight/telemetry' }, @{ Path = $bad; Repository = 'diginsight/smartcache' })) {
                $null = Test-ReleaseDownload -Path $case.Path -Repository $case.Repository -Tag 'v3.8.0.1' -PackageVersion '3.8.0.1'
                $staged.Add($case.Path)
            }
            $merged = Join-Path $root 'merged'
            $null = Merge-UpstreamStaging -StagedPath @($staged) -MergedPath $merged
            Publish-LocalSource -StagedPath $merged -DestinationPath $destination
            throw 'The invalid upstream should have aborted the bootstrap.'
        }
        catch {
            if ($_.Exception.Message -like '*should have aborted*') { throw }
        }

        $after = @(Get-ChildItem -LiteralPath $destination -File | Sort-Object Name | ForEach-Object { "$($_.Name):$((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash)" })
        if (($before -join '|') -cne ($after -join '|')) {
            throw 'The local source was modified by a failed bootstrap.'
        }
    }

    Invoke-Test 'publishing keeps the .gitkeep marker and replaces packages' {
        $release = New-ReleaseFixture -Root (Join-Path $tempRoot 'publish')
        $destination = Join-Path $tempRoot 'publish' 'packages'
        $null = New-Item -ItemType Directory -Path $destination -Force
        '' | Set-Content -Path (Join-Path $destination '.gitkeep')
        New-TestNupkg -Path (Join-Path $destination 'Stale.Package.1.0.0.nupkg') -Id 'Stale.Package' -Version '1.0.0'

        $merged = Join-Path $tempRoot 'publish' 'merged'
        $null = Merge-UpstreamStaging -StagedPath @($release) -MergedPath $merged
        Publish-LocalSource -StagedPath $merged -DestinationPath $destination

        if (-not (Test-Path -LiteralPath (Join-Path $destination '.gitkeep'))) { throw 'The .gitkeep marker was removed.' }
        if (Test-Path -LiteralPath (Join-Path $destination 'Stale.Package.1.0.0.nupkg')) { throw 'A stale package survived.' }
        $packages = @(Get-ChildItem -LiteralPath $destination -File -Filter '*.nupkg')
        if ($packages.Count -ne 2) { throw "Expected 2 packages, found $($packages.Count)." }
    }
}
finally {
    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Tests passed: $script:Passed; failed: $script:Failed."
if ($script:Failed -ne 0) {
    exit 1
}
