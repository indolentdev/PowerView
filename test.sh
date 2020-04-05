#!/bin/bash
mono Deps/nuget.exe restore
msbuild PowerView.sln /p:Configuration=Debug
if [ $? -ne 0 ]
then
  exit 1
fi
mono packages/NUnit.ConsoleRunner.3.11.1/tools/nunit3-console.exe PowerView.Model.Test/bin/Debug/PowerView.Model.Test.dll PowerView.Service.Test/bin/Debug/PowerView.Service.Test.dll PowerView.Test/bin/Debug/PowerView.Test.dll
