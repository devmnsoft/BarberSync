const fs = require('fs');
const requiredFiles = [
  'App.js',
  'src/services/api.js',
  'src/screens/LoginScreen.js',
  'src/screens/ScheduleScreen.js',
  'src/screens/HistoryScreen.js'
];
for (const file of requiredFiles) {
  if (!fs.existsSync(file)) {
    console.error(`Missing required file: ${file}`);
    process.exit(1);
  }
}
console.log('Mobile smoke test passed.');
