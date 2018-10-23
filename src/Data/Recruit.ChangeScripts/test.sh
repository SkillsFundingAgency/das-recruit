#!/bin/bash

#mongo recruit --authenticationDatabase admin -u dbadmin -p changeme --eval 'load("./documentMigration.js")'

mongo --authenticationDatabase admin -u lee-mongo -p pYdeByhd7buDgvENAvqL2e51f7PWMYFCSkOCBoNVEznAdBSohSl2mjIkgqYt2zpu31Z4AzLPCYTZvowdAVP5jw== --ssl lee-mongo.documents.azure.com:10255/lee-db --eval 'load("./documentMigration.js")'