var matchedDocs = db.queryViews.find(
{
    "type": "LiveVacancy",
    "applicationMethod": { $exists: false }
});

print("Found " + matchedDocs.count() + " documents to operate on.");

while (matchedDocs.hasNext()) {
    var doc = matchedDocs.next();

    var accept = db.queryViews.update(
    {
        "_id": doc._id,
        "applicationMethod": { $exists: false }
    },
    {
        $set: { "applicationMethod": "ThroughExternalApplicationSite" }
    },
    {
        upsert: false
    });

    print("Updated document '" + doc._id + "' with applicationMethod: ThroughExternalApplicationSite");
}