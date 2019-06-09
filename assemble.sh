#!/bin/bash

# Make the build - i.e. compile and test
echo Building and testing server
sh ./release.sh
if [ $? -ne 0 ]
then
  exit 1
fi

echo Building and testing web application
sh ./release-web.sh
if [ $? -ne 0 ]
then
  exit 1
fi

if [ -d "PowerView/bin/Release/PowerView-Web" ]
then
  rm -r "PowerView/bin/Release/PowerView-Web"
fi

echo Copying web application
cp -r "PowerView-Web/dist/PowerView-Web" "PowerView/bin/Release/PowerView-Web"
if [ $? -ne 0 ]
then
  exit 1
fi
