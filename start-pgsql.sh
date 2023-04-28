#!/bin/sh

username=wr_dualstack
password=T2ie7yd1aHBw6MImg02e2tJdKmU9xeUGftaB5sN3ZGhMUdq2qI
db_name=wr_dualstack

docker run --pull always \
-e POSTGRES_USER=${username} \
-e POSTGRES_PASSWORD=${password} \
-e POSTGRES_DB=${db_name} \
-p 5432:5432 \
--name dualstack-pgsql \
-v dualstack-pgsql-db-vol:/var/lib/postgresql/data \
-d postgres:10