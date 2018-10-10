{
    print("Start update of document field named 'status' in vacancies collection documents.");

    let writeResult = db.vacancies.updateMany(
        {
            "status":
            {
                $in: [ "PendingReview", "UnderReview" ]
            }
        },
        {
            $set: { "status": "Submitted" }
        }
    );

    if (writeResult.matchedCount !== writeResult.modifiedCount) {
        printjson("Error occurred updating vacancy documents status field.");
        quit(14);
    }

    print(`Found '${writeResult.matchedCount}' documents to update, updated ${writeResult.modifiedCount}.`);

    print("Finished update of document field named 'status' in vacancies collection documents.");
}