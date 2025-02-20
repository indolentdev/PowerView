#!/bin/bash
dotnet test PowerView.sln -c Debug
if [ $? -eq 0 ]
then
  echo "Success: All tests passed"
  exit 0
else
  echo "Failure: One or more tests failed"
  exit 1
fi
