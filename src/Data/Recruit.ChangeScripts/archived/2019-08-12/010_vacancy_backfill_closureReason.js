{
    const sleepTime = 500;
    const autoClosure = "Auto";
    const manualClosure = "Manual";

    function setClosureReason(vac) {
        print(`updating ${vac.vacancyReference}`);
        db.vacancies.updateOne({ "_id": vac._id }, { $set: { "closureReason": vac.closureReason} });
    }

    var closedVacancies = db.vacancies.aggregate([
        { $match : {"status" : "Closed", "closureReason": {$exists: 0} }},
        { $project : {
            "vacancyReference": 1, 
            "closedByUser": 1, 
            "closureReason": {
                $cond: {
                    if: { $eq: ["$closedByUser", undefined] },
                    then: autoClosure,
                    else: manualClosure
                }
            }
        }}
    ]).toArray();

    let counter = 0,
        autoCounter = 0,
        manualCounter = 0;

    closedVacancies.forEach(vac => {
        counter++;
        var counterMessage = vac.closureReason === autoClosure ? `AutoCounter: ${++autoCounter}` : `ManualCounter: ${++manualCounter}`;
        print(`${counter}. Ref: ${vac.vacancyReference}, Reason: ${vac.closureReason}, ${counterMessage}`);

        setClosureReason(vac);

        if ((counter % 100) == 0) {
            print(`stopping for a bit at counter ${counter}... `);
            /* eslint-disable */
            sleep(sleepTime);
            /* eslint-disable */
            print(`resuming at ${counter}...`);
        }
    });

    print("Counts: ");
    print(`Closed vacancies: ${counter}, Manually closed vacancies: ${manualCounter}, Automatically closed vacancies: ${autoCounter}`);
}
