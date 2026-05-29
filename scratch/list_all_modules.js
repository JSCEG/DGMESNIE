const fs = require('fs');
const data = JSON.parse(fs.readFileSync('scratch/all_modules.json', 'utf8'));
console.log("=== ALL ACTIVE MODULE CONTROLLERS AND ACTIONS ===");
data.modules.forEach(m => {
    console.log(`ID: ${m.ModuloId} | Title: ${m.Title} | Controller: ${m.Controller} | Action: ${m.Action} | Perfiles: ${m.Perfiles}`);
});
