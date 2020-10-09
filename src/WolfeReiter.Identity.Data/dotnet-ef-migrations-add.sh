#!/bin/sh

cwd=$(dirname $0)

cd $cwd
dotnet ef migrations add "$1" --context PgSqlContext --output-dir Migrations/PgSqlMigrations
dotnet ef migrations add "$1" --context SqlServerContext --output-dir Migrations/SqlServerMigrations