{
    const batchLimit = 500;
    var count = 0, limit = Number(db.users.count());

    // 2. Add VacancySentForReview preference to all Providers that don't have a userNotificationPreferences.
    do {
        print(`Loading user batch ${count * batchLimit} -> ${(count * batchLimit) + batchLimit} of ${limit}`);

        var employersWithoutUserNotificationPreferences = db.users.aggregate([
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
                        "userType": "Employer"
                    }]
            }
            }]);

        print(`Found ${employersWithoutUserNotificationPreferences._batch.length} users without UserNotificationPreferences`);

        while (employersWithoutUserNotificationPreferences.hasNext())
        {
            var doc = employersWithoutUserNotificationPreferences.next();

            print(`Updating ${doc.name}`);

            var userNotificationPreferences = {
                _id: doc.idamsUserId,
                notificationTypes: "VacancySentForReview",
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