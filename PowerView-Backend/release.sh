#!/bin/bash

dotnet clean PowerView.sln -c Release
retVal=$?
if [ $retVal -ne 0 ]; then
  echo dotnet clean Release returned $retVal
  exit 1
fi

dotnet clean PowerView.sln -c Debug
retVal=$?
if [ $retVal -ne 0 ]; then
  echo dotnet clean Debug returned $retVal
  exit 1
fi

if [ -d "PowerView/obj/" ]; then
  rm -r PowerView/obj/
fi
if [ -d "PowerView/bin/" ]; then
  rm -r PowerView/bin/ 
fi

sh ./test.sh
retVal=$?
if [ $retVal -ne 0 ]; then
  echo test.sh returned $retVal
  exit 1
fi

dotnet publish PowerView.sln -c Release -p:UseAppHost=false
retVal=$?
if [ $retVal -ne 0 ]; then
  echo dotnet publish returned $retVal
  exit 1
fi

perl -p -e 's/\n/\r\n/' < PowerView/bin/Release/net6.0/publish/appsettings.json > PowerView/bin/Release/net6.0/publish/appsettings.json_win
retVal=$?
if [ $retVal -ne 0 ]; then
  echo appsettings.json fiddling 1/3 returned $retVal
  exit 1
fi
rm PowerView/bin/Release/net6.0/publish/appsettings.json
retVal=$?
if [ $retVal -ne 0 ]; then
  echo appsettings.json fiddling 2/3 returned $retVal
  exit 1
fi
mv PowerView/bin/Release/net6.0/publish/appsettings.json_win PowerView/bin/Release/net6.0/publish/appsettings.json
retVal=$?
if [ $retVal -ne 0 ]; then
  echo appsettings.json fiddling 3/3 returned $retVal
  exit 1
fi

perl -p -e 's/\n/\r\n/' < PowerView/bin/Release/net6.0/publish/NLog.config > PowerView/bin/Release/net6.0/publish/NLog.config_win
retVal=$?
if [ $retVal -ne 0 ]; then
  echo NLog.config fiddling 1/3 returned $retVal
  exit 1
fi
rm PowerView/bin/Release/net6.0/publish/NLog.config
retVal=$?
if [ $retVal -ne 0 ]; then
  echo NLog.config fiddling 2/3 returned $retVal
  exit 1
fi
mv PowerView/bin/Release/net6.0/publish/NLog.config_win PowerView/bin/Release/net6.0/publish/NLog.config
retVal=$?
if [ $retVal -ne 0 ]; then
  echo NLog.config fiddling 3/3 returned $retVal
  exit 1
fi
