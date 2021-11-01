{
    const batchLimit = 500;
    var count = 0, limit = Number(db.users.count());

    // 1. Add VacancySentForReview preference to all Employers that already have a userNotificationPreferences.
    do {
        print(`Loading user batch ${count * batchLimit} -> ${(count * batchLimit) + batchLimit} of ${limit}`);

        var employersWithoutVacancySentForReview = db.users.aggregate([
            {
                $skip: count * batchLimit
            },
            {
                $limit: batchLimit
            },
            {
                $lookup:
                {
                    from: "userNotificationPreferences",
                    localField: "idamsUserId",
                    foreignField: "_id",
                    as: "Preferences"
                }
            },
            {
                $match:
                {
                    $and: [
                        {
                            "Preferences": { $exists: true , $not:
                                {
                                    $size: 0
                                }}
                        },
                        {
                            "userType": "Employer"
                        }]
                }
            },
            {
                $project:
                {
                    _id: 1,
                    userType: 1,
                    idamsUserId: 1,
                    name: 1,
                    preference:
                    {
                        "$arrayElemAt": ["$Preferences", 0]
                    }
                }
            },
            {
                $match:
                {
                    
                    $or: [
                        {
                            "preference.notificationTypes":
                            {
                                $not: /VacancySentForReview/
                            }
                        },
                        {
                            "preference.notificationTypes":
                            {
                                $exists: false
                            }
                        }]
                    
                }
            }]);

        print(`Found ${employersWithoutVacancySentForReview._batch.length} users without VacancySentForReview preference set`);

        while (employersWithoutVacancySentForReview.hasNext())
        {
            var doc = employersWithoutVacancySentForReview.next();
            var updatedNotificationTypes = "VacancySentForReview";

            print(`Updating ${doc.name}`);

            if (doc.preference.notificationTypes && doc.preference.notificationTypes.length > 1)
            {
                print(`${doc.name} has existing notificationTypes. Adding VacancySentForReview to existing.`);
                updatedNotificationTypes = doc.preference.notificationTypes + ", VacancySentForReview";
            }

            var updateDocument = {
                $set:
                {
                    "notificationTypes": updatedNotificationTypes
                }
            };

            var writeResult = db.userNotificationPreferences.update(
                {
                    "_id": doc.preference._id
                }, updateDocument,
                {
                    upsert: false
                });

            if (writeResult.hasWriteConcernError())
            {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated ${doc.name}`);
        }

        count = count + 1;
    } while (count * batchLimit <= limit);
}