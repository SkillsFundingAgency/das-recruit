# Table of Contents
* [Data Seeder Tool](#seedDataWriterTool)
* [Writing Change Migration Scripts](#authoringChangeMigrationScripts)
* [Archiving Change Migration Scripts](#archivingChangeMigrationScripts)
* [Running Change Migration Scripts](#runningChangeMigrationScripts)

&nbsp;

<a id="seedDataWriterTool"></a>
# :leaves: Seed Data Writer Tool :rocket:

## Introduction

This output of the `Console.RecruitSeedDataWriter` project can be used to load initial data into the Recruit document database used by the following applications:
- [Employer Recruit](https://github.com/SkillsFundingAgency/das-recruit/tree/master/src/Employer/Employer.Web)
- [Provider Recruit](https://github.com/SkillsFundingAgency/das-recruit/tree/master/src/Provider/Provider.Web)
- [QA Recruit](https://github.com/SkillsFundingAgency/das-recruit/tree/master/src/QA/QA.Web)
- [Recruit API](https://github.com/SkillsFundingAgency/das-recruit-api)
- Vacancy API 1.0 (TBC)

## Project Setup

The basic principle behind this repository is to store a copy of the reference data that needs to be loaded into the Recruit database (any other Mongo API supported document database can be targetted).

## Prerequisites

- DotNet Core
- A target document database created in MongoDB :leaves: or Azure CosmosDB :rocket:.

## Running Locally

The project can be opened within Visual Studio or VS Code. Once built, from the folder containing the built binaries `/bin/{Configuration}/netcoreapp2.0/` you can run the tool in the following two ways:
1. `./SeedScript.ps1 -$dbConnectionString {DatabaseConnectionStringUrl}` from a Powershell prompt. Will attempt to load all existing data document scripts specified inside `SeedScript.ps1` into the target database.
2. `dotnet Console.RecruitSeedDataWriter.dll {DatabaseConnectionStringUrl} -c {collectionName} -f {jsonFilePath}` to load a single document into a target collection.

## Deployment

The project can be published by running `dotnet publish` in the project directory.

&nbsp;

<a id="authoringChangeMigrationScripts"></a>
# Writing change migration scripts :leaves: :scroll:

## Prerequisites

- A target document database created in MongoDB :leaves: or Azure CosmosDB :rocket:.
- Mongo shell (Pointing at your target database server.)
- NodeJS 6.0+ (Optional)
- ESLint (Optionally already installed globally)

## Instructions

1. Create new script following the convention used for existing files which are number ordered by prefixing the filename with a three digit sequence e.g. 010.
2. Add a call to the new script from the **_documentMigration.js_** file by adding a `load("{filename.js}");` line.

   **If you do not have ESLint installed globally, then run the following step in a command shell, otherwise skip to step 4.**
3. From the 'Recruit.ChangeScripts' directory in a command shell, run `npm install`. If you do not already have ESLint installed globally.
4. In a command shell in the same directory, run `npm run lint` to check the script-Java is written in a standard format.

&nbsp;

<a id="archivingChangeMigrationScripts"></a>
# Archiving change migration scripts :leaves: :scroll:

Any scripts that are no longer required to be run during release should be moved to the `archived` folder. The filename should be prefixed with the date that the script is no longer to be used in the format of `yyyy-mm-dd_`.

&nbsp;

<a id="runningChangeMigrationScripts"></a>
# Running change migration scripts locally in Mongo shell :leaves: :shell:

## Prerequisites

- A target document database created in MongoDB :leaves: or Azure CosmosDB :rocket:.
- Mongo shell (Pointing at your target database server.)

> If you need to connect to the Cosmos DB Emulator using Mongo Shell use this command
`mongo recruit --port 10255 --ssl --authenticationDatabase admin -u localhost -p C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==`

## Instructions

1. From the Mongo shell you can navigate to the directory that holds the **_documentMigration.js_** and child scripts using `cd("../../../dev/das-recruit/src/Data/Recruit.ChangeScipts")` assuming you have cloned the repository into the dev folder of your root dir. You can use `pwd()` to print your working directory to help you navigate.
2. Enter the command `load("documentMigration.js")` and press <kbd>Enter</kbd>
3. You should see output relating to the changes included in the change migration script(s).
