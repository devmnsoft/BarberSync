const fs = require('fs');
const requiredFiles = [
  'index.js',
  'App.js',
  'src/services/api.js',
  'src/theme/colors.js',
  'src/theme/spacing.js',
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

const appSource = fs.readFileSync('App.js', 'utf8');
for (const importPath of ["./src/theme/colors", "./src/theme/spacing", "./src/services/api"]) {
  if (!appSource.includes(importPath)) {
    console.error(`App.js is missing required demo import: ${importPath}`);
    process.exit(1);
  }
}

for (const jsFile of ['App.js', 'src/services/api.js', 'src/theme/colors.js', 'src/theme/spacing.js']) {
  require('child_process').execFileSync(process.execPath, ['--check', jsFile], { stdio: 'inherit' });
}
console.log('Mobile smoke test passed.');
