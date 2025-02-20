#!/bin/bash

#nvm use 22

sh ./test.sh
if [ $? -ne 0 ]
then
  echo test.sh returned $?
  exit 1
fi

ng build --configuration production --aot --output-hashing=all --base-href /web/
if [ $? -ne 0 ]
then
  echo ng build returned $?
  cd ..
  exit 1
fi


