# :leaves: Seed Data Writer Tool :rocket: #

## Introduction ##

This output of the `Console.RecruitSeedDataWriter` project can be used to load initial data into the Recruit document database used by the following applications:
- Employer Recruit
- QA Recruit
- Provider Recruit (TBC)
- Vacancy API 1.0 (TBC)

## Project Setup ##

The basic principle behind this repository is to store a copy of the reference data that needs to be loaded into the Recruit database (any other document database can be targetted).

## Prerequisites ##

- DotNet Core
- A target document database created in MongoDB :leaves: or Azure CosmosDB :rocket:.

## Running Locally ##

The project can be opened within Visual Studio or VS Code. Once built, from the folder containing the built binaries `/bin/{Configuration}/netcoreapp2.0/` you can run the tool in the following two ways:
1. `./SeedScript.ps1 -$dbConnectionString {DatabaseConnectionStringUrl}` from a Powershell prompt. Will attempt to load all existing data document scripts specified inside `SeedScript.ps1` into the target database.
2. `dotnet Console.RecruitSeedDataWriter.dll {DatabaseConnectionStringUrl} -c {collectionName} -f {jsonFilePath}` to load a single document into a target collection.

## Deployment ##

The project can be published by running `dotnet publish` in the project directory.

&nbsp;

# Running migration scripts locally in Mongo shell :leaves: :shell:

## Prerequisites ##

- Mongo shell
- A target document database created in MongoDB :leaves: or Azure CosmosDB :rocket:.


## Instructions ##

1. From the Mongo shell you can navigate to the directory that holds the `databaseMigration.js` and child scripts using `cd("../../../dev/das-recruit/src/data/CosmosDb")` assuming you have cloned the repository into the dev folder of your root dir. You can use `pwd()` to print your working directory to help you navigate.

2. Enter the command `load("documentMigration.js")` and press `Enter`
3. You should see output relating to the chnages including the migration script.