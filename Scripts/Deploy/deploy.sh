#!/bin/bash

LOCAL=$(dirname "$0")

cd "${LOCAL}/../../"

rm -rf bin
rm -rf obj

dotnet restore
dotnet pack
dotnet nuget push bin/Release/Yape.Http.Client.1.0.0.nupkg -s yape-bolivia-nuget-registry