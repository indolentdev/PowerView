#!/bin/bash

cd PowerView-Web
ng test
#ng test --watch=false
if [ $? -ne 0 ]
then
  echo ng test returned $?
  cd ..
  exit 1
fi

cd ..
