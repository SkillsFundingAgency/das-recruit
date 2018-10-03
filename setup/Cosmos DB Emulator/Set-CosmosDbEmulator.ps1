# This configures your local Cosmos Db Emulator using the Configuration.json
# A modified version of https://github.com/SkillsFundingAgency/devops-automation/blob/master/Infrastructure/Resources/Set-CosmosDbAccountComponents.ps1

$CosmosDbConfigurationFilePath = (Resolve-Path -Path $PSScriptRoot\..\..\src\Data\CosmosDb\Configuration.json).Path

$CosmosDBModuleVersion = "2.1.3.528"

if (!(Get-Module CosmosDB | Where-Object { $_.Version.ToString() -eq $CosmosDBModuleVersion })) {
    Write-Host "Minimum module version is not imported."
    if (!(Get-InstalledModule CosmosDB -MinimumVersion $CosmosDBModuleVersion -ErrorAction SilentlyContinue)) {
        Write-Host "Minimum module version is not installed."
        Install-Module CosmosDB -MinimumVersion $CosmosDBModuleVersion -Scope CurrentUser -Force
    }
    Import-Module CosmosDB -MinimumVersion $CosmosDBModuleVersion
}

Class CosmosDbStoredProcedure {
    [string]$StoredProcedureName
}

Class CosmosDbIndex {
    [string]$kind
    [string]$dataType
    [int32]$precision
}

Class CosmosDbIncludedPath {
    [string]$path
    [CosmosDbIndex[]]$indexes
}

Class CosmosDbExcludedPath {
    [string]$path
}

Class CosmosDbIndexingPolicy {
    [CosmosDbIncludedPath[]]$includedPaths
    [CosmosDbExcludedPath[]]$excludedPaths
    [bool]$automatic
    [string]$indexingMode
}

Class CosmosDbCollection {
    [string]$CollectionName
    [string]$PartitionKey = $null
    [int]$OfferThroughput
    [CosmosDbIndexingPolicy]$IndexingPolicy
    [CosmosDbStoredProcedure[]]$StoredProcedures
    [int]$DefaultTtl
}

Class CosmosDbDatabase {
    [string]$DatabaseName
    [CosmosDbCollection[]]$Collections
}

Class CosmosDbSchema {
    [CosmosDbDatabase[]]$Databases
}

$CosmosDbConfiguration = [CosmosDbSchema](Get-Content $CosmosDbConfigurationFilePath | ConvertFrom-Json)

$CosmosDbContext = New-CosmosDbContext -Emulator

