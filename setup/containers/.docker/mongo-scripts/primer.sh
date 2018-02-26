#!/bin/bash

ROOT_USER=${MONGODB_ROOT_USERNAME}
ROOT_PASS=${MONGODB_ROOT_PASSWORD}
ROOT_DB="admin"
ROOT_ROLE=${MONGODB_ROOT_ROLE}

# MODIFY_USER=${MONGODB_READWRITE_USERNAME}
# MODIFY_PASS=${MONGODB_READWRITE_PASSWORD}
# MODIFY_DB=${MONGODB_DBNAME}
# MODIFY_ROLE=${MONGODB_READWRITE_ROLE}

# QUERY_USER=${MONGODB_READ_USERNAME}
# QUERY_PASS=${MONGODB_READ_PASSWORD}
# QUERY_DB=${MONGODB_DBNAME}
# QUERY_ROLE=${MONGODB_READ_ROLE}

# Start Mongod
echo "Starting Mongod"
echo "....waiting for Mongod"
/usr/bin/mongod &
while ! nc -vz localhost 27017; do sleep 1; done

# Create Root User
if [[ -n $ROOT_ROLE ]] && [[ -n $ROOT_PASS ]] && [[ -n $ROOT_USER ]]
then
	echo "Creating the root user: \"$ROOT_USER\"..."
	mongo $ROOT_DB --eval "db.createUser({ user: '$ROOT_USER', pwd: '$ROOT_PASS', roles: [ { role: '$ROOT_ROLE', db: '$ROOT_DB' } ] });"
fi

# Create user with read/write privileges
# if [[ -n $MODIFY_ROLE ]] && [[ -n $MODIFY_DB ]] && [[ -n $MODIFY_USER ]] && [[ -n $MODIFY_PASS ]]
# then
# 	echo "Creating modify user: \"$MODIFY_USER\"..."
# 	mongo $MODIFY_DB --eval "db.createUser({ user: '$MODIFY_USER', pwd: '$MODIFY_PASS', roles: [ { role: '$MODIFY_ROLE', db: '$MODIFY_DB' } ] });"
# fi

# Create user with read only privileges
# if [[ -n $QUERY_ROLE ]] && [[ -n $QUERY_DB ]] && [[ -n $QUERY_USER ]] && [[ -n $QUERY_PASS ]]
# then
# 	echo "Creating query user: \"$QUERY_USER\"..."
# 	mongo $QUERY_DB --eval "db.createUser({ user: '$QUERY_USER', pwd: '$QUERY_PASS', roles: [ { role: '$QUERY_ROLE', db: '$QUERY_DB' } ] });"
# fi

# Stop MongoDB service
echo "Shutting down MongoDB after adding users/roles...."
/usr/bin/mongod --shutdown

echo "==============================================================="
echo "MongoDB accounts created: "
echo "USER: $ROOT_USER   | ROLE: $ROOT_ROLE"
# echo "USER: $MODIFY_USER | ROLE: $MODIFY_ROLE | DATABASE: $MODIFY_DB"
# echo "USER: $QUERY_USER  | ROLE: $QUERY_ROLE  | DATABASE: $QUERY_DB"
echo "==============================================================="

export MONGODB_ROOT_PASSWORD=""
export MONGODB_PASSWORD=""

rm -f /.runonce