const fs = require('fs');
const required = ['app/index.html','App.jsx','app/src/main.js'];
for (const file of required){
  if (!fs.existsSync(file)) {
    console.error(`Missing required file: ${file}`);
    process.exit(1);
  }
}
console.log('Totem smoke test passed.');
