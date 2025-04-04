#!/bin/bash

echo -e "\e[1m----------------------------------------"
echo -e "\e[1m       ASP.NET Runtime 8 Installer"
echo -e "\e[1m----------------------------------------"
echo ""
echo -e "\e[1mInstall scipt made based on work of Pete Codes / PJG Creations 2021"
echo -e "\e[1mhttps://github.com/pjgpetecodes/dotnet6pi/blob/master/install.sh"
echo ""
: '
MIT License

Copyright (c) 2020 Peter Gallagher

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
'

if [[ $EUID -ne 0 ]]; then
   echo -e "\e[1;31mThis script must be run as root (sudo $0)" 
   exit 1
fi


cpuinfo=$(cat /proc/cpuinfo)
if [[ $cpuinfo == *"ARMv6"* ]]; then
  echo ".NET does not work on ARM v6 processors"
  echo ".NET requires at least ARM v7 processor (e.g. Raspberry Pi 2 Model B)"
  echo "Aborting install."
  exit 1
fi

#echo -e "\e[0m"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[1m         Fetching .NET Versions"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"


dotnetver=8.0

#sdkfile=/tmp/dotnetsdk.tar.gz
aspnetfile=/tmp/aspnetcore.tar.gz

download() {
    [[ $downloadspage =~ $1 ]]
    linkpage=$(wget -qO - https://dotnet.microsoft.com${BASH_REMATCH[1]})

    matchdl='id="directLink" href="([^"]*)"'
    [[ $linkpage =~ $matchdl ]]
    wget -O $2 "${BASH_REMATCH[1]}"
}

detectArch() {
    arch=arm32

    if command -v uname > /dev/null; then
        machineCpu=$(uname -m)-$(uname -p)

        if [[ $machineCpu == *64* ]]; then
            arch=arm64
        fi
    fi
}

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m        Installation information"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

#echo ""
#echo "This will install the latest versions of the following:"
#echo ""
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"
#echo "- .NET SDK $dotnetver"
echo "- ASP.NET Runtime $dotnetver"
echo ""
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"
#echo -e "Any suggestions or questions, email \e[1;4mpete@pjgcreations.co.uk"
#echo -e "\e[0mSend me a tweet \e[1;4m@pete_codes"
#echo -e "\e[0mTutorials on \e[1;4mhttps://www.petecodes.co.uk"
#echo ""
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"


echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m         Installing Dependencies"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

apt-get -y install libunwind8 gettext

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m           Remove Old Binaries"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

#rm -f $sdkfile
rm -f $aspnetfile

#echo -e "\e[0m"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[1m        Getting .NET SDK $dotnetver"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"

[[ "$dotnetver" > "5" ]] && dotnettype="dotnet" || dotnettype="dotnet-core"
downloadspage=$(wget -qO - https://dotnet.microsoft.com/download/$dotnettype/$dotnetver)

detectArch

#download 'href="([^"]*sdk-[^"/]*linux-'$arch'-binaries)"' $sdkfile

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m       Getting ASP.NET Runtime $dotnetver"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

download 'href="([^"]*aspnetcore-[^"/]*linux-'$arch'-binaries)"' $aspnetfile

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m       Creating Main Directory"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

if [[ -d /opt/dotnet ]]; then
    echo "/opt/dotnet already  exists on your filesystem."
else
    echo "Creating Main Directory"
    echo ""
    mkdir /opt/dotnet
fi

#echo -e "\e[0m"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[1m    Extracting .NET SDK $dotnetver"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"

#tar -xvf $sdkfile -C /opt/dotnet/

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m    Extracting ASP.NET Runtime $dotnetver"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

tar -xvf $aspnetfile -C /opt/dotnet/

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m    Link Binaries to User Profile"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

ln -s /opt/dotnet/dotnet /usr/local/bin

#echo -e "\e[0m"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[1m    Make Link Permanent"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"

#if grep -q 'export DOTNET_ROOT=' /home/pi/.bashrc;  then
#  echo 'Already added link to .bashrc'
#else
#  echo 'Adding Link to .bashrc'
#  echo 'export DOTNET_ROOT=/opt/dotnet' >> /home/pi/.bashrc
#fi

#echo -e "\e[0m"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[1m         Download Debug Stub"
#echo -e "\e[1m----------------------------------------"
#echo -e "\e[0m"

#cd ~

#wget -O /home/pi/dotnetdebug.sh https://raw.githubusercontent.com/pjgpetecodes/dotnet6pi/master/dotnetdebug.sh
#chmod +x /home/pi/dotnetdebug.sh 

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m          Run dotnet --info"
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"

dotnet --info

echo -e "\e[0m"
echo -e "\e[1m----------------------------------------"
echo -e "\e[1m              ALL DONE!"
#echo ""
#echo -e "\e[1mNote: It's highly recommended that you perform a reboot at this point!"
#echo ""
#echo -e "\e[0mGo ahead and run \e[1mdotnet new console \e[0min a new directory!"
#echo ""
#echo ""
#echo ""
#echo -e "\e[0mLet me know how you get on by tweeting me at \e[1;5m@pete_codes\e[0m"
#echo ""
echo -e "\e[1m----------------------------------------"
echo -e "\e[0m"
