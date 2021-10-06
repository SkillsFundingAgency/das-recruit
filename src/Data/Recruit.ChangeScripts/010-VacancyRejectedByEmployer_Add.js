{
    const batchLimit = 100;

    // 1. Add VacancyRejectedByEmployer preference to all Providers that already have a userNotificationPreferences.
    do {
        var providersWithoutVacancyRejectedByEmployer = db.userNotificationPreferences.aggregate([
            {
                $lookup:
            {
                from: "users",
                "let":
                {
                    idamsUserId:
                    {
                        $toString: "$_id"
                    }
                },
                pipeline: [
                    {
                        $match:
                    {
                        $expr:
                        {
                            $and: [
                                {
                                    $eq: ["$$idamsUserId", "$idamsUserId"]
                                },
                                {
                                    $eq: ["$userType", "Provider"]
                                }]
                        }
                    }
                    }],
                as: "User"
            }
            },
            {
                $match:
            {
                $and: [
                    {
                        "User":
                    {
                        $exists: true,
                        $not:
                        {
                            $size: 0
                        }
                    }
                    },
                    {
                        $or: [
                            {
                                "notificationTypes":
                        {
                            $not: /VacancyRejectedByEmployer/
                        }
                            },
                            {
                                "notificationTypes":
                        {
                            $exists: false
                        }
                            }]
                    }]
            }
            },
            {
                $limit: batchLimit
            },
            {
                $project:
            {
                _id: 1,
                notificationTypes: 1,
                user:
                {
                    "$arrayElemAt": ["$User", 0]
                }
            }
            }]);

        print(`Found ${providersWithoutVacancyRejectedByEmployer._batch.length} users without VacancyRejectedByEmployer preference set`);

        while (providersWithoutVacancyRejectedByEmployer.hasNext())
        {
            var doc = providersWithoutVacancyRejectedByEmployer.next();
            var updatedNotificationTypes = "VacancyRejectedByEmployer";

            print(`Updating ${doc.user.name}`);

            if (doc.notificationTypes && doc.notificationTypes.length > 1)
            {
                print(`${doc.user.name} has existing notificationTypes. Adding VacancyRejectedByEmployer to existing.`);
                updatedNotificationTypes = doc.notificationTypes + ", VacancyRejectedByEmployer";
            }

            var updateDocument = {
                $set:
                {
                    "notificationTypes": updatedNotificationTypes
                }
            };

            var writeResult = db.userNotificationPreferences.update(
                {
                    "_id": doc._id
                }, updateDocument,
                {
                    upsert: false
                });

            if (writeResult.hasWriteConcernError())
            {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated ${doc.user.name}`);
        }

    } while (providersWithoutVacancyRejectedByEmployer._batch.length >= batchLimit);
}