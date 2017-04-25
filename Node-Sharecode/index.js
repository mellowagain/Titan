const csgo = require('csgo');

console.log(new csgo.SharecodeDecoder(process.argv[2].substring(61)).decode().matchId);

/*
 * Usage:
 * nodejs index.js <Sharelink>
 */
