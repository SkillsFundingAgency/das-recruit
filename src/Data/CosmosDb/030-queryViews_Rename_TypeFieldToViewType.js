{
    print("Start copy of document field named 'type' to a new field 'viewType' in queryViews collection documents.");

    const query = {
            "viewType": { $exists: false }
        },
        batchUpdateLimit = 500;
    let passThrough = 1,
        maxLoops = Math.ceil(db.queryViews.find().count(query) / batchUpdateLimit);

    if (maxLoops === 0) {
        maxLoops = 1;
    }

    do {
        let matchedDocs = db.queryViews.aggregate([
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

            let writeResult = db.queryViews.update({
                "_id": doc._id
            }, {
                $set: { "viewType": doc.type }
            });

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated document '${doc._id}', copied field 'type' to a new field 'viewType'.`);
        }

        passThrough++;
    }
    while (passThrough <= maxLoops && db.queryViews.find().count(query) > 0);

    print("Finished copy of document field named 'type' to a new field 'viewType' in queryViews collection documents.");
}