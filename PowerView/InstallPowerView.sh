#!/bin/bash

echo Install PowerView in progress

# Check for permissions
if [ "$(id -u)" != "0" ] 
then
   echo This script must be run as root. Aborting install.
   exit 1
fi

dotnet=$(dotnet --info)
if [ $? -ne 0 ]
then
  echo dotnet is not installed or not accessible. Aborting install. 
  exit 1
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
  echo This upgrade will uninstall PowerView and delete the 
  echo /opt/PowerView directory. A new directory will be created from scratch.
  echo The /var/lib/PowerView database directory will be retained.
  echo A new powervw user will become the owner of /opt/PowerView and
  echo /var/lib/PowerView. 
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

LOGDIR=/var/log/PowerView
if [ ! -d $LOGDIR ]
then
  echo Setting up log directory $DBDIR
  $(mkdir $LOGDIR)
else
  echo Reusing existing log directory $LOGDIR
fi
# Setting db directory owner and permissions
chown -R $user:$user $LOGDIR
chmod -R 755 $LOGDIR

# Setup DB dir
DBDIR=/var/lib/PowerView
if [ ! -d $DBDIR ]
then
  echo Setting up db directory $DBDIR
  $(mkdir $DBDIR)
else
  echo Reusing existing db directory $DBDIR
fi
# Setting db directory owner and permissions
chown -R $user:$user $DBDIR
chmod -R 755 $DBDIR


# Copy files to bin dir and set permissions
echo Setting up application directory $BINDIR
if [ ! -d $BINDIR ]
then
  mkdir $BINDIR
else
  echo Upgrading existing PowerView install
  rm $BINDIR/*.dll
  rm $BINDIR/*.sh
  rm $BINDIR/PowerView*.json
  rm $BINDIR/web.config
#  rm -rf $BINDIR/Doc
  rm -rf $BINDIR/da
  rm -rf $BINDIR/PowerView-Web
  rm -rf $BINDIR/runtimes
fi

cp *.dll $BINDIR
cp St*PowerView.sh $BINDIR
cp Un*.sh $BINDIR
cp PowerView*.json $BINDIR
cp web.config $BINDIR
#cp -r Doc $BINDIR/Doc
cp -r da $BINDIR/da
cp -r PowerView-Web $BINDIR/PowerView-Web
cp -r runtimes $BINDIR/runtimes

if [ ! -f $BINDIR/appsettings.json ]
then
  cp appsettings.json $BINDIR
fi
if [ ! -f $BINDIR/NLog.config ]
then
  cp NLog.config $BINDIR
fi

chown -R $user:$user $BINDIR
chmod -R 755 $BINDIR
chmod 775 $BINDIR/*.sh

echo Registering service

cp powerview.service /etc/systemd/system
systemctl daemon-reload
systemctl enable powerview.service
if [ $? -ne 0 ]
then
  echo Serivce registration failed. Aborting install. 
fi

echo Starting PowerView service

./StartPowerView.sh
if [ $? -ne 0 ]
then
  echo Service failed to start 
fi


echo Install complete
echo Check http://localhost:47362

