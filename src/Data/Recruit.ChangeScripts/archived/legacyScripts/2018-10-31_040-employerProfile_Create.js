{
    function getEmployerAccountIds(userInfos) {
        var lookup = [];
        print("Building lookup");
        for (let index = 0; index < userInfos.length; index++) {
            const element = userInfos[index];
            const employerAccountId = element._id.split("_")[1];
            lookup.push({ employerAccountId: employerAccountId,  legalEntities: element.legalEntities });
        }

        return lookup;
    }

    function getLatestAbout(legalEntityId) {

        var filter = { "legalEntityId": legalEntityId, "employerDescription": { $exists: true }};
        var result  = db.vacancies.find(filter).sort( { "lastUpdatedDate": -1 } ).limit(1);
        var vacancy = result[0];
        
        return vacancy ? vacancy.employerDescription : null;
    }

    function getLatestEmployerWebsiteUrl(legalEntityId) {
        var filter = { "legalEntityId": legalEntityId, "employerWebsiteUrl": { $exists: true }};
        var result  = db.vacancies.find(filter).sort( { "lastUpdatedDate": -1 } ).limit(1);
        var vacancy = result[0];
        
        return vacancy ? vacancy.employerWebsiteUrl : null;
    }

    print();
    print();
    print("Start creating Employer Profiles");
    print();

    let allEmployerProfilesCount = db.employerProfiles.count();

    if (allEmployerProfilesCount == 0) {
        let editUserInfos = db.queryStore.find({ "viewType": "EditVacancyInfo" }).toArray();
        print(`Found ${editUserInfos.length} EditInfo record/s`);

        var accountIds = getEmployerAccountIds(editUserInfos);

        for (let index = 0; index < accountIds.length; index++) {
            const profilesToInsert = [];
            const element = accountIds[index];

            for (let entityIndex = 0; entityIndex < element.legalEntities.length; entityIndex++) {
                const entity = element.legalEntities[entityIndex]; 

                var employerProfile = {};

                employerProfile._id = element.employerAccountId + "_" + entity.legalEntityId;
                employerProfile.employerAccountId = element.employerAccountId;
                employerProfile.legalEntityId = entity.legalEntityId;
                employerProfile.createdDate = new Date();

                const latestAbout = getLatestAbout(entity.legalEntityId);
                const latestWebsiteUrl = getLatestEmployerWebsiteUrl(entity.legalEntityId);

                if (latestAbout)
                    employerProfile.aboutOrganisation = latestAbout;
                
                if (latestWebsiteUrl)
                    employerProfile.organisationWebsiteUrl = latestWebsiteUrl;

                profilesToInsert.push(employerProfile);
            }

            var result = db.employerProfiles.insertMany(profilesToInsert);

            if (result.acknowledged)
                print(`Inserted ${result.insertedIds.length} profiles for employer ${element.employerAccountId}`);
            else
                print(`Error inserting profiles for employer ${element.employerAccountId}`);
        }

    }
    else {
        print("Employer Profiles already exist. No need to run this script");
    }

    print();
    print("Inserted Employer Profiles");
    print();
    print();
}