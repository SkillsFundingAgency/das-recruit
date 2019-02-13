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

    print("Start adding/updating vacancyReviews collection and changing any employer contact field to fall under an employerContact field.");

    const query = {
            "vacancySnapshot.ownerType": "Employer",
            $or: [
                { "vacancySnapshot.employerContactName": { $exists: true } },
                { "vacancySnapshot.employerContactEmail": { $exists: true } },
                { "vacancySnapshot.employerContactPhone": { $exists: true } }
            ]
        },
        batchUpdateLimit = 5;
    let passThrough = 1,
        maxLoops = Math.ceil(db.reviews.count(query) / batchUpdateLimit);

    if (maxLoops === 0) {
        maxLoops = 1;
    }

    do {
        let matchedDocs = db.reviews.aggregate([
            {
                $match: query
            },
            {
                $sort: { "dateCreated": 1 }
            },
            {
                $limit: batchUpdateLimit
            },
            {
                $project: {
                    "vacancySnapshot.employerContactName": 1,
                    "vacancySnapshot.employerContactEmail": 1,
                    "vacancySnapshot.employerContactPhone": 1
                }
            }
        ]);

        print(`Found ${matchedDocs._batch.length} document(s) to operate on in pass-through ${passThrough} of ${maxLoops}.`);

        while (matchedDocs.hasNext()) {
            let doc = matchedDocs.next();

            let writeResult = db.reviews.update({
                "_id": doc._id
            }, {
                $rename: {
                    "vacancySnapshot.employerContactName": "vacancySnapshot.employerContact.Name",
                    "vacancySnapshot.employerContactEmail": "vacancySnapshot.employerContact.Email",
                    "vacancySnapshot.employerContactPhone": "vacancySnapshot.employerContact.Phone"
                }
            }, {
                upsert: false
            });

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated document '${toGUID(doc._id.hex())}' employer contact field(s) to fall under an employerContact field.`);
        }

        passThrough++;
    } while (passThrough <= maxLoops && db.reviews.count(query) > 0);

    print("Finished adding/updating vacancyReviews collection changing any employer contact field to fall under an employerContact field.");
}