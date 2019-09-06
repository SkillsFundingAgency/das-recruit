{
    print("Start updating communicationMessages collection documents which have a status of 'Unsent' to 'Pending'.");

    let writeResult = db.communicationMessages.updateMany({
        "status": "Unsent"
    }, {
        "$set": { "status": "Pending" }
    });

    if (writeResult.hasWriteConcernError()) {
        printjson(writeResult.writeConcernError);
        quit(14);
    }

    print(`Found ${writeResult.matchedCount} communicationMessages with status 'Unsent' of which ${writeResult.modifiedCount} have now been updated to have status 'Pending'.`);

    print("Finished updating communicationMessages collection documents which have a status of 'Unsent' to 'Pending'.");
}