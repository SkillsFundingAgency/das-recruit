{
    const batchLimit = 100;

    // 2. Add VacancySentForReview preference to all Providers that don't have a userNotificationPreferences.
    do {
        var employersWithoutUserNotificationPreferences = db.users.aggregate([
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
                        "Preferences": []
                    },
                    {
                        "userType": "Employer"
                    }]
            }
            },
            {
                $limit: batchLimit
            }]);

        print(`Found ${employersWithoutUserNotificationPreferences._batch.length} users without UserNotificationPreferences`);

        while (employersWithoutUserNotificationPreferences.hasNext())
        {
            var doc = employersWithoutUserNotificationPreferences.next();

            print(`Updating ${doc.name}`);

            var userNotificationPreferences = {
                _id: doc.idamsUserId,
                notificationTypes: "VacancySentForReview"
            };

            var writeResult = db.userNotificationPreferences.insert(userNotificationPreferences);

            if (writeResult.hasWriteConcernError())
            {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated ${doc.name}`);
        }

    } while (employersWithoutUserNotificationPreferences._batch.length >= batchLimit);
}