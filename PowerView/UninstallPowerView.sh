#!/bin/bash

# Check for permissions
if [ "$(id -u)" != "0" ]; then
   echo This script must be run as root
   exit 1
fi

echo Unregistering service

systemctl disable --now powerview.service
if [ $? -ne 0 ]
then
  echo Serivce unregistration failed. Aborting. 
  exit 1
fi

rm /etc/systemd/system/powerview.service
systemctl daemon-reload

echo PowerView service unregistered
echo User powervw remain. Folders remain:
echo /opt/PowerView, /var/lib/PowerView and /var/log/PowerView
echo Uninstall complete
