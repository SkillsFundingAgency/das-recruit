{
    const batchLimit = 100;
    var count = 0, limit = Number(db.users.count());

    // 2. Add VacancyRejectedByEmployer preference to all Providers that don't have a userNotificationPreferences.
    do {
        print(`Loading user batch ${count * batchLimit} -> ${(count * batchLimit) + batchLimit} of ${limit}`);

        var providersWithoutUserNotificationPreferences = db.users.aggregate([
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
                        "Preferences": []
                    },
                    {
                        "userType": "Provider"
                    }]
            }
            }]);

        print(`Found ${providersWithoutUserNotificationPreferences._batch.length} users without UserNotificationPreferences`);

        while (providersWithoutUserNotificationPreferences.hasNext())
        {
            var doc = providersWithoutUserNotificationPreferences.next();

            print(`Updating ${doc.name}`);

            var userNotificationPreferences = {
                _id: doc.idamsUserId,
                notificationTypes: "VacancyRejectedByEmployer",
                notificationScope: "OrganisationVacancies"
            };

            var writeResult = db.userNotificationPreferences.insert(userNotificationPreferences);

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