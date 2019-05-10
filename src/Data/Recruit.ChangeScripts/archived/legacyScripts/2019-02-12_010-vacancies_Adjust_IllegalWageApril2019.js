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
            "wage.wageType" : "FixedWage",
            "isDeleted" : false,
            "status" : { $nin : ["Draft", "Closed", "Referred"]} 
        },
        apprenticeshipNationalMinimumWage2019 = 3.9;

    let matchedDocs = db.vacancies.aggregate([
        {
            $match: query
        },
        {
            $sort: { "dateCreated": 1 }
        }]);

    while (matchedDocs.hasNext()) {
        let doc = matchedDocs.next();

        var perHourWage = doc.wage.fixedWageYearlyAmount / 52 / doc.wage.weeklyHours;
        
        if(perHourWage < apprenticeshipNationalMinimumWage2019){
            print(`Found illegal 2019 wage. '${perHourWage}' per hour in document '${toGUID(doc._id.hex())}'`);

            let writeResult = db.vacancies.update({
                "_id": doc._id
            }, {
                $set: { "wage.wageType": "NationalMinimumWageForApprentices" },
                $unset : {"wage.fixedWageYearlyAmount" : "" }
            }, {
                upsert: false
            });

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated document '${toGUID(doc._id.hex())}' with wageType: NationalMinimumWageForApprentices.`);
        }
        else{
            print(`2019 wage OK. '${perHourWage}' per hour in document '${toGUID(doc._id.hex())}'. Not updating`);
        }
    }

    print("Finished updating Vacancies with illegal 2019 wages.");
}