foreach ($Database in $CosmosDbConfiguration.Databases) {
    # --- Create Database
    try {
        $ExistingDatabase = $null
        $ExistingDatabase = Get-CosmosDbDatabase -Context $CosmosDbContext -Id $Database.DatabaseName
    }
    catch {
    }

    if (!$ExistingDatabase) {
        Write-Host "Creating Database: $($Database.DatabaseName)"
        $null = New-CosmosDbDatabase -Context $CosmosDbContext -Id $Database.DatabaseName
    }

    foreach ($Collection in $Database.Collections) {
        # --- Create or Update Collection
        try {
            $ExistingCollection = $null
            $GetCosmosDbDatabaseParameters = @{
                Context  = $CosmosDbContext
                Database = $Database.DatabaseName
                Id       = $Collection.CollectionName
            }
            $ExistingCollection = Get-CosmosDbCollection @GetCosmosDbDatabaseParameters
        }
        catch {
        }

        if (!$ExistingCollection) {

            Write-Host "Creating Collection: $($Collection.CollectionName) in $($Database.DatabaseName)"

            if ($Collection.IndexingPolicy) {
                [CosmosDB.IndexingPolicy.Path.IncludedPath[]]$IndexIncludedPaths = @()
                [CosmosDB.IndexingPolicy.Path.ExcludedPath[]]$IndexExcludedPaths = @()

                foreach ($includedPath in $Collection.IndexingPolicy.includedPaths) {
                    [CosmosDB.IndexingPolicy.Path.Index[]]$IndexRanges = @()

                    foreach ($index in $includedPath.indexes) {
                        if ($index.kind -eq "Spatial") {
                            [CosmosDB.IndexingPolicy.Path.IndexSpatial]$thisIndex = New-CosmosDbCollectionIncludedPathIndex -Kind $index.kind -DataType $index.dataType
                            Write-Host "IncludedPathIndex kind: $($thisIndex.kind) index: $($thisIndex.DataType)"
                        }
                        elseif ($index.kind -eq "Range") {
                            [CosmosDB.IndexingPolicy.Path.IndexRange]$thisIndex = New-CosmosDbCollectionIncludedPathIndex -Kind $index.kind -DataType $index.dataType -Precision $index.precision
                            Write-Host "IncludedPathIndex kind: $($thisIndex.kind) index: $($thisIndex.DataType) precision: $($thisIndex.precision)"
                        }
                        elseif ($index.kind -eq "Hash") {
                            [CosmosDB.IndexingPolicy.Path.IndexHash]$thisIndex = New-CosmosDbCollectionIncludedPathIndex -Kind $index.kind -DataType $index.dataType -Precision $index.precision
                            Write-Host "IncludedPathIndex kind: $($thisIndex.kind) index: $($thisIndex.DataType) precision: $($thisIndex.precision)"
                        }
                        $IndexRanges += $thisIndex
                    }

                    $indexIncludedPath = New-CosmosDbCollectionIncludedPath -Path $includedPath.path -Index $indexRanges
                    $IndexIncludedPaths += $indexIncludedPath

                    Write-Host "indexIncludedPath path: $($includedPath.path.GetType()) index: $($indexRanges.GetType())"
                    Write-Host "indexIncludedPath indexes (object): $($indexRanges)"
                }

                Write-Host "indexIncludedPaths: $($IndexIncludedPaths)"

                foreach ($excludedPath in $Collection.IndexingPolicy.excludedPaths) {
                    $indexExcludedPath = New-CosmosDbCollectionExcludedPath -Path $excludedPath.path
                    $IndexExcludedPaths += $indexExcludedPath

                    Write-Host "indexExcludedPath path: $($excludedPath.path)"
                }

                Write-Host "indexExcludedPaths: $($IndexExcludedPaths)"

                $IndexingPolicy  = New-CosmosDbCollectionIndexingPolicy -Automatic $Collection.IndexingPolicy.automatic -IndexingMode $Collection.IndexingPolicy.indexingMode -IncludedPath $IndexIncludedPaths -ExcludedPath $IndexExcludedPaths -Debug
                Write-Host "Created New-CosmosDbCollectionIndexingPolicy: Automatic: $($IndexingPolicy.Automatic) Mode: $($IndexingPolicy.IndexingMode) IPs: $($IndexIncludedPaths.GetType()) EPs: $($IndexExcludedPaths.GetType())"
                $NewCosmosDbCollectionParameters = @{
                    Context         = $CosmosDbContext
                    Database        = $Database.DatabaseName
                    Id              = $Collection.CollectionName
                    OfferThroughput = $Collection.OfferThroughput
                    IndexingPolicy  = $IndexingPolicy
                }

            }
            elseif (!$Collection.IndexingPolicy) {
                Write-Host "No IndexingPolicy for Collection: $($Collection.CollectionName)"

                $NewCosmosDbCollectionParameters = @{
                    Context         = $CosmosDbContext
                    Database        = $Database.DatabaseName
                    Id              = $Collection.CollectionName
                    OfferThroughput = $Collection.OfferThroughput
                }
            }

            if ($Collection.PartitionKey) {
                if ($PartitionKeyFix.IsPresent) {
                    $NewCosmosDbCollectionParameters.Add('PartitionKey', "'`$v'/$($Collection.PartitionKey)/'`$v'")
                }
                else {
                    $NewCosmosDbCollectionParameters.Add('PartitionKey', $Collection.PartitionKey)
                }
            }

            if ($Collection.DefaultTtl) {
                Write-Host "Add DefaultTtl: $($Collection.DefaultTtl)"
                $NewCosmosDbCollectionParameters += @{
                    DefaultTimeToLive = $Collection.DefaultTtl
                }
            }

            $null = New-CosmosDbCollection @NewCosmosDbCollectionParameters

            Write-Host "Collection Details: Context: Account - $($CosmosDbContext.Account), BaseUri - $($CosmosDbContext.BaseUri); Database: $($Database.DatabaseName); IndexingPolicy: $($IndexingPolicy)"
        }
        else {
            Write-Host "Updating Collection: $($Collection.CollectionName) in $($Database.DatabaseName)"

            # Check for any indexes and update
            if ($Collection.IndexingPolicy) {
                [CosmosDB.IndexingPolicy.Path.IncludedPath[]]$IndexIncludedPaths = @()
                [CosmosDB.IndexingPolicy.Path.ExcludedPath[]]$IndexExcludedPaths = @()

                foreach ($includedPath in $Collection.IndexingPolicy.includedPaths) {
                    [CosmosDB.IndexingPolicy.Path.Index[]]$IndexRanges = @()

                    foreach ($index in $includedPath.indexes) {
                        if ($index.kind -eq "Spatial") {
                            [CosmosDB.IndexingPolicy.Path.IndexSpatial]$thisIndex = New-CosmosDbCollectionIncludedPathIndex -Kind $index.kind -DataType $index.dataType
                            Write-Host "IncludedPathIndex kind: $($thisIndex.kind) index: $($thisIndex.DataType)"
                        }
                        elseif ($index.kind -eq "Range") {
                            [CosmosDB.IndexingPolicy.Path.IndexRange]$thisIndex = New-CosmosDbCollectionIncludedPathIndex -Kind $index.kind -DataType $index.dataType -Precision $index.precision
                            Write-Host "IncludedPathIndex kind: $($thisIndex.kind) index: $($thisIndex.DataType) precision: $($thisIndex.precision)"
                        }
                        elseif ($index.kind -eq "Hash") {
                            [CosmosDB.IndexingPolicy.Path.IndexHash]$thisIndex = New-CosmosDbCollectionIncludedPathIndex -Kind $index.kind -DataType $index.dataType -Precision $index.precision
                            Write-Host "IncludedPathIndex kind: $($thisIndex.kind) index: $($thisIndex.DataType) precision: $($thisIndex.precision)"
                        }
                        $IndexRanges += $thisIndex
                    }

                    $indexIncludedPath = New-CosmosDbCollectionIncludedPath -Path $includedPath.path -Index $indexRanges
                    $IndexIncludedPaths += $indexIncludedPath

                    Write-Host "indexIncludedPath path: $($includedPath.path.GetType()) index: $($indexRanges.GetType())"
                    Write-Host "indexIncludedPath indexes (object): $($indexRanges)"
                }

                Write-Host "indexIncludedPaths: $($IndexIncludedPaths)"

                foreach ($excludedPath in $Collection.IndexingPolicy.excludedPaths) {
                    $indexExcludedPath = New-CosmosDbCollectionExcludedPath -Path $excludedPath.path
                    $IndexExcludedPaths += $indexExcludedPath

                    Write-Host "indexExcludedPath path: $($excludedPath.path)"
                }

                Write-Host "indexExcludedPaths: $($IndexExcludedPaths)"

                $IndexingPolicy  = New-CosmosDbCollectionIndexingPolicy -Automatic $Collection.IndexingPolicy.automatic -IndexingMode $Collection.IndexingPolicy.indexingMode -IncludedPath $IndexIncludedPaths -ExcludedPath $IndexExcludedPaths -Debug
                Write-Host "Created New-CosmosDbCollectionIndexingPolicy: Automatic: $($IndexingPolicy.Automatic) Mode: $($IndexingPolicy.IndexingMode) IPs: $($IndexIncludedPaths.GetType()) EPs: $($IndexExcludedPaths.GetType())"

                $UpdatedCosmosDbCollectionParameters = @{
                    Context        = $CosmosDbContext
                    Database       = $Database.DatabaseName
                    Id             = $Collection.CollectionName
                    IndexingPolicy = $IndexingPolicy
                }
            }
            elseif (!$Collection.IndexingPolicy) {
                Write-Host "No IndexingPolicy for Collection: $($Collection.CollectionName)"
                if ($UpdateIndexPolicyFix.IsPresent) {
                    $DefaultIndexString = New-CosmosDbCollectionIncludedPathIndex -Kind Hash -DataType String -Precision 3
                    $DefaultIndexNumber = New-CosmosDbCollectionIncludedPathIndex -Kind Range -DataType Number -Precision -1
                    $DefaultIndexIncludedPath = New-CosmosDbCollectionIncludedPath -Path '/*' -Index $DefaultIndexString,$DefaultIndexNumber
                    $DefaultIndexingPolicy  = New-CosmosDbCollectionIndexingPolicy -Automatic $true -IndexingMode Consistent -IncludedPath $DefaultIndexIncludedPath

                    $UpdatedCosmosDbCollectionParameters = @{
                        Context        = $CosmosDbContext
                        Database       = $Database.DatabaseName
                        Id             = $Collection.CollectionName
                        IndexingPolicy = $DefaultIndexingPolicy
                    }
                }
                else {
                    $UpdatedCosmosDbCollectionParameters = @{
                        Context  = $CosmosDbContext
                        Database = $Database.DatabaseName
                        Id       = $Collection.CollectionName
                    }
                }

            }

            if ($Collection.DefaultTtl) {
                Write-Host "Add DefaultTtl: $($Collection.DefaultTtl)"
                $UpdatedCosmosDbCollectionParameters += @{
                    DefaultTimeToLive = $Collection.DefaultTtl
                }
            }

            Write-Host "Set Cosmos Collection: $($Collection.CollectionName)"
            $null = Set-CosmosDbCollection @UpdatedCosmosDbCollectionParameters
        }

        foreach ($StoredProcedure in $Collection.StoredProcedures) {
            # --- Create Stored Procedure
            try {
                $ExistingStoredProcedure = $null
                $GetCosmosDbStoredProcParameters = @{
                    Context      = $CosmosDbContext
                    Database     = $Database.DatabaseName
                    CollectionId = $Collection.CollectionName
                    Id           = $StoredProcedure.StoredProcedureName
                }
                $ExistingStoredProcedure = Get-CosmosDbStoredProcedure @GetCosmosDbStoredProcParameters
            }
            catch {
            }

            $FindStoredProcFileParameters = @{
                Path    = (Resolve-Path $CosmosDbProjectFolderPath)
                Filter  = "$($StoredProcedure.StoredProcedureName)*"
                Recurse = $true
                File    = $true
            }
            $StoredProcedureFile = Get-ChildItem @FindStoredProcFileParameters | ForEach-Object { $_.FullName }

            if (!$StoredProcedureFile) {
                Write-Host "Stored Procedure name $($StoredProcedure.StoredProcedureName) could not be found in $(Resolve-Path $CosmosDbProjectFolderPath)"
                throw "$_"
            }

            if ($StoredProcedureFile.GetType().Name -ne "String") {
                Write-Host "Multiple Stored Procedures with name $($StoredProcedure.StoredProcedureName) found in $(Resolve-Path $CosmosDbProjectFolderPath)"
                throw "$_"
            }

            if (!$ExistingStoredProcedure) {
                Write-Host "Creating Stored Procedure: $($StoredProcedure.StoredProcedureName) in $($Collection.CollectionName) in $($Database.DatabaseName)"
                $NewCosmosDbStoredProcParameters = @{
                    Context             = $CosmosDbContext
                    Database            = $Database.DatabaseName
                    CollectionId        = $Collection.CollectionName
                    Id                  = $StoredProcedure.StoredProcedureName
                    StoredProcedureBody = (Get-Content $StoredProcedureFile -Raw)
                }
                $null = New-CosmosDbStoredProcedure @NewCosmosDbStoredProcParameters
            }
            elseif ($ExistingStoredProcedure.body -ne (Get-Content $StoredProcedureFile -Raw)) {
                Write-Host "Updating Stored Procedure: $($StoredProcedure.StoredProcedureName) in $($Collection.CollectionName) in $($Database.DatabaseName)"
                $SetCosmosDbStoredProcParameters = @{
                    Context             = $CosmosDbContext
                    Database            = $Database.DatabaseName
                    CollectionId        = $Collection.CollectionName
                    Id                  = $StoredProcedure.StoredProcedureName
                    StoredProcedureBody = (Get-Content $StoredProcedureFile -Raw)
                }
                $null = Set-CosmosDbStoredProcedure @SetCosmosDbStoredProcParameters
            }
        }
    }
}




