#!/bin/bash

# Make the build - i.e. compile and test
echo Building and testing server and web application
sh ./assemble.sh
if [ $? -ne 0 ]
then
  exit 1
fi

echo Running
mono PowerView/bin/Release/PowerView.exe
