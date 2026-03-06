#!/bin/bash

cd "$(dirname "$0")" && dotnet clean && dotnet restore && cd Api && func host start
