#!/bin/bash
PID=$(pgrep -d " " -f .*mono.*PowerView.exe.*)
if [ -z "$PID" ]
then
  echo PowerView already stopped
else
  kill -SIGTERM $PID
  echo Stop signal sent to PowerView
fi