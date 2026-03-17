// Minimal Node.js application to print command line arguments and environment variables

// To make the example more realistic and to show how to install dependencies in a Node.js app
const envinfo = require('envinfo');

envinfo.run(
    {
  System: ['OS', 'CPU', 'Memory'],
  Binaries: ['Node', 'npm', 'Yarn'],
  Browsers: ['Chrome', 'Firefox', 'Safari'],
  npmPackages: ['pg', 'dotenv'],
  npmGlobalPackages: ['npm', 'typescript']
    }, 
    { showNotFound: true }
)
.then(info => {
  console.log('envinfo:');
  console.log(info);
});

// Print command line arguments
console.log('Command line arguments (process.argv):');
process.argv.forEach((arg, index) => {
  console.log(`  [${index}]: ${arg}`);
});
console.log();

// Print all environment variables
console.log('Environment variables:', process.env, '\n');