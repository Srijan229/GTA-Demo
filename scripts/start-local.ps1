$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$runDirectory = Join-Path $workspace 'tmp\run'
New-Item -ItemType Directory -Force -Path $runDirectory | Out-Null

$listeners = Get-NetTCPConnection -State Listen -LocalPort 5080, 5173 -ErrorAction SilentlyContinue
if (-not ($listeners | Where-Object LocalPort -eq 5080)) {
    $passwordLine = Get-Content (Join-Path $workspace '.env') | Where-Object { $_ -like 'MYSQL_PASSWORD=*' }
    $databasePassword = ($passwordLine -split '=', 2)[1]
    $env:ConnectionStrings__GtaDatabase = "Server=localhost;Port=3307;Database=team2_wp;User=team2app;Password=$databasePassword"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    Start-Process -FilePath 'C:\Program Files\dotnet\dotnet.exe' `
        -ArgumentList @('run', '--project', 'apps/api/Gta.Application.Api', '--urls', 'http://localhost:5080') `
        -WorkingDirectory $workspace -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $runDirectory 'api.log') `
        -RedirectStandardError (Join-Path $runDirectory 'api-error.log')
}

if (-not ($listeners | Where-Object LocalPort -eq 5173)) {
    Start-Process -FilePath 'C:\Program Files\nodejs\npm.cmd' `
        -ArgumentList @('run', 'dev:web') `
        -WorkingDirectory $workspace -WindowStyle Hidden `
        -RedirectStandardOutput (Join-Path $runDirectory 'web.log') `
        -RedirectStandardError (Join-Path $runDirectory 'web-error.log')
}
