var reviewsCollection = db.getCollection("reviews");
if (reviewsCollection) {
    print("Dropping collection `reviews`");
    reviewsCollection.drop();
}