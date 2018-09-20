function millisToMinutesAndSeconds(millis) {
    let minutes = Math.floor(millis / 60000);
    let seconds = ((millis % 60000) / 1000).toFixed(0);
    return (seconds === 60 ? `${(minutes+1)}:00` : `${minutes}:${(seconds < 10 ? "0" : "")}${seconds}`);
}

{
    print("Start copy of documents from queryViews collection to queryStore collection.");

    const oneSecondInMilliseconds = 1000;
    let uniqueQueryViewsViewTypes = db.queryViews.distinct("viewType");

    let uniqueViewTypes = db.queryStore.distinct("viewType");

    uniqueViewTypes.forEach(vt => {
        print(`Deleting documents of viewType ${vt} from queryStore.`);
        db.queryStore.deleteMany({ "viewType": vt });
    });

    let queryViewDocs = db.queryViews.find().toArray();

    uniqueQueryViewsViewTypes.forEach(vt => {
        print(`queryViews collection has ${db.queryViews.find({}).toArray().filter(doc => doc.viewType === vt).length} ${vt} documents.`);
    });

    let insertCount = 0,
        insertLoopCount = 0,
        startTime = new Date().getTime();

    if (queryViewDocs.length > 0) {

        for (let pos = 0, insertBatchSize = 5; pos <= queryViewDocs.length; pos += insertBatchSize) {
            print("... insert processing ...");
            insertLoopCount++;
            let docs = queryViewDocs.slice(pos, pos + insertBatchSize);

            if (docs.length > 0) {
                let insertResult = db.queryStore.insertMany(docs);
                insertCount += insertResult.insertedIds.length;
            }
            /* eslint-disable */
            sleep(oneSecondInMilliseconds);
            /* eslint-disable */
        }
    }

    let totalTime = (new Date().getTime() - startTime),
        totalTimeTaken = millisToMinutesAndSeconds(totalTime),
        totalInsertTimeTaken = millisToMinutesAndSeconds(totalTime - (insertLoopCount * oneSecondInMilliseconds));

    print(`Inserted ${insertCount} documents into queryStore. Took ${totalInsertTimeTaken}`);
    print(`Looped insert ${insertLoopCount} times. Took overall ${totalTimeTaken}`);

    uniqueQueryViewsViewTypes.forEach(vt => {
        print(`queryStore collection has ${db.queryStore.count({ "viewType": vt }) + 0} ${vt} documents.`);
    });

    print("Finished copy of documents from queryViews collection to queryStore collection.");
}
