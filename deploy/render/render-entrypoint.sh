#!/bin/sh

# region env
export ASPNETCORE_URLS="http://+:5000"
export ASPNETCORE_ENVIRONMENT="Production"
export TZ="Asia/Shanghai"
# endregion env

dotnet WebScreenshot.dll