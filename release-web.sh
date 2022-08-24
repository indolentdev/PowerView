#!/bin/bash

sh ./test-web.sh
if [ $? -ne 0 ]
then
  echo test-web.sh returned $?
  exit 1
fi

cd PowerView-Web
ng build --configuration production --aot --output-hashing=all --base-href /web/
if [ $? -ne 0 ]
then
  echo ng build returned $?
  cd ..
  exit 1
fi

cd ..
