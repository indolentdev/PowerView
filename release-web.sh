#!/bin/bash

sh ./test-web.sh
if [ $? -ne 0 ]
then
  exit 1
fi

cd PowerView-Web
ng build --prod --base-href /web --deploy-url /web/
if [ $? -ne 0 ]
then
  cd ..
  exit 1
fi

cd ..
