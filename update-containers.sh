#!/bin/sh

current_userid=$(id -u)
if [ $current_userid -eq 0 ]; then
    echo "$(basename "$0") should be run as a standard user." >&2
    exit 1
fi

printf_color() {
    #BLACK="\033[0;30m"
    #RED="\033[0;31m"
    #GREEN="\033[0;32m"
    #BROWN="\033[0;33m"
    #BLUE="\033[0;34m"
    #PURPLE="\033[0;35m"
    CYAN="\033[0;36m"
    #LIGHT_GRAY="\033[0;37m"
    #DARK_GRAY="\033[1;30m"
    #LIGHT_RED="\033[1;31m"
    #LIGHT_GREEN="\033[1;32m"
    #YELLOW="\033[1;33m"
    #LIGHT_BLUE="\033[1;34m"
    #LIGHT_PURPLE="\033[1;35m"
    #LIGHT_CYAN="\033[1;36m"
    #LIGHT_WHITE="\033[1;37m"
    END="\033[0m"
    
    printf "${CYAN}${1}${END}"
}

printf_color "Updating PostgreSQL 10 container...\n"
docker pull postgres:10
printf_color "Done.\n\n"

printf_color "Updating SQL Server 2019 container...\n"
docker pull mcr.microsoft.com/mssql/server:2019-latest
printf_color "Done.\n\n"

printf_color "Updating redis container...\n"
docker pull redis:6
printf_color "Done.\n\n"

printf_color "Updating dotnet core 3.1 containers...\n"
#docker pull mcr.microsoft.com/dotnet/core/sdk:3.1
#docker pull mcr.microsoft.com/dotnet/core/aspnet:3.1
docker pull mcr.microsoft.com/dotnet/core/sdk:3.1-alpine
docker pull mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
printf_color "Done.\n"
