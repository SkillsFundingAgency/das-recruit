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

    print("Start updating Vacancies with illegal 2019 wages.");

    const query = {
            "startDate": { $gte: ISODate("2019-04-01T00:00:00.000Z") },
            "wage.wageType" : "FixedWage"
        },
        batchUpdateLimit = 500,
        apprenticeshipNationalMinimumWage2019 = 3.9;
    let passThrough = 1,
        maxLoops = Math.ceil(db.vacancies.find().count(query) / batchUpdateLimit);

    if (maxLoops === 0) {
        maxLoops = 1;
    }

    do {
        let matchedDocs = db.vacancies.aggregate([
            {
                $match: query
            },
            {
                $sort: { "dateCreated": 1 }
            },
            {
                $limit: batchUpdateLimit
            }
        ]);

        print(`Found ${matchedDocs._batch.length} document(s) to operate on in pass-through ${passThrough} of ${maxLoops}.`);

        while (matchedDocs.hasNext()) {
            let doc = matchedDocs.next();

            var perHourWage = doc.wage.fixedWageYearlyAmount / 52 / doc.wage.weeklyHours;
            
            if(perHourWage < apprenticeshipNationalMinimumWage2019){
                print(`Found illegal 2019 wage. '${perHourWage}' per hour in document '${toGUID(doc._id.hex())}'`);

                let writeResult = db.vacancies.update({
                    "_id": doc._id
                }, {
                    $set: { 
                        "wage.wageType": "NationalMinimumWageForApprentices",
                        "wage.fixedWageYearlyAmount" : null
                    }
                }, {
                    upsert: false
                });

                if (writeResult.hasWriteConcernError()) {
                    printjson(writeResult.writeConcernError);
                    quit(14);
                }

                print(`Updated document '${toGUID(doc._id.hex())}' with wageType: NationalMinimumWageForApprentices.`);
            }
        }

        passThrough++;
    } while (passThrough <= maxLoops && db.vacancies.find().count(query) > 0);

    print("Finished updating Vacancies with illegal 2019 wages.");
}