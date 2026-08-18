$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$output = Join-Path $workspace 'artifacts\production'

npm run lint:web
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
npm run test:web
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
npm run build:web
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& 'C:\Program Files\dotnet\dotnet.exe' test (Join-Path $workspace 'apps\api\Gta.Application.sln') --configuration Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& 'C:\Program Files\dotnet\dotnet.exe' publish (Join-Path $workspace 'apps\api\Gta.Application.Api\Gta.Application.Api.csproj') --configuration Release --output $output
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$wwwroot = Join-Path $output 'wwwroot'
New-Item -ItemType Directory -Force -Path $wwwroot | Out-Null
Copy-Item -Path (Join-Path $workspace 'apps\web\dist\*') -Destination $wwwroot -Recurse -Force
Write-Output $output
