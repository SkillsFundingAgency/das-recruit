{
    print("Start removing employerAccountId from vacancyReviews.");

    const query = {
            "employerAccountId": { $exists: true }
        },
        batchUpdateLimit = 500,
        coll = db.reviews;
    let passThrough = 1,
        maxLoops = Math.ceil(coll.find().count(query) / batchUpdateLimit);

    if (maxLoops === 0) {
        maxLoops = 1;
    }

    do {
        let matchedDocs = coll.aggregate([
            {
                $match: query
            },
            {
                $sort: { "lastUpdated": 1 }
            },
            {
                $limit: batchUpdateLimit
            }
        ]);

        print(`Found ${matchedDocs._batch.length} document(s) to operate on in pass-through ${passThrough} of ${maxLoops}.`);

        while (matchedDocs.hasNext()) {
            let doc = matchedDocs.next();

            let writeResult = coll.update({
                "_id": doc._id
            }, {
                $unset: { "employerAccountId": "" }
            });

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated document '${doc._id}', removed employerAccountId.`);
        }

        passThrough++;
    }
    while (passThrough <= maxLoops && coll.find().count(query) > 0);

    print("Finished removing employerAccountId from vacancyReviews.");
}