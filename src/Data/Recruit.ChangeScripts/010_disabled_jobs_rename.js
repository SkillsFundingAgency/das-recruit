{
    print("Start updating Jobs system configuration documents with disabled jobs.");

    let jobsConfigDocId = "RecruitWebJobsSystem";

    //for DEMO disable - GenerateBlockedEmployersQueueTrigger
    //for TEST2 disable - GenerateBlockedEmployersQueueTrigger
    //for PROD disable - GenerateVacancyAnalyticsSummaryQueueTrigger    

    let targetEnv = db.getMongo().toString().replace("connection to ", "");

    print(`Processing environment '${targetEnv}'.`);

    if(targetEnv.contains("-demo-")){
        db.configuration.updateOne({"_id": jobsConfigDocId}, { $set : {disabledJobs : ["GenerateBlockedEmployersQueueTrigger"]}});
        print("Updated using DEMO configuration");
    }
    else if(targetEnv.contains("-test2-")){
        db.configuration.updateOne({"_id": jobsConfigDocId}, { $set : {disabledJobs : ["GenerateBlockedEmployersQueueTrigger"]}});
        print("Updated using TEST2 configuration");
    }
    else if(targetEnv.contains("-prd-")){
        db.configuration.updateOne({"_id": jobsConfigDocId}, { $set : {disabledJobs : ["GenerateVacancyAnalyticsSummaryQueueTrigger"]}});
        print("Updated using PROD configuration");
    }
    else {
        print("No changes required for this environment");
    }

    print("Finished updating Jobs system configuration documents with disabled jobs.");
}