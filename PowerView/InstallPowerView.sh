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

# Setup DB dir
DBDIR=/var/lib/PowerView
echo Setting up db directory $DBDIR
if [ ! -d $DBDIR ]
then
  $(mkdir $DBDIR)
else
  echo Reusing existing db directory
fi

# Copy files to bin dir and set permissions
BINDIR=/opt/PowerView
echo Installing to $BINDIR
if [ ! -d $BINDIR ]
then
  $(mkdir $BINDIR)
else
  echo Upgrading existing PowerView install
  $(rm $BINDIR/*.dll)
  $(rm $BINDIR/*.exe)
  $(rm $BINDIR/*.sh)
  $(rm -rf $BINDIR/Doc)
  $(rm -rf $BINDIR/da)
  $(rm -rf $BINDIR/PowerView-Web)  
fi
$(cp *.dll $BINDIR)
$(cp *.exe $BINDIR)
$(cp St*PowerView.sh $BINDIR)
$(cp Un*.sh $BINDIR)
$(cp -r PowerView-Web $BINDIR/PowerView-Web)
$(cp -r Doc $BINDIR/Doc)
$(cp -r da $BINDIR/da)

$(chmod 664 $BINDIR/*.dll)
$(chmod 775 $BINDIR/*.sh)

if [ ! -f $BINDIR/PowerView.exe.config ]
then
  $(cp PowerView.exe.config $BINDIR)
fi

# Copy to init.d
echo Setting up init.d
$(cp PowerView $INITDDIR)
$(chmod 755 $POWERVIEWINITD)
if [ -f $INSSERV ]
then
  $($INSSERV PowerView)
else
  $(ln -s $POWERVIEWINITD /etc/rc0.d/K20PowerView) # couldn't make update-rc.d run properly form the scirpt?!?!
  $(ln -s $POWERVIEWINITD /etc/rc1.d/K20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc6.d/K20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc2.d/S20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc3.d/S20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc4.d/S20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc5.d/S20PowerView)
fi

echo Install complete
echo Reboot to launch PowerView as part of startup
