#!/bin/bash

cd PowerView-Web
ng test
#ng test --watch=false
if [ $? -ne 0 ]
then
  cd ..
  exit 1
fi

cd ..
