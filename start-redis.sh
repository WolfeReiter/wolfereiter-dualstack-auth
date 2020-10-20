#!/bin/sh

docker run --rm \
--name dualstack-redis \
-p 6379:6379 \
-d redis:6