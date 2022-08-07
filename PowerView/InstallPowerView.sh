#!/bin/bash

echo Install PowerView in progress

# Check for permissions
if [ "$(id -u)" != "0" ] 
then
   echo This script must be run as root. Aborting install.
   exit 1
fi

dotnet=$(dotnet --version)
if [ $? -ne 0 ]
then
  echo dotnet is not installed or not accessible. Aborting install. 
fi

BINDIR=/opt/PowerView

# Uninstall v0.0.x PowerView
LEGACYBIN=$BINDIR/PowerView.exe
if [ -f $LEGACYBIN ]
then
  # Check for running the process
  PID=$(pgrep -d " " -f .*mono.*PowerView.exe.*)
  if [ -n "$PID" ]
  then
    echo PowerView v0.0.x is running. Stop it first using "sudo /opt/PowerView/StopPowerView.sh"
    exit 1
  fi
  
  echo PowerView v0.0.x is installed. 
  echo This upgrade will first uninstall then delete the echo /opt/PowerView
  echo directory and a new directory will be created from scratch.
  echo The /var/lib/PowerView directory will be retained.
  read -p "Do you want to proceed? (yes/no) " yn

  case $yn in 
	yes ) echo ok, proceeding;;
	no ) echo exiting;
		exit;;
 	* ) echo invalid response;
		exit 1;;
  esac

  # Remove from /etc/init.d
  INITDDIR=/etc/init.d
  POWERVIEWINITD=$INITDDIR/PowerView
  INSSERV=/sbin/insserv
  if [ -f $POWERVIEWINITD ]
  then
    echo Cleaning up init.d
    if [ ! -f $INSSERV ]
    then
      $(rm /etc/rc0.d/K20PowerView)
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

  echo Deleting $BINDIR  
  rm -rf $BINDIR  
fi

# Check PowerView isn't running
PID=$(pgrep -d " " -f .*dotnet.*PowerView.dll.*)
if [ -n "$PID" ]
then
  echo PowerView is running. Stop it first using "sudo /opt/PowerView/StopPowerView.sh"
  exit 1
fi

# Crate powervw user and group
user=powervw
id -u $user >/dev/null 2>&1
if [ "$?" == "0" ] 
then
  echo User $user already exists
else
  echo Creating user $user
  useradd -r -U $user
  if [ $? -ne 0 ]
  then
    echo Create user $user failed. Aborting install.
    exit 1
  fi
fi

if id --name --groups --zero "$user" | grep --quiet --null-data --line-regexp --fixed-strings "$user"
then
    echo User $user belongs to group $user
else
    echo User $user does not belong to group $user. Aborting install.
    exit 1
fi

# Add pi user to powervw group
pi=pi
id -u $pi >/dev/null 2>&1
if [ "$?" == "0" ] 
then
  if id --name --groups --zero "$pi" | grep --quiet --null-data --line-regexp --fixed-strings "$user"
  then
    echo User $pi belongs to group $user
  else
    echo Adding $pi to $user group. Effective next login.
    usermod -a -G $user $pi
  fi
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

echo Setting db directory owner and permissions
chown -R $user:$user $DBDIR
chmod -R 660 $DBDIR


# Copy files to bin dir and set permissions
echo Setting up application directory $BINDIR
if [ ! -d $BINDIR ]
then
  $(mkdir $BINDIR)
  
else
  echo Upgrading existing PowerView install
  $(rm $BINDIR/*.dll)
  $(rm $BINDIR/*.sh)
  $(rm $BINDIR/PowerView*.json)
  $(rm $BINDIR/web.config)
#  $(rm -rf $BINDIR/Doc)
  $(rm -rf $BINDIR/da)
  $(rm -rf $BINDIR/PowerView-Web)  
  $(rm -rf $BINDIR/runtimes)
fi

$(cp *.dll $BINDIR)
$(cp *.exe $BINDIR)
$(cp St*PowerView.sh $BINDIR)
$(cp Un*.sh $BINDIR)
$(cp PowerView*.json $BINDIR)
$(cp web.config $BINDIR)
#$(cp -r Doc $BINDIR/Doc)
$(cp -r da $BINDIR/da)
$(cp -r PowerView-Web $BINDIR/PowerView-Web)
$(cp -r runtimes $BINDIR/runtimes)

if [ ! -f $BINDIR/appsettings.json ]
then
  $(cp appsettings.json $BINDIR)
fi

if [ ! -f $BINDIR/NLog.config ]
then
  $(cp NLog.config $BINDIR)
fi

chown -R $user:$user $BINDIR
$(chmod 664 $BINDIR/*.dll)
$(chmod 775 $BINDIR/*.sh)


# /etc/systemd/system
# sudo systemctl start alarm
# sudo systemctl status tempMonitor.service
# sudo systemctl enable tempMonitor.service
# systemctl daemon-reload

: '

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
    $(rm /etc/rc0.d/K20PowerView)
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



# Copy to init.d
echo Setting up init.d
$(cp PowerView $INITDDIR)
$(chmod 755 $POWERVIEWINITD)
if [ -f $INSSERV ]
then
  $($INSSERV PowerView)
else
  $(ln -s $POWERVIEWINITD /etc/rc0.d/K20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc1.d/K20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc6.d/K20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc2.d/S20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc3.d/S20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc4.d/S20PowerView)
  $(ln -s $POWERVIEWINITD /etc/rc5.d/S20PowerView)
fi

'

echo Install complete
echo Reboot to launch PowerView as part of startup
