# Minimal Node.js application to print command line arguments and environment variables

# Move to the app directory
cd apps/nodejs-env/app
echo "CURRENT DIRECTORY: $(pwd)"

# Create the app
npm init -y

## Install dependencies (to make the example more realistic and to show how to install dependencies in a Node.js app)
npm install envinfo

## Reinstalling dependencies 
npm install

# Run the app with 2 arguments:
node app.js 'arg1' 'arg2'
