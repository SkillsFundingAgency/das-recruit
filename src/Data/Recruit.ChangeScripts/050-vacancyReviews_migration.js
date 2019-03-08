{
    print((new Date()) + "Start migrating documents from reviews to vacancyReviews collection.");

    db.reviews.find().forEach( function(x){db.vacancyReviews.insert(x)} );

    print((new Date()) + "Finished migrating vacancyReviews documents.");
}