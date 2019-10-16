{
    print("Start updating communicationMessages collection documents which have a status of 'Unsent' to 'Pending'.");

    let writeResult = db.communicationMessages.updateMany({
        "status": "Unsent"
    }, {
        "$set": { "status": "Pending" }
    });

    if (writeResult.matchedCount !== writeResult.modifiedCount) {
        printjson("Error occurred updating vacancy with legalEntityId field.");
        quit(14);
    }

    print(`Found ${writeResult.matchedCount} communicationMessages with status 'Unsent' of which ${writeResult.modifiedCount} have now been updated to have status 'Pending'.`);

    print("Finished updating communicationMessages collection documents which have a status of 'Unsent' to 'Pending'.");
}