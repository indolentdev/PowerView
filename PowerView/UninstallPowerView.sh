#!/bin/bash

# Check for permissions
if [ "$(id -u)" != "0" ]; then
   echo This script must be run as root
   exit 1
fi

# Check for running the process
PID=$(pgrep -d " " -f .*mono.*PowerView.exe.*)
if [ -n "$PID" ]
then
  echo PowerView is running. Stop it first using "sudo /opt/PowerView/StopPowerView.sh"
  exit 1
fi

# Remove from /etc/init.d
INITDDIR=/etc/init.d
POWERVIEWINITD=$INITDDIR/PowerView
INSSERV=/sbin/insserv
if [ -f $POWERVIEWINITD ]
then
  echo Cleaning up init.d
  if [ ! -f $INSSERV ]
  then
    $(rm /etc/rc0.d/K20PowerView) # couldn't make update-rc.d run properly form the scirpt?!?!
    $(rm /etc/rc1.d/K20PowerView)
    $(rm /etc/rc6.d/K20PowerView)
    $(rm /etc/rc2.d/S20PowerView)
    $(rm /etc/rc3.d/S20PowerView)
    $(rm /etc/rc4.d/S20PowerView)
    $(rm /etc/rc5.d/S20PowerView)
  else
    $($INSSERV -r PowerView)
  fi
  $(rm $POWERVIEWINITD)
fi

echo PowerView will no longer start automatically
echo Files present in /opt/PowerView and /var/lib/PowerView
echo Uninstall complete
