#!/bin/sh

cwd=$(dirname $0)

cd $cwd
dotnet ef migrations remove --context PgSqlContext
dotnet ef migrations remove --context SqlServerContext