print("Start adding/updating Vacancies with default applicationMethod.");

var query = {
    "applicationMethod": { $exists: false }
};

do {
    var matchedDocs = db.vacancies.find(query);

    print("Found " + matchedDocs.count() + " document(s) to operate on.");

    while (matchedDocs.hasNext()) {
        var doc = matchedDocs.next();

        var writeResult = db.vacancies.update({
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
} while (db.vacancies.count(query) > 0);

print("Finished adding/updating Vacancies with default applicationMethod.");