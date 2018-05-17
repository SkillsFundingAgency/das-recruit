
param(
    [string]$dbConnectionString
)

dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll $dbConnectionString -c sequences -f Sequence\Sequence_Vacancy.json
dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll $dbConnectionString -f Data\CandidateSkills.json -o true
dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll $dbConnectionString -f Data\QualificationTypes.json -o true
dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll $dbConnectionString -f Data\MinimumWageRanges.json -o true
