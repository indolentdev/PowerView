#!/bin/bash

echo Building and testing backend and web
sh ./assemble.sh
if [ $? -ne 0 ]
then
  exit 1
fi

#dotnet list package --outdated

echo Running
dotnet build/PowerView.dll
