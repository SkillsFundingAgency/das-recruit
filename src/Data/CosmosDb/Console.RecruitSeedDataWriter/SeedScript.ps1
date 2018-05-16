
param(
    [string]$dbConnectionString
)

& dotnet Console.RecruitSeedDataWriter.dll $dbConnectionString -c sequences -f Sequence\Sequence_Vacancy.json
& dotnet Console.RecruitSeedDataWriter.dll $dbConnectionString -f Data\CandidateSkills.json -o true
& dotnet Console.RecruitSeedDataWriter.dll $dbConnectionString -f Data\QualificationTypes.json -o true
& dotnet Console.RecruitSeedDataWriter.dll $dbConnectionString -f Data\MinimumWageRanges.json -o true
