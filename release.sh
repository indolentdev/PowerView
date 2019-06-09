#!/bin/bash
msbuild PowerView.sln /p:Configuration=Release /t:Clean
if [ $? -ne 0 ]
then
  exit 1
fi

msbuild PowerView.sln /p:Configuration=Debug /t:Clean
if [ $? -ne 0 ]
then
  exit 1
fi

if [ -d "PowerView/obj/" ]; then
  rm -r PowerView/obj/
fi
if [ -d "PowerView/obj/" ]; then
  rm -r PowerView/obj/ 
fi

sh ./test.sh
if [ $? -ne 0 ]
then
  exit 1
fi

msbuild PowerView.sln /p:Configuration=Release /t:Build
if [ $? -ne 0 ]
then
  exit 1
fi

perl -p -e 's/\n/\r\n/' < PowerView/bin/Release/PowerView.exe.config > PowerView/bin/Release/PowerView.exe.config_win
if [ $? -ne 0 ]
then
  exit 1
fi
rm PowerView/bin/Release/PowerView.exe.config
if [ $? -ne 0 ]
then
  exit 1
fi
mv PowerView/bin/Release/PowerView.exe.config_win PowerView/bin/Release/PowerView.exe.config
if [ $? -ne 0 ]
then
  exit 1
fi
