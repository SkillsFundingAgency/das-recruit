{
    print("Start copy of documents from queryViews collection to queryStore collection.");

    let uniqueQueryViewsViewTypes = db.queryViews.distinct("viewType");

    let uniqueViewTypes = db.queryStore.distinct("viewType");

    uniqueViewTypes.forEach(doc => {
        db.queryStore.deleteMany({ "viewType": doc.viewType });
    });

    let queryViewDocs = db.queryViews.find().toArray();

    uniqueQueryViewsViewTypes.forEach(vt => {
        print(`queryViews collection has ${db.queryViews.count({ "viewType": vt.viewType })} ${vt.viewType}.`);
    });

    let insertResult = db.queryStore.insertMany(queryViewDocs);

    print(`Inserted ${insertResult.insertedIds.length} documents into queryStore.`);

    uniqueQueryViewsViewTypes.forEach(vt => {
        print(`queryStore collection has ${db.queryStore.count({ "viewType": vt.viewType })} ${vt.viewType}.`);
    });

    print("Finished copy of documents from queryViews collection to queryStore collection.");
}