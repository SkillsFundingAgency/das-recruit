function millisToMinutesAndSeconds(millis) {
    let minutes = Math.floor(millis / 60000);
    let seconds = ((millis % 60000) / 1000).toFixed(0);
    return (seconds === 60 ? `${(minutes+1)}:00` : `${minutes}:${(seconds < 10 ? "0" : "")}${seconds}`);
}

{
    print("Start copy of documents from queryViews collection to queryStore collection.");

    let uniqueQueryViewsViewTypes = db.queryViews.distinct("viewType");

    let uniqueViewTypes = db.queryStore.distinct("viewType");

    uniqueViewTypes.forEach(vt => {
        print(`Deleting documents of viewType ${vt} from queryStore.`);
        db.queryStore.deleteMany({ "viewType": vt });
    });

    let queryViewDocs = db.queryViews.find().toArray();

    uniqueQueryViewsViewTypes.forEach(vt => {
        print(`queryViews collection has ${db.queryViews.find().count({ "viewType": vt })} ${vt}.`);
    });

    var startTime = new Date().getTime();

    let insertResult = db.queryStore.insertMany(queryViewDocs);

    let timeTaken = millisToMinutesAndSeconds(new Date().getTime() - startTime);

    print(`Inserted ${insertResult.insertedIds.length} documents into queryStore. Took ${timeTaken}`);

    uniqueQueryViewsViewTypes.forEach(vt => {
        print(`queryStore collection has ${db.queryStore.count({ "viewType": vt })} ${vt}.`);
    });

    print("Finished copy of documents from queryViews collection to queryStore collection.");
}