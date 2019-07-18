{
    print("Start updating Provider and Employer Recruit system configuration documents.");

    let employerRecruitSystemConfigDocId = "EmployerRecruitSystem",
        providerRecruitSystemConfigDocId = "ProviderRecruitSystem",
        targetEnv = db.getMongo().toString().replace("connection to ", "");

    if (targetEnv.contains("-prd-")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDate: ISODate("2019-05-30T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDate: ISODate("2019-06-30T00:00:00.000Z") } });
    }

    print("Finished updating Provider and Employer Recruit system configuration documents.");
}