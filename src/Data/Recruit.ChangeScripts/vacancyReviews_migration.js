{
    var sourceCount = db.reviews.count();
    print((new Date()) + " Start migrating " + sourceCount + " documents from reviews to vacancyReviews collection.");

    db.reviews.find().forEach( function(x){db.vacancyReviews.insert(x);} );

    var targetCount = db.vacancyReviews.count();
    print((new Date()) + " Finished migrating " + targetCount + " to vacancyReviews documents.");
}