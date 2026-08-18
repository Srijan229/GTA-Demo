$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$publishDirectory = Join-Path $workspace 'artifacts\production'
$databasePassword = ((Get-Content (Join-Path $workspace '.env') | Where-Object { $_ -like 'MYSQL_PASSWORD=*' }) -split '=', 2)[1]

$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5081'
$env:ConnectionStrings__GtaDatabase = "Server=localhost;Port=3307;Database=team2_wp;User=team2app;Password=$databasePassword"
$env:Database__ApplyMigrations = 'false'
$env:DocumentStorage__RootPath = Join-Path $workspace 'tmp\production-documents'
$logDirectory = Join-Path $workspace 'tmp\production-smoke'
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null

$applicationDll = Join-Path $publishDirectory 'Gta.Application.Api.dll'
$process = Start-Process -FilePath 'C:\Program Files\dotnet\dotnet.exe' `
    -ArgumentList "`"$applicationDll`"" `
    -WorkingDirectory $publishDirectory -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput (Join-Path $logDirectory 'stdout.log') `
    -RedirectStandardError (Join-Path $logDirectory 'stderr.log')
try {
    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try {
            $health = Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5081/health/ready' -TimeoutSec 2
            if ($health.StatusCode -eq 200) { $ready = $true; break }
        } catch { }
        Start-Sleep -Seconds 1
    }
    if (-not $ready) {
        Get-Content (Join-Path $logDirectory 'stdout.log') -Tail 50 -ErrorAction SilentlyContinue
        Get-Content (Join-Path $logDirectory 'stderr.log') -Tail 50 -ErrorAction SilentlyContinue
        throw 'Production health check did not become ready.'
    }

    $homeResponse = Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5081/' -TimeoutSec 5
    try { Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5081/api/v1/admin/access' -TimeoutSec 5 | Out-Null; throw 'Protected endpoint did not fail closed.' } catch { if ($_.Exception.Response.StatusCode.value__ -ne 401) { throw } }
    try { Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5081/api/v1/development/users' -TimeoutSec 5 | Out-Null; throw 'Development endpoint was exposed in Production.' } catch { if ($_.Exception.Response.StatusCode.value__ -ne 404) { throw } }

    [pscustomobject]@{ Health = $health.StatusCode; Home = $homeResponse.StatusCode; ProtectedEndpoint = 401; DevelopmentEndpoint = 404 }
} finally {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
}
