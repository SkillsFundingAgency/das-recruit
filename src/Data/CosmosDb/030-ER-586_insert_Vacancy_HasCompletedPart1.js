// https://stackoverflow.com/a/44564089/5596802
function toGUID(hex) {
    var a = hex.substr(6, 2) + hex.substr(4, 2) + hex.substr(2, 2) + hex.substr(0, 2);
    var b = hex.substr(10, 2) + hex.substr(8, 2);
    var c = hex.substr(14, 2) + hex.substr(12, 2);
    var d = hex.substr(16, 16);
    hex = a + b + c + d;
    var uuid = hex.substr(0, 8) + "-" + hex.substr(8, 4) + "-" + hex.substr(12, 4) + "-" + hex.substr(16, 4) + "-" + hex.substr(20, 12);
    return uuid;
}

print("Start adding/updating Vacancies with HasCompletedPart1 true");

var query = {
        "hasCompletedPart1": { $exists: false },
        "title": { $exists: true },
        "employerLocation": { $exists: true },
        "programmeId": { $exists: true },
        "wage": { $exists: true }
    },
    batchUpdateLimit = 500,
    passThrough = 1;

var maxLoops = Math.ceil(db.vacancies.find().count(query) / batchUpdateLimit);

if (maxLoops === 0) {
    maxLoops = 1;
}

do {
    var matchedDocs = db.vacancies.aggregate([
        {
            $match: query
        },
        {
            $sort: { "dateCreated": 1 }
        },
        {
            $limit: batchUpdateLimit
        }
    ]);

    print("Found " + matchedDocs._batch.length + " document(s) to operate on in passThrough " + passThrough + " of " + maxLoops + ".");

    while (matchedDocs.hasNext()) {
        var doc = matchedDocs.next();

        var writeResult = db.vacancies.update({
            "_id": doc._id,
            "hasCompletedPart1": { $exists: false },
            "title": { $exists: true },
            "employerLocation": { $exists: true },
            "programmeId": { $exists: true },
            "wage": { $exists: true }
        }, {
            $set: { "hasCompletedPart1": true }
        }, {
            upsert: false
        });

        if (writeResult.hasWriteConcernError()) {
            printjson(writeResult.writeConcernError);
            quit(14);
        }

        print("Updated document '" + toGUID(doc._id.hex()) + "' with hasCompletedPart1: true.");
    }

    passThrough++;
} while (passThrough <= maxLoops && db.vacancies.find().count(query) > 0);

print("Finished adding/updating Vacancies with default hasCompletedPart1.");