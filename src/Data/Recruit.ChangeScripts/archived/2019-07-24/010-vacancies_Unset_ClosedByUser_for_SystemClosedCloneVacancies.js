{
    const oneSecondInMilliseconds = 1000;

    // https://stackoverflow.com/a/44564089/5596802
    function toGUID(hex) {
        let a = hex.substr(6, 2) + hex.substr(4, 2) + hex.substr(2, 2) + hex.substr(0, 2);
        let b = hex.substr(10, 2) + hex.substr(8, 2);
        let c = hex.substr(14, 2) + hex.substr(12, 2);
        let d = hex.substr(16, 16);
        hex = a + b + c + d;
        let uuid = hex.substr(0, 8) + "-" + hex.substr(8, 4) + "-" + hex.substr(12, 4) + "-" + hex.substr(16, 4) + "-" + hex.substr(20, 12);
        return uuid;
    }

    function getTimePortion(d) {
        let timePortionStartIndex = 11,
            timePortionLength = 8,
            isoString = d.toISOString();
        return isoString.substr(timePortionStartIndex, timePortionLength);
    }

    // vacancies are considered closed by the system overnight job if the time they were closed falls between 1 minute 20 seconds after midnight
    function isBetweenSystemClosingTime(timePortion) {
        let lowerTime = "00:00:00",
            upperTime = "00:01:20";
        return timePortion >= lowerTime && timePortion < upperTime;
    }

    function unsetClosedByUser(vac) {
        let originalClosedByUserJsonString = JSON.stringify(vac.closedByUser);
        print(`Unsetting 'closedByUser' for '${toGUID(vac._id.hex())}':${Number(vac.vacancyReference)}, original value: ${originalClosedByUserJsonString}`);
        /* eslint-disable */
        sleep(oneSecondInMilliseconds);
        /* eslint-disable */

        db.vacancies.updateOne({ "_id": vac._id }, { $unset: { "closedByUser": 1} });
    }

    print("Start treatment of incorrect closed vacancies from a clone where the 'closedByUser' field was copied from the source vacancy.");

    var closedCloneVacancies = db.vacancies.aggregate([
        { "$match":  { "status": "Closed", "sourceType": "Clone", "closedByUser": { "$exists": true } } },
        { "$project": { "vacancyReference": 1, "closedDate": 1, "closedByUser": 1 } }
    ]);

    print(`Found ${closedCloneVacancies.toArray().length} closed cloned vacancies that may have been closed by the system.`);


    let vacanciesThatNeedClosedByUserUnsetting = closedCloneVacancies.toArray()
                                                .filter(vac => {
                                                    var t = getTimePortion(vac.closedDate);
                                                    return isBetweenSystemClosingTime(t);
                                                }),
    vacanciesThatDoNotNeedClosedByUserUnsetting = closedCloneVacancies.toArray()
                                                .filter(vac => {
                                                    var t = getTimePortion(vac.closedDate);
                                                    return isBetweenSystemClosingTime(t) === false;
                                                });


    print(`Found ${vacanciesThatNeedClosedByUserUnsetting.length} vacancies that needs unsetting of 'closedByUser' field.`);
    print();
    print();
    print("------------------------ Vacancies closed by user ------------------------");
    print();
    vacanciesThatDoNotNeedClosedByUserUnsetting.forEach(vac => print(`Vacancy ${Number(vac.vacancyReference)} was closed by user ${vac.closedByUser.email} at: ${vac.closedDate.toISOString()} - time portion ${getTimePortion(vac.closedDate)}`));
    print();
    print();
    print("------------------------ Vacancies closed by system ------------------------");
    print();
    vacanciesThatNeedClosedByUserUnsetting.forEach(vac => print(`Vacancy ${Number(vac.vacancyReference)} was closed by the system at: ${vac.closedDate.toISOString()} - time portion ${getTimePortion(vac.closedDate)}`));
    print();
    print();
    print("------------------------ Unset 'closedByUser' on closed clone vacancies closed by system ------------------------");
    print();
    vacanciesThatNeedClosedByUserUnsetting.forEach(vac => unsetClosedByUser(vac));

    print();
    print();
    print("Finished treatment of incorrect closed vacancies from a clone where the 'closedByUser' field was copied from the source vacancy.");
}