const fs = require('fs');

const required = ['app/index.html', 'App.jsx', 'app/src/main.js'];
for (const file of required) {
  if (!fs.existsSync(file)) {
    console.error(`Missing required file: ${file}`);
    process.exit(1);
  }
}

const main = fs.readFileSync('app/src/main.js', 'utf8');
if (/http:\/\/(api|localhost):\d+/.test(main)) {
  console.error('Totem browser code must use the /KioskApi proxy instead of direct API hosts.');
  process.exit(1);
}

if (!main.includes('/KioskApi')) {
  console.error('Totem browser code must default to the /KioskApi proxy.');
  process.exit(1);
}

const app = fs.readFileSync('App.jsx', 'utf8');
for (const contract of ["setStep(saved.step)", "api.flow('DELETE')", 'Confirmar check-in e pré-comanda']) {
  if (!app.includes(contract)) {
    console.error(`Totem flow is missing contract: ${contract}`);
    process.exit(1);
  }
}

console.log('Totem smoke test passed.');
