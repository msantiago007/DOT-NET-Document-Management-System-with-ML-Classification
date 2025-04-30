#!/bin/bash
export PATH=$PATH:~/.dotnet
cd /home/administrator/nodejs/DocumentManagementML
~/.dotnet/dotnet run --project src/DocumentManagementML.API/DocumentManagementML.API.csproj --urls="http://0.0.0.0:5149"