print("Start setting outage message.");
db.configuration.update({_id:"EmployerRecruitSystem"}, {$set: {plannedOutageMessage: "The recruitment service will be unavailable on 9th May 2019 between 10am and 12pm for essential maintenance."}});
print("Outage message set on employer.");
db.configuration.update({_id:"QaRecruitSystem"}, {$set: {plannedOutageMessage: "Vacancy QA  will be unavailable on 9th May 2019 between 10am and 12pm for essential maintenance."}});
print("Outage message set on QA.");
db.configuration.update({_id:"ProviderRecruitSystem"}, {$set: {plannedOutageMessage: "Recruit apprentices will be unavailable on 9th May 2019 between 10am and 12pm for essential maintenance."}});
print("Outage message set on provider.");
print("Completed setting outage message.");