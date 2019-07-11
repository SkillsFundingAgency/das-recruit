{
    print("Start updating Jobs system configuration documents with disabled jobs.");

    let jobsRecruitSystemConfigDocId = "RecruitWebJobsSystem";

    let targetEnv = db.getMongo().toString().replace("connection to ", "");

    print(`Processing environment '${targetEnv}'.`);

    if (targetEnv.contains("-prd-")) {
        db.configuration.updateOne({ "_id": jobsRecruitSystemConfigDocId }, { $set : {disabledJobs : []} });
    }

    print("Finished updating Jobs system configuration documents with disabled jobs.");
}