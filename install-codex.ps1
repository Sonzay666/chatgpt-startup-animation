$ErrorActionPreference = 'Stop'

$repo = 'Sonzay666/chatgpt-startup-animation'
$assetName = 'Codex-550C-Launcher-win-x64.zip'
$checksumName = 'SHA256SUMS.txt'
$headers = @{
    Accept = 'application/vnd.github+json'
    'User-Agent' = 'chatgpt-startup-animation-installer'
}
$work = Join-Path $env:TEMP ('Codex550C-install-' + [guid]::NewGuid().ToString('N'))
$installRoot = Join-Path $env:LOCALAPPDATA 'Programs\Codex-550C-Animation'

try {
    if (-not [Environment]::Is64BitOperatingSystem) { throw 'This release requires 64-bit Windows.' }
    if (-not (Get-AppxPackage -Name 'OpenAI.Codex' -ErrorAction SilentlyContinue)) {
        throw 'Install the official Windows desktop app with Codex access before installing this launcher.'
    }
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
    New-Item -ItemType Directory -Path $work -Force | Out-Null
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$repo/releases/latest" -Headers $headers
    $zipAsset = $release.assets | Where-Object name -eq $assetName | Select-Object -First 1
    $sumAsset = $release.assets | Where-Object name -eq $checksumName | Select-Object -First 1
    if (-not $zipAsset -or -not $sumAsset) { throw 'The latest GitHub Release is missing its ZIP or checksum file.' }

    $zipPath = Join-Path $work $assetName
    $sumPath = Join-Path $work $checksumName
    Invoke-WebRequest -Uri $zipAsset.browser_download_url -Headers $headers -OutFile $zipPath
    Invoke-WebRequest -Uri $sumAsset.browser_download_url -Headers $headers -OutFile $sumPath

    $sumLine = Get-Content -LiteralPath $sumPath | Where-Object { $_ -match [regex]::Escape($assetName) } | Select-Object -First 1
    if (-not $sumLine) { throw 'The SHA-256 entry for the launcher ZIP was not found.' }
    $expectedHash = ($sumLine -split '\s+', 2)[0].ToUpperInvariant()
    $actualHash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToUpperInvariant()
    if ($actualHash -ne $expectedHash) { throw 'The ZIP SHA-256 check failed. The package was not installed.' }

    $safeTag = $release.tag_name -replace '[^A-Za-z0-9._-]', '_'
    $versionFolder = Join-Path $installRoot ($safeTag + '-' + $actualHash.Substring(0, 12))
    $marker = Join-Path $versionFolder '.package-sha256'
    if (-not (Test-Path -LiteralPath $marker) -or (Get-Content -LiteralPath $marker -Raw).Trim() -ne $actualHash) {
        if (Test-Path -LiteralPath $versionFolder) {
            $versionFolder = Join-Path $installRoot ($safeTag + '-' + $actualHash.Substring(0, 12) + '-' + [guid]::NewGuid().ToString('N'))
        }
        $stage = Join-Path $work 'payload'
        Expand-Archive -LiteralPath $zipPath -DestinationPath $stage
        $exe = Join-Path $stage 'Codex550CLauncher.exe'
        if (-not (Test-Path -LiteralPath $exe)) { throw 'The downloaded package does not contain the launcher executable.' }
        New-Item -ItemType Directory -Path $installRoot -Force | Out-Null
        Move-Item -LiteralPath $stage -Destination $versionFolder
        Set-Content -LiteralPath (Join-Path $versionFolder '.package-sha256') -Value $actualHash -Encoding ASCII
    }

    $launcher = Join-Path $versionFolder 'Codex550CLauncher.exe'
    if (-not (Test-Path -LiteralPath $launcher)) { throw 'The installed launcher executable is missing.' }
    $desktop = [Environment]::GetFolderPath('DesktopDirectory')
    $shortcutPath = Join-Path $desktop 'Codex 550C 开场动画.lnk'
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $launcher
    $shortcut.WorkingDirectory = $versionFolder
    $shortcut.IconLocation = "$launcher,0"
    $shortcut.Description = 'Open Codex with the 550C startup animation overlay'
    $shortcut.Save()

    Write-Output "Installed $($release.tag_name). Desktop shortcut: $shortcutPath"
}
finally {
    if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue }
}
