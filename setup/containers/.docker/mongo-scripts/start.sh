#!/usr/bin/env bash

if [[ -e /.runonce ]]; then
    echo "Kicking off entrypoint.sh script"
    /mongo-scripts/entrypoint.sh

   echo "Kicking off primer.sh script"
    /mongo-scripts/primer.sh
fi

# Start MongoDB
echo "Starting MongoDB server Mongod"
/usr/bin/mongod --auth --bind_ip_all $@