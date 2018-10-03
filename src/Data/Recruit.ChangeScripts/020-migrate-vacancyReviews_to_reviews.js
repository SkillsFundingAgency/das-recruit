{
    //Migrating documents from 'vacancyReviews' to 'reviews'
    //This script is idempotent
    print("Migrating documents from 'vacancyReviews' to 'reviews'");

    load("helper.js");

    let matchedDocs = db.vacancyReviews.find();

    while (matchedDocs.hasNext()) {
        let doc = matchedDocs.next();
        
        if(db.reviews.findOne({"_id" : doc._id}) == null){
            print(`Inserting '${toGUID(doc._id.hex())}'`);
            
            let writeResult = db.reviews.insert(doc);

            if (writeResult.hasWriteConcernError()) {
                printjson(writeResult.writeConcernError);
                quit(14);
            }
        }
        else{
            print(`Skipping '${toGUID(doc._id.hex())}'`);
        }
    }

    print("Finished migrating documents from 'vacancyReviews' to 'reviews'");
}