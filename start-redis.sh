#!/bin/sh

docker run --pull --rm \
--name dualstack-redis \
-p 6379:6379 \
-d redis:6