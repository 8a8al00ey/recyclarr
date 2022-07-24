#!/bin/sh
set -ex

# Do not shallow clone because gitversion needs history!
git clone -b $BUILD_FROM_BRANCH "https://github.com/$REPOSITORY.git" source
cd source

dotnet tool install -g powershell
echo $PATH
ls -al /usr/bin

pwsh ./ci/Publish.ps1 $runtime
cp ./publish/$runtime/recyclarr ..
