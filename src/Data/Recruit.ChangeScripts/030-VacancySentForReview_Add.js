{
    const batchLimit = 100;

    // 3. Add VacancySentForReview preference to all Employers that already have a userNotificationPreferences.
    do {
        var employersWithoutVacancySentForReview = db.userNotificationPreferences.aggregate([
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
                                    $eq: ["$userType", "Employer"]
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
                            $not: /VacancySentForReview/
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

        print(`Found ${employersWithoutVacancySentForReview._batch.length} users without VacancySentForReview preference set`);

        while (employersWithoutVacancySentForReview.hasNext())
        {
            var doc = employersWithoutVacancySentForReview.next();
            var updatedNotificationTypes = "VacancySentForReview";

            print(`Updating ${doc.user.name}`);

            if (doc.notificationTypes && doc.notificationTypes.length > 1)
            {
                print(`${doc.user.name} has existing notificationTypes. Adding VacancySentForReview to existing.`);
                updatedNotificationTypes = doc.notificationTypes + ", VacancySentForReview";
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

    } while (employersWithoutVacancySentForReview._batch.length >= batchLimit);
}