{
    const batchLimit = 100;

    // 2. Add VacancyRejectedByEmployer preference to all Providers that don't have a userNotificationPreferences.
    do {
        var providersWithoutUserNotificationPreferences = db.users.aggregate([
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
                    "userType": "Provider"
                }]
            }
        },
        {
            $limit: batchLimit
        }]);

        print(`Found ${providersWithoutUserNotificationPreferences._batch.length} users without UserNotificationPreferences`);

        while (providersWithoutUserNotificationPreferences.hasNext())
        {
            var doc = providersWithoutUserNotificationPreferences.next();

            print(`Updating ${doc.name}`);

            var userNotificationPreferences = {
                _id: doc.idamsUserId,
                notificationTypes: 'VacancyRejectedByEmployer'
            };

            var writeResult = db.userNotificationPreferences.insert(userNotificationPreferences);

            if (writeResult.hasWriteConcernError())
            {
                printjson(writeResult.writeConcernError);
                quit(14);
            }

            print(`Updated ${doc.name}`);
        }

    } while (providersWithoutUserNotificationPreferences._batch.length >= batchLimit);
}