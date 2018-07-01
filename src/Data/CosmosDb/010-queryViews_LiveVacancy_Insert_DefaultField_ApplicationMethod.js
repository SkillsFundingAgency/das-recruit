print("Start adding/updating LiveVacancy queryViews with default applicationMethod.");

var query = {
    "type": "LiveVacancy",
    "applicationMethod": { $exists: false }
};

do {
    var matchedDocs = db.queryViews.find(query);

    print("Found " + matchedDocs.count() + " document(s) to operate on.");

    while (matchedDocs.hasNext()) {
        var doc = matchedDocs.next();

        var writeResult = db.queryViews.update({
            "_id": doc._id,
            "applicationMethod": { $exists: false }
        }, {
            $set: { "applicationMethod": "ThroughExternalApplicationSite" }
        }, {
            upsert: false
        });

        if (writeResult.hasWriteConcernError()) {
            printjson(writeResult.writeConcernError);
            quit(14);
        }

        print("Updated document '" + doc._id + "' with applicationMethod: ThroughExternalApplicationSite.");
    }
}
while (db.queryViews.count(query) > 0);

print("Finished adding/updating LiveVacancy queryViews with default applicationMethod.");