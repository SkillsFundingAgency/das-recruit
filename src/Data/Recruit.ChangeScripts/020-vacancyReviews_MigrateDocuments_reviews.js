{
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

    //Migrating documents from 'vacancyReviews' to 'reviews'
    //This script is idempotent
    print("Migrating documents from 'vacancyReviews' to 'reviews'");

    print(`vacancyReview collection document count=${db.vacancyReviews.count().toNumber()}`);
    print(`review collection document count=${db.reviews.count().toNumber()}`);

    let matchedDocs = db.vacancyReviews.find();

    while (matchedDocs.hasNext()) {
        let doc = matchedDocs.next();
        
        if(db.reviews.findOne({"_id" : doc._id}) === null){
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

    print(`vacancyReview collection document count=${db.vacancyReviews.count().toNumber()}`);
    print(`review collection document count=${db.reviews.count().toNumber()}`);

    print("Finished migrating documents from 'vacancyReviews' to 'reviews'");
}