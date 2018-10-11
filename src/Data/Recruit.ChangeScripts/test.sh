#!/bin/bash

mongo recruit --authenticationDatabase admin -u dbadmin -p changeme --eval 'load("./documentMigration.js")'