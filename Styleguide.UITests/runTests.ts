import TestRunner from './src/TestRunner';
import AzureBlobStorage from './src/AzureBlobStorage';
import LocalFileStorage from './src/LocalFileStorage';
require('dotenv').load();

let fileStorage = process.env.AZURE_STORAGE_CONTAINER_NAME 
     ? new AzureBlobStorage(process.env.AZURE_STORAGE_CONTAINER_NAME)
     : new LocalFileStorage();

let testRunner = new TestRunner(process.env.STYLEGUIDE_URL, process.env.EXCLUDE_PARTIALS, fileStorage);

testRunner.run()
    .catch(err => {
        console.error(err);
        process.exit(1);
    });