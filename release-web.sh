#!/bin/bash

sh ./test-web.sh
if [ $? -ne 0 ]
then
  exit 1
fi

cd PowerView-Web
ng build --configuration production --aot --output-hashing=all --base-href /web/
if [ $? -ne 0 ]
then
  cd ..
  exit 1
fi

cd ..
