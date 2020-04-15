print("Start setting outage message.");
db.configuration.update({_id:"EmployerRecruitSystem"}, {$set: {plannedOutageMessage: "Vacancies approved on 20 and 21 April 2020 will not show on the ‘Find an apprenticeship’ service until after 22 April 2020."}});
print("Outage message set on employer.");
db.configuration.update({_id:"QaRecruitSystem"}, {$set: {plannedOutageMessage: "Vacancies approved on 20 and 21 April 2020 will not show on the ‘Find an apprenticeship’ service until after 22 April 2020."}});
print("Outage message set on QA.");
db.configuration.update({_id:"ProviderRecruitSystem"}, {$set: {plannedOutageMessage: "Vacancies approved on 20 and 21 April 2020 will not show on the ‘Find an apprenticeship’ service until after 22 April 2020."}});
print("Outage message set on provider.");
print("Completed setting outage message.");