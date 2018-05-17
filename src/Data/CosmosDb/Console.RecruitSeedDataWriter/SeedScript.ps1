
param(
    [string]$dbConnectionString
)

$dbConnectionString = $dbConnectionString.Replace("&", "\"&\"")

Invoke-Expression "dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll ""$dbConnectionString"" -c sequences -f Sequence\Sequence_Vacancy.json"
Invoke-Expression "dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll ""$dbConnectionString"" -f Data\CandidateSkills.json -o true"
Invoke-Expression "dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll ""$dbConnectionString"" -f Data\QualificationTypes.json -o true"
Invoke-Expression "dotnet Esfa.Recruit.Console.RecruitSeedDataWriter.dll ""$dbConnectionString"" -f Data\MinimumWageRanges.json -o true"
