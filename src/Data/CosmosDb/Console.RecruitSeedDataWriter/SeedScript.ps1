
param(
    [string]$dbConnectionString
)

try {
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -c sequences -f "$PSScriptRoot/Sequence/Sequence_Vacancy.json"
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -f "$PSScriptRoot/Data/CandidateSkills.json" -o true
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -f "$PSScriptRoot/Data/QualificationTypes.json" -o true
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -f "$PSScriptRoot/Data/BannedPhrases.json" -o true
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -f "$PSScriptRoot/Data/Profanities.json" # Due to the sensitive content within this document, the actual profanity content will be loaded manually.

    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -c configuration -f "$PSScriptRoot/Data/QaRules.json"
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -c configuration -f "$PSScriptRoot/Data/EmployerRecruitSystem.json"
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -c configuration -f "$PSScriptRoot/Data/ProviderRecruitSystem.json"
    & dotnet "$PSScriptRoot/Esfa.Recruit.Console.RecruitSeedDataWriter.dll" $($dbConnectionString) -c configuration -f "$PSScriptRoot/Data/RecruitWebJobsSystem.json"
}
catch {
    throw $_
}