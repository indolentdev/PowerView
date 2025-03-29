#!/bin/bash

echo Building and testing backend
cd PowerView-Backend
sh ./release.sh
if [ $? -ne 0 ]
then
  echo Backend release.sh returned $?
  cd ..
  exit 1
fi
cd ..

echo Building and testing web
cd PowerView-Web
sh ./release.sh
if [ $? -ne 0 ]
then
  echo Web release.sh returned $?
  cd ..
  exit 1
fi
cd ..

echo Cleaning build folder
if [ -d "build" ]
then
  rm -r "build"
fi

mkdir build

echo Copying backend
cp -r "PowerView-Backend/PowerView/bin/Release/net8.0/publish/." "build"
if [ $? -ne 0 ]
then
  exit 1
fi

echo Copying web
cp -r "PowerView-Web/dist/PowerView-Web/browser" "build/PowerView-Web"
if [ $? -ne 0 ]
then
  exit 1
fi
