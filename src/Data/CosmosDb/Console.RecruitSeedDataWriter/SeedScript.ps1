
param(
    [string]$dbConnectionString
)

try {
    & dotnet "$PSScriptRoot\Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($ConnectionString) -c sequences -f "$PSScriptRoot\Sequence\Sequence_Vacancy.json"
    & dotnet "$PSScriptRoot\Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($ConnectionString) -f "$PSScriptRoot\Data\CandidateSkills.json" -o true
    & dotnet "$PSScriptRoot\Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($ConnectionString) -f "$PSScriptRoot\Data\QualificationTypes.json" -o true
    & dotnet "$PSScriptRoot\Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($ConnectionString) -f "$PSScriptRoot\Data\MinimumWageRanges.json" -o true
}
catch {
    throw $_
}