
// Scripts called by this script will raise Mongo exit codes as documented on the below link.
// https://docs.mongodb.com/manual/reference/exit-codes/

if (db !== "recruit") {
    db = db.getSiblingDB("recruit");
}

load("010-queryViews_LiveVacancy_Insert_DefaultField_ApplicationMethod.js");
load("020-vacancies_Insert_DefaultField_ApplicationMethod.js");
