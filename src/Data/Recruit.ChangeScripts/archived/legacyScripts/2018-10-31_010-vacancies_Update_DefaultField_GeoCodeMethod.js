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

    print("Start updating Vacancies with employer locations and empty geoCodeMethod to default 'Unspecified'.");

    const query = {
            "geoCodeMethod": { $exists: false }
        },
        batchUpdateLimit = 500;
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

            let writeResult = db.vacancies.update({
                "_id": doc._id,
                "geoCodeMethod": { $exists: false }
            }, {
                $set: { "geoCodeMethod": "Unspecified" }
            }, {
                upsert: false
            });

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated document '${toGUID(doc._id.hex())}' with geoCodeMethod: Unspecified.`);
        }

        passThrough++;
    } while (passThrough <= maxLoops && db.vacancies.find().count(query) > 0);

    print("Finished updating Vacancies with employer locations and empty geoCodeMethod to default 'Unspecified'.");
}