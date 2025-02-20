#!/bin/bash

#ng test
#ng test --watch=false
ng test --no-watch --no-progress
if [ $? -ne 0 ]
then
  echo ng test returned $?
  cd ..
  exit 1
fi

