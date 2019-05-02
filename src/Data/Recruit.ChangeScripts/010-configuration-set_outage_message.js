var message = "The recruitment service will be unavailable on 9th May 2019 between 10am and 12pm for essential maintenance.";
print("Start setting outage message.");
db.configuration.update({_id:"EmployerRecruitSystem"}, {$set: {plannedOutageMessage: message}});
print("Outage message set on employer.");
db.configuration.update({_id:"QaRecruitSystem"}, {$set: {plannedOutageMessage: message}});
print("Outage message set on QA.");
db.configuration.update({_id:"ProviderRecruitSystem"}, {$set: {plannedOutageMessage: message}});
print("Outage message set on provider.");
print("Completed setting outage message.");