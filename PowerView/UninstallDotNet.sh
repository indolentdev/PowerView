#!/bin/bash

echo -e "\e[1m----------------------------------------"
echo -e "\e[1m      ASP.NET Runtime 6 Uninstaller"
echo -e "\e[1m----------------------------------------"
echo ""

if [[ $EUID -ne 0 ]]; then
   echo -e "\e[1;31mThis script must be run as root (sudo $0)" 
   exit 1
fi


aspnetfile=/tmp/aspnetcore.tar.gz
rm -f $aspnetfile


echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m       Unlink dotnet binary"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"
rm /usr/local/bin/dotnet
if [ $? -ne 0 ]
then
  echo Remove dotnet link failed. Aborting. 
  exit 1
fi

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m     Deleting dotnet main directory"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

rm -rf /opt/dotnet
if [ $? -ne 0 ]
then
  echo Delete main directory failed. Aborting. 
  exit 1
fi

echo Dotnet uninstall complete

