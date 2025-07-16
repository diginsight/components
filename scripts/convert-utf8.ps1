# UTF-8 Encoding Utility Script
# This script helps detect and convert UTF-16 encoded files to UTF-8 without BOM

param(
    [switch]$Check,
    [switch]$Convert,
    [string[]]$Extensions = @("*.cs", "*.csproj", "*.xml", "*.config", "*.props", "*.targets", "*.json", "*.yml", "*.yaml", "*.md", "*.txt")
)

function Test-UTF16File {
    param([string]$FilePath)
    try {
        $bytes = Get-Content $FilePath -Encoding Byte -TotalCount 2 -ErrorAction SilentlyContinue
        return ($bytes.Length -ge 2 -and $bytes[0] -eq 255 -and $bytes[1] -eq 254)
    }
    catch {
        return $false
    }
}

function Convert-ToUTF8 {
    param([string]$FilePath)
    try {
        Write-Host "Converting $FilePath to UTF-8..." -ForegroundColor Yellow
        $content = Get-Content $FilePath -Raw -Encoding Unicode
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($FilePath, $content, $utf8NoBom)
        Write-Host "✓ Converted $FilePath" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Failed to convert $FilePath : $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "UTF-8 Encoding Utility" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan

$utf16Files = @()
$allFiles = Get-ChildItem -Path . -Recurse -Include $Extensions | Where-Object { !$_.PSIsContainer }

Write-Host "Scanning $($allFiles.Count) files..." -ForegroundColor White

foreach ($file in $allFiles) {
    if (Test-UTF16File -FilePath $file.FullName) {
        $utf16Files += $file
        Write-Host "UTF-16 detected: $($file.FullName)" -ForegroundColor Magenta
    }
}

if ($utf16Files.Count -eq 0) {
    Write-Host "✓ No UTF-16 files found. All files appear to be properly encoded." -ForegroundColor Green
} else {
    Write-Host "Found $($utf16Files.Count) UTF-16 encoded files:" -ForegroundColor Yellow
    $utf16Files | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor White }
    
    if ($Convert) {
        Write-Host "`nConverting files to UTF-8..." -ForegroundColor Yellow
        foreach ($file in $utf16Files) {
            Convert-ToUTF8 -FilePath $file.FullName
        }
        Write-Host "`n✓ Conversion completed!" -ForegroundColor Green
    } else {
        Write-Host "`nTo convert these files to UTF-8, run: .\convert-utf8.ps1 -Convert" -ForegroundColor Cyan
    }
}

if (!$Check -and !$Convert) {
    Write-Host "`nUsage:" -ForegroundColor Cyan
    Write-Host "  .\convert-utf8.ps1 -Check     # Check for UTF-16 files" -ForegroundColor White
    Write-Host "  .\convert-utf8.ps1 -Convert   # Convert UTF-16 files to UTF-8" -ForegroundColor White
}
