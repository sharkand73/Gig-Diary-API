dotnet build -c Release
zip -j ../Infrastructure/Lambda/GigDiary ./ApiLambda/src/ApiLambda/bin/Release/net8.0/*.*
