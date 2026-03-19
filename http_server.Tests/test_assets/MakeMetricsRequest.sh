#!/bin/bash

curl --cacert ca.pem \
     --cert client.pem \
     --key client.key \
     --raw \
     --http2 \
     --trace-ascii - \
     --trace-time \
     https://localhost:8080/metrics
