#!/bin/sh

password=YkBkKCta6zNhlp6buYMAf6tmjjSl4WkIj0R8BfrCXA1zjjQ4PM

docker run --rm \
-e 'ACCEPT_EULA=Y' \
-e "MSSQL_SA_PASSWORD=${password}" \
--name dualstack-mssqlserver \
-p 1433:1433 -v dualstack-mssql-db-vol:/var/opt/mssql \
-d mcr.microsoft.com/mssql/server:2019-latest

