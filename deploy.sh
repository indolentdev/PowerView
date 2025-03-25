#!/bin/bash

component=PowerView

echo Complete build, test, release and deployment of $component

read -p "Enter remote ip address:" remoteaddress
read -p "Enter username on $remoteaddress:" remoteusername
read -s -p "$remoteusername@$remoteaddress's password:" remotepassword 

# Test the remote address and credentials
echo $'\n' # newline after read -s
echo Testing credentials
sshpass -p $remotepassword ssh $remoteusername@$remoteaddress 'cd'
if [ $? -ne 0 ]
then
  echo Address, username or password not valid.
  exit 1
fi
echo Credentials OK

# Make the build - i.e. compile and test
echo Building and testing backend and web
sh ./assemble.sh
retVal=$?
if [ $retVal -ne 0 ]; then
  echo assemble.sh returned $retVal
  exit 1
fi

source=build/

echo Extracting version number
executable=$source$component.dll
asmversion=$(strings $executable | egrep '[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$')
if [ $? -ne 0 ]
then
  echo strings and egrep failed. Unable to get assembly version. $executable - $asmversion
  exit 1
fi
version=$(echo $asmversion | sed -n 's/\s*\([0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*\)\..*/\1/p')
if [ $? -ne 0 ]
then
  echo sed failed. Unable to get assembly version. $executable - $asmversion - $version
  exit 1
fi

echo Version: $version

name=$component-$version

sourcename=$name
sourcename+=/

# Rename the build output to match the component name and version.
if [ -d "$sourcename" ]; then
  echo Removing existing folder $sourcename
  rm -r $sourcename
fi

echo Moving release output to $sourcename
mv $source $sourcename
if [ $? -ne 0 ]
then
  echo Failed moving into $sourcename
  exit 1
fi

echo Zipping $sourcename
zipname=$name.zip
if [ -f "$zipname" ]
then
  echo Removing existing $zipname
  rm $zipname
fi
zip -r $zipname $name
md5name=$name-md5.txt
if [ -f "$md5name" ]
then
  echo Removing existing $md5name
  rm $md5name
fi
md5sum $zipname > $md5name

# Uncomment next line to exit script after zipping the release
#exit 1

# Prepare the folder on remote system
echo Deleting temporary folder $name on remote system
sshpass -p $remotepassword ssh $remoteusername@$remoteaddress "if [ -d "$name" ]; then rm -r $name; fi"
if [ $? -ne 0 ]
then
  echo Failed deleting remote system temporary folder $name
  exit 1
fi

echo Copying $sourcename to remote system temporary folder
sshpass -p $remotepassword scp -r $sourcename $remoteusername@$remoteaddress:/home/$remoteusername/

# Install and reboot
echo Stopping $component on remote system
sshpass -p $remotepassword ssh $remoteusername@$remoteaddress "cd $name; sudo ./Stop$component.sh"
if [ $? -ne 0 ]
then
  echo Failed stopping $component on remote system
  exit 1
fi
echo Installing $name on remote system
sleep 5s
sshpass -p $remotepassword ssh $remoteusername@$remoteaddress "cd $name; sudo ./Install$component.sh"
if [ $? -ne 0 ]
then
  echo Failed installing $name on remote system
  exit 1
fi

echo Done. Script ran to completion.
