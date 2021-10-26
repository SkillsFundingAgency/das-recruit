{
    const batchLimit = 100;

    // 1. Add VacancyRejectedByEmployer preference to all Providers that already have a userNotificationPreferences.
    do {
        var providersWithoutVacancyRejectedByEmployer = db.users.aggregate([
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
                            "userType": "Provider"
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
                                $not: /VacancyRejectedByEmployer/
                            }
                        },
                        {
                            "preference.notificationTypes":
                            {
                                $exists: false
                            }
                        }]
                    
                }
            },
            {
                $limit: batchLimit
            }]);

        print(`Found ${providersWithoutVacancyRejectedByEmployer._batch.length} users without VacancyRejectedByEmployer preference set`);

        while (providersWithoutVacancyRejectedByEmployer.hasNext())
        {
            var doc = providersWithoutVacancyRejectedByEmployer.next();
            var updatedNotificationTypes = "VacancyRejectedByEmployer";

            print(`Updating ${doc.name}`);

            if (doc.preference.notificationTypes && doc.preference.notificationTypes.length > 1)
            {
                print(`${doc.name} has existing notificationTypes. Adding VacancyRejectedByEmployer to existing.`);
                updatedNotificationTypes = doc.preference.notificationTypes + ", VacancyRejectedByEmployer";
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

    } while (providersWithoutVacancyRejectedByEmployer._batch.length >= batchLimit);
}