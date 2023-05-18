#!/bin/sh

# region env
export ASPNETCORE_URLS="http://+:$PORT"
export ASPNETCORE_ENVIRONMENT="Production"
export TZ="Asia/Shanghai"
# endregion env

dotnet WebScreenshot.dll