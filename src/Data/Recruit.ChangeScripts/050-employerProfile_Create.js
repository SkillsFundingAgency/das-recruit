{
    // Generate NUUID: https://stackoverflow.com/questions/31836350/how-to-generate-nuuid-in-mongodb
    function HexToBase64(hex) {
        var base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        var base64 = "";
        var group;
        for (var i = 0; i < 30; i += 6) {
            group = parseInt(hex.substr(i, 6), 16);
            base64 += base64Digits[(group >> 18) & 0x3f];
            base64 += base64Digits[(group >> 12) & 0x3f];
            base64 += base64Digits[(group >> 6) & 0x3f];
            base64 += base64Digits[group & 0x3f];
        }
        group = parseInt(hex.substr(30, 2), 16);
        base64 += base64Digits[(group >> 2) & 0x3f];
        base64 += base64Digits[(group << 4) & 0x3f];
        base64 += "==";
        return base64;
    }
    
    function NUUID(uuid) {
        var hex = uuid.replace(/[{}-]/g, ""); // remove extra characters
        var a = hex.substr(6, 2) + hex.substr(4, 2) + hex.substr(2, 2) + hex.substr(0, 2);
        var b = hex.substr(10, 2) + hex.substr(8, 2);
        var c = hex.substr(14, 2) + hex.substr(12, 2);
        var d = hex.substr(16, 16);
        hex = a + b + c + d;
        var base64 = HexToBase64(hex);
        return new BinData(3, base64); // eslint-disable-line no-undef
    }

    function uuid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
        }
        return s4() + s4() + "-" + s4() + "-" + s4() + "-"+ s4() + "-" + s4() + s4() + s4();
    }
    
    function getEmployerAccountIds(userInfos) {
        var lookup = [];
        print("Building lookup");
        for (let index = 0; index < userInfos.count(); index++) {
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

    let allEmployerProfilesCount = db.employerProfiles.find({}).count();

    if (allEmployerProfilesCount === 0) {
        let editUserInfos = db.queryStore.find({ "viewType": "EditVacancyInfo" });
        print(`Found ${editUserInfos.count()} EditInfo record/s`);

        var accountIds = getEmployerAccountIds(editUserInfos);

        for (let index = 0; index < accountIds.length; index++) {
            const profilesToInsert = [];
            const element = accountIds[index];

            for (let entityIndex = 0; entityIndex < element.legalEntities.length; entityIndex++) {
                const entity = element.legalEntities[entityIndex];

                var employerProfile = {};

                employerProfile._id = NUUID(uuid());
                employerProfile.employerAccountId = element.employerAccountId;
                employerProfile.legalEntityId = entity.legalEntityId;

                const latestAbout = getLatestAbout(entity.legalEntityId);
                const latestWebsiteUrl = getLatestEmployerWebsiteUrl(entity.legalEntityId);

                if (latestAbout)
                    employerProfile.aboutOrganisation = latestAbout;
                
                if (latestWebsiteUrl)
                    employerProfile.organisationWebsiteUrl = latestWebsiteUrl;

                profilesToInsert.push(employerProfile);
            }

            db.employerProfiles.insertMany(profilesToInsert);

            print(`Inserted ${profilesToInsert.length} profiles for employer ${element.employerAccountId}`);
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