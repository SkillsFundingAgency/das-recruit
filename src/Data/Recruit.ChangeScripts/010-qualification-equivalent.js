{
    // https://stackoverflow.com/a/44564089/5596802
    function toGUID(hex) {
        let a = hex.substr(6, 2) + hex.substr(4, 2) + hex.substr(2, 2) + hex.substr(0, 2);
        let b = hex.substr(10, 2) + hex.substr(8, 2);
        let c = hex.substr(14, 2) + hex.substr(12, 2);
        let d = hex.substr(16, 16);
        hex = a + b + c + d;
        let uuid = hex.substr(0, 8) + "-" + hex.substr(8, 4) + "-" + hex.substr(12, 4) + "-" + hex.substr(16, 4) + "-" + hex.substr(20, 12);
        return uuid;
    }

    print("Start updating Vacancies appending 'or equivalent' to qualifications.");

    const query = {
        "qualifications": { $exists: true }
    };

    let matchedDocs = db.vacancies.aggregate([
        {
            $match: query
        },
        {
            $sort: { "dateCreated": 1 }
        }]);

    while (matchedDocs.hasNext()) {
        let doc = matchedDocs.next();
        let changed = false;
        
        for(var i = 0; i < doc.qualifications.length; i++)
        {
            let currentQualificationType = doc.qualifications[i].qualificationType;
            let updatedQualificationType = doc.qualifications[i].qualificationType;

            switch(currentQualificationType)
            {
            case "GCSE":
                updatedQualificationType = "GCSE or equivalent";
                break;
            case "AS Level":
                updatedQualificationType = "AS Level or equivalent";
                break;
            case "A Level":
                updatedQualificationType = "A Level or equivalent";
                break;
            case "BTEC":
                updatedQualificationType = "BTEC or equivalent";
                break;
            case "NVQ or SVQ Level 1":
                updatedQualificationType = "NVQ or SVQ Level 1 or equivalent";
                break;
            case "NVQ or SVQ Level 2":
                updatedQualificationType = "NVQ or SVQ Level 2 or equivalent";
                break;
            case "NVQ or SVQ Level 3":
                updatedQualificationType = "NVQ or SVQ Level 3 or equivalent";
                break; 
            }

            if(currentQualificationType != updatedQualificationType)
            {
                doc.qualifications[i].qualificationType = updatedQualificationType;
                changed = true;

                print(`Changing qualificationType:'${currentQualificationType}' to '${doc.qualifications[i].qualificationType}'`);
            }
        }

        if(changed){
            let writeResult = db.vacancies.update({
                "_id": doc._id
            }, {
                $set: { "qualifications": doc.qualifications }
            }, {
                upsert: false
            });

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated document '${toGUID(doc._id.hex())}'`);
        }
        else{
            print(`Skipping document '${toGUID(doc._id.hex())}'. Qualficiations already updated`);
        }
    }

    print("Finished updating Vacancies appending 'or equivalent' to qualifications.");
}