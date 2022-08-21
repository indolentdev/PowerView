#!/bin/bash
if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root (sudo $0)"
   exit 1
fi

sudo systemctl start powerview2.service
