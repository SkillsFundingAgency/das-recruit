{
    print("Start updating Provider and Employer Recruit system configuration documents.");

    let employerRecruitSystemConfigDocId = "EmployerRecruitSystem",
        providerRecruitSystemConfigDocId = "ProviderRecruitSystem",
        showAnalyticsForVacanciesApprovedAfterDateFieldName = "showAnalyticsForVacanciesApprovedAfterDate", // eslint-disable-line
        targetEnv = db.getMongo().toString().replace("connection to ", "");

    if (targetEnv.contains("localhost") || targetEnv.contains("127.0.0.1:")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-29T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-29T00:00:00.000Z") } });
    }

    if (targetEnv.contains("-at-")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-29T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-29T00:00:00.000Z") } });
    }

    if (targetEnv.contains("-test-")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
    }

    if (targetEnv.contains("-test2-")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
    }

    if (targetEnv.contains("-demo-")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
    }

    if (targetEnv.contains("-pp-")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2019-05-30T00:00:00.000Z") } });
    }

    if (targetEnv.contains("-prd-")) {
        db.configuration.updateOne({ "_id": employerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2030-12-31T00:00:00.000Z") } });
        db.configuration.updateOne({ "_id": providerRecruitSystemConfigDocId }, { $set: { showAnalyticsForVacanciesApprovedAfterDateFieldName: ISODate("2030-12-31T00:00:00.000Z") } });
    }

    print("Finished updating Provider and Employer Recruit system configuration documents.");
}