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

