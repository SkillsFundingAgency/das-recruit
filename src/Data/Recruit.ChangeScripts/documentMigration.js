
// Scripts called by this script will raise Mongo exit codes as documented on the below link.
// https://docs.mongodb.com/manual/reference/exit-codes/

{
    const targetDbName = "recruit";

    if (db !== targetDbName) {
        db = db.getSiblingDB(targetDbName);
    }

    let changeScriptFileRegex = /^\.\/\d{3}.*js$/,
        changeScripts = ls().filter(scr => changeScriptFileRegex.test(scr)).sort(function(a, b){return a - b;});

    print(`Found ${changeScripts.length} change scripts to run:`);
    changeScripts.forEach(scr => print(scr));

    changeScripts.forEach(scr => load(scr));
}
