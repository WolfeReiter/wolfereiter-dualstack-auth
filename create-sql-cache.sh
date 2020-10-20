#!/bin/sh

docker exec -it dualstack-mssqlserver /opt/mssql-tools/bin/sqlcmd \
-S localhost -U sa -P "YkBkKCta6zNhlp6buYMAf6tmjjSl4WkIj0R8BfrCXA1zjjQ4PM" \
-Q "IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'WR_DUALSTACK')
    BEGIN
    create database WR_DUALSTACK
    END"

dotnet sql-cache create \
"Server=localhost;Initial Catalog=WR_DUALSTACK;User Id=sa;Password=YkBkKCta6zNhlp6buYMAf6tmjjSl4WkIj0R8BfrCXA1zjjQ4PM" \
dbo Cache
