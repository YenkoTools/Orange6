#!/bin/bash

cd "$(dirname "$0")" && dotnet clean && dotnet restore && cd src/Api && func host start
