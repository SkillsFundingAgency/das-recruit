{
    print("Start updating Jobs system configuration documents with disabled jobs.");

    let jobsRecruitSystemConfigDocId = "RecruitWebJobsSystem";

    let targetEnv = db.getMongo().toString().replace("connection to ", ""),
        jobsToDisable = ["CommunicationRequestQueueTrigger", "CommunicationMessageDispatcherQueueTrigger"];

    print(`Processing environment '${targetEnv}'.`);

    if (targetEnv.contains("-at-")) {
        db.configuration.updateOne(
            { "_id": jobsRecruitSystemConfigDocId },
            { $addToSet : { disabledJobs : { $each: jobsToDisable } } }
        );
        print("Updated using AT configuration");
    }
    else if (targetEnv.contains("-test-")) {
        db.configuration.updateOne(
            { "_id": jobsRecruitSystemConfigDocId },
            { $addToSet : { disabledJobs : { $each: jobsToDisable } } }
        );
        print("Updated using TEST configuration");
    }
    else if (targetEnv.contains("-test2-")) {
        db.configuration.updateOne(
            { "_id": jobsRecruitSystemConfigDocId },
            { $addToSet : { disabledJobs : { $each: jobsToDisable } } }
        );
        print("Updated using TEST2 configuration");
    }
    else if (targetEnv.contains("-demo-")) {
        db.configuration.updateOne(
            { "_id": jobsRecruitSystemConfigDocId },
            { $addToSet : { disabledJobs : { $each: jobsToDisable } } }
        );
        print("Updated using DEMO configuration");
    }
    else if (targetEnv.contains("-pp-")) {
        db.configuration.updateOne(
            { "_id": jobsRecruitSystemConfigDocId },
            { $addToSet : { disabledJobs : { $each: jobsToDisable } } }
        );
        print("Updated using PREPROD configuration");
    }
    else if (targetEnv.contains("-prd-")) {
        db.configuration.updateOne(
            { "_id": jobsRecruitSystemConfigDocId },
            { $addToSet : { disabledJobs : { $each: jobsToDisable } } }
        );
        print("Updated using PROD configuration");
    }
    else {
        print("No changes required for this environment");
    }

    print("Finished updating Jobs system configuration documents with disabled jobs.");
}