{
    print("Start removing accountsDeclaredAsLevyPayers from users.");

    const query = {
            "accountsDeclaredAsLevyPayers": { $exists: true }
        },
        batchUpdateLimit = 500,
        coll = db.users;     
    let passThrough = 1,
        maxLoops = Math.ceil(coll.find().count(query) / batchUpdateLimit);

    if (maxLoops === 0) {
        maxLoops = 1;
    }

    print(query);

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
                $unset: { "accountsDeclaredAsLevyPayers": "" }
            });

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated document '${doc._id}', removed accountsDeclaredAsLevyPayers.`);
        }

        passThrough++;
    }
    while (passThrough <= maxLoops && coll.find().count(query) > 0);

    print("Finished removing accountsDeclaredAsLevyPayers from users.");
}