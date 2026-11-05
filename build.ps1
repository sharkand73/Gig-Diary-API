$ErrorActionPreference = "Stop"

dotnet build -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

zip -j ../Infrastructure/Lambda/GigDiary ./ApiLambda/src/ApiLambda/bin/Release/net10.0/*.*
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Set-Location ../Infrastructure
terraform apply -auto-approve
