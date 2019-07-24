print("Start clearing outage message.");
db.configuration.update({_id:"EmployerRecruitSystem"}, {$set: {plannedOutageMessage: ""}});
print("Outage message cleared on employer.");
db.configuration.update({_id:"QaRecruitSystem"}, {$set: {plannedOutageMessage: ""}});
print("Outage message cleared on QA.");
db.configuration.update({_id:"ProviderRecruitSystem"}, {$set: {plannedOutageMessage: ""}});
print("Outage message cleared on provider.");
print("Completed clearing outage message.");