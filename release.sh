#!/bin/bash

version=1.0.0.0

dotnet clean PowerView.sln -c Release
if [ $? -ne 0 ]
then
  exit 1
fi

dotnet clean PowerView.sln -c Debug
if [ $? -ne 0 ]
then
  exit 1
fi

if [ -d "PowerView/obj/" ]; then
  rm -r PowerView/obj/
fi
if [ -d "PowerView/bin/" ]; then
  rm -r PowerView/bin/ 
fi

sh ./test.sh
if [ $? -ne 0 ]
then
  exit 1
fi

dotnet publish PowerView.sln -c Release -p:Version=$version -p:UseAppHost=false
if [ $? -ne 0 ]
then
  exit 1
fi

perl -p -e 's/\n/\r\n/' < PowerView/bin/Release/net6.0/publish/appsettings.json > PowerView/bin/Release/net6.0/publish/appsettings.json_win
if [ $? -ne 0 ]
then
  exit 1
fi
rm PowerView/bin/Release/net6.0/publish/appsettings.json
if [ $? -ne 0 ]
then
  exit 1
fi
mv PowerView/bin/Release/net6.0/publish/appsettings.json_win PowerView/bin/Release/net6.0/publish/appsettings.json
if [ $? -ne 0 ]
then
  exit 1
fi

perl -p -e 's/\n/\r\n/' < PowerView/bin/Release/net6.0/publish/NLog.config > PowerView/bin/Release/net6.0/publish/NLog.config_win
if [ $? -ne 0 ]
then
  exit 1
fi
rm PowerView/bin/Release/net6.0/publish/NLog.config
if [ $? -ne 0 ]
then
  exit 1
fi
mv PowerView/bin/Release/net6.0/publish/NLog.config_win PowerView/bin/Release/net6.0/publish/NLog.config
if [ $? -ne 0 ]
then
  exit 1
fi
