{
    print("Starting deletion of MinimumWageRanges from referenceData");

    db.referenceData.remove({"_id" : "MinimumWageRanges"});

    print("Finished deletion of MinimumWageRanges from referenceData");
}