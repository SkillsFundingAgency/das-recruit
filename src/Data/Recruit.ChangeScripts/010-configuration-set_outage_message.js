var message = "The recruitment service will be unavailable on 9th May 2019 between 10am and 11am for essential maintenance."
print("Start setting outage message.");
db.configuration.update({_id:"EmployerRecruitSystem"}, {$set: {plannedOutageMessage: "outage is planned"}});
print("Outage message set on employer.");
db.configuration.update({_id:"QaRecruitSystem"}, {$set: {plannedOutageMessage: "outage is planned"}});
print("Outage message set on QA.");
db.configuration.update({_id:"ProviderRecruitSystem"}, {$set: {plannedOutageMessage: "outage is planned"}});
print("Outage message set on provider.");
print("Completed setting outage message.");