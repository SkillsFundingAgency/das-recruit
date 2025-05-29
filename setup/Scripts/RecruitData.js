use recruit


db.createCollection("vacancies");
db.createCollection("queryStore");
db.createCollection("referenceData");
db.createCollection("sequences");
db.createCollection("users");
db.createCollection("applicationReviews");
db.createCollection("configuration");
db.createCollection("vacancyReviews");
db.createCollection("employerProfiles");
db.createCollection("reports");
db.createCollection("userNotificationPreferences");
db.createCollection("communicationMessages");
db.createCollection("blockedOrganisations");

//Then Run

db.configuration.insertOne({
                           	_id : "QaRules",
                           	titlePopularityPercentageThreshold : 10
                           })

db.configuration.insertOne({
                           	_id : "EmployerRecruitSystem",
                           	plannedOutageMessage : ""
                           })
db.configuration.insertOne({
                           	_id : "RecruitWebJobsSystem",
                           	disabledJobs : [ ]
                           })
db.configuration.insertOne({
                           	_id : "ProviderRecruitSystem",
                           	plannedOutageMessage : ""
                           })
db.configuration.insertOne({
                           	_id : "QaRecruitSystem",
                           	plannedOutageMessage : ""
                           })

// Then
db.referenceData.insertOne({
                            _id : "CandidateSkills",
                            lastUpdatedDate : "2018-07-01T00:00:00Z",
                            skills : [
                                "Communication skills",
                                "IT skills",
                                "Attention to detail",
                                "Organisation skills",
                                "Customer care skills",
                                "Problem solving skills",
                                "Presentation skills",
                                "Administrative skills",
                                "Number skills",
                                "Analytical skills",
                                "Logical",
                                "Team working",
                                "Creative",
                                "Initiative",
                                "Non judgemental",
                                "Patience",
                                "Physical fitness"
                            ]
                          })
db.referenceData.insertOne({
                           	_id : "QualificationTypes",
                           	lastUpdatedDate : "2018-12-18T00:00:00Z",
                           	qualificationTypes : [
                           		"GCSE or equivalent",
                           		"AS Level or equivalent",
                           		"A Level or equivalent",
                           		"BTEC or equivalent",
                           		"NVQ or SVQ Level 1 or equivalent",
                           		"NVQ or SVQ Level 2 or equivalent",
                           		"NVQ or SVQ Level 3 or equivalent",
                           		"Other"
                           	]
                           })

                           db.referenceData.insertOne({
                           	_id : "BannedPhrases",
                           	lastUpdatedDate : "2019-08-21T00:00:00Z",
                           	bannedPhrases : [
                           		"driving licence",
                           		"permanent",
                           		"under 16",
                           		"under sixteen",
                           		"car",
                           		"drive",
                           		"unpaid",
                           		"young people",
                           		"young person",
                           		"male",
                           		"female",
                           		"man",
                           		"woman",
                           		"lady",
                           		"hand",
                           		"eye",
                           		"foot",
                           		"kick",
                           		"run",
                           		"stand",
                           		"physically strong",
                           		"physically active",
                           		"young",
                           		"mature",
                           		"fingers",
                           		"head",
                           		"jump",
                           		"school",
                           		"schools",
                           		"feet",
                           		"hands",
                           		"trainee",
                           		"nude",
                           		"nudes"
                           	]
                           })

db.referenceData.insertOne({
    _id : "Profanities",
    lastUpdatedDate : "2018-10-11T00:00:00Z",
    profanities : [
        "sauna",
        "saunas",
        "sensual",
        "x-rated"
    ]
    })


db.sequences.insertOne(
  {
    "_id": "Sequence_Vacancy",
    "lastValue": 1000000001
  }
)

