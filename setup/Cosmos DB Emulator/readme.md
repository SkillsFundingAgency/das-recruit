# Azure Cosmos DB Emulator setup

## Introduction

This PowerShell script `Set-CosmosDbEmulator.ps1` creates/updates your local Cosmos DB Emulator with the database and collections defined in [Configuration.json](https://github.com/SkillsFundingAgency/das-recruit/blob/master/src/Data/CosmosDb/Configuration.json).

This script is a modified version of [Set-CosmosDbAccountComponents.ps1](https://github.com/SkillsFundingAgency/devops-automation/blob/master/Infrastructure/Resources/Set-CosmosDbAccountComponents.ps1)

There is a dependency on [CosmosDB PowerShell Module](https://github.com/PlagueHO/CosmosDB) which will be automatically installed if not present.

## How to Use
Ensure CosmosDB Emulator is running and execute `.\Set-CosmosDbEmulator.ps1` from PowerShell.

If any databases/Collections are missing then they will be created.

Any Indexes will be created / re-created.

Stored procedures will be created or updated

## Note
[Use the Azure Cosmos DB Emulator for local development and testing](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator#differences-between-the-emulator-and-the-service) mentions by default the total maximum number of parition collections is defaulted to 1 and you should use `/PatitionCount=xxx` to increase it.  I'm using Cosmos DB Emulator version 1.22.0.0 and have not needed to do this.



