{
    function buildLookup(userInfos) {
        var lookup = {};
        print("Building lookup");
        for (let index = 0; index < userInfos.length; index++) {
            const element = userInfos[index];
            const employerAccountId = element._id.split("_")[1];
            lookup[employerAccountId] = element.legalEntities;
        }

        return lookup;
    }

    function matchLegalEntityName(nameToMatch, accountLegalEntities) {
        for (let index = 0; index < accountLegalEntities.length; index++) {
            const element = accountLegalEntities[index];
            if (nameToMatch === element.name)
                return element.legalEntityId;
        }

        return null;
    }

    print();
    print();
    print("Start updating Vacancies with LegalEntityId if missing and has an EmployerName");
    print();

    let allVacancies = db.vacancies.find({ "legalEntityId": { $exists: false }, "employerName": { $exists: true } }).toArray();
    var targetVacancyCount = allVacancies.length;
    print(`There are ${targetVacancyCount} vacancies with missing a LegalEntityId field.`);

    if (allVacancies.length > 0) {

        let editUserInfos = db.queryStore.find({ "viewType": "EditVacancyInfo" }).toArray();

        print(`Found ${editUserInfos.length} EditInfo record/s`);

        var lookup = buildLookup(editUserInfos);

        print("Updating vacancies");

        var updateCount = 0;
        
        for (let index = 0; index < targetVacancyCount; index++) {
            const element = allVacancies[index];
            
            var legalEntityId = matchLegalEntityName(element.employerName, lookup[element.employerAccountId]);
            
            let writeResult = db.vacancies.updateOne(
                { "_id": element._id },
                { $set: { "legalEntityId": legalEntityId }}
            );
        
            if (writeResult.matchedCount !== writeResult.modifiedCount) {
                printjson("Error occurred updating vacancy with legalEntityId field.");
                quit(14);
            }

            updateCount++;
        }

        print(`Updated ${updateCount} vacancies with LegalEntityId`);

        let postUpdateVacancies = db.vacancies.find({ "legalEntityId": { $exists: false }, "employerName": { $exists: true } }).toArray();

        if (postUpdateVacancies.length > 0)
            print(`WARNING: There is/are still ${allVacancies.length} vacancies with missing a LegalEntityId field.`);        
    }

    print();
    print("Updated Vacancies with LegalEntityId.");
    print();
    print();
}