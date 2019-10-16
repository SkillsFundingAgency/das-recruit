{
    print("Start updating communicationMessages collection documents which have a status of 'FailedDelivery' to 'NotSent'.");

    let writeResult = db.communicationMessages.updateMany({
        "status": "FailedDelivery"
    }, {
        "$set": { "status": "NotSent" }
    });

    if (writeResult.matchedCount !== writeResult.modifiedCount) {
        printjson("Error occurred updating communicationMessages status field.");
        quit(14);
    }

    print(`Found ${writeResult.matchedCount} communicationMessages with status 'FailedDelivery' of which ${writeResult.modifiedCount} have now been updated to have status 'NotSent'.`);

    print("Finished updating communicationMessages collection documents which have a status of 'FailedDelivery' to 'NotSent'.");
}