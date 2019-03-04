import AzureBlobStorage from './src/AzureBlobStorage';
import LocalFileStorage from './src/LocalFileStorage';
import AcceptedInitializer from './src/AcceptedInitializer';
require('dotenv').load();

let fileStorage = process.env.AZURE_STORAGE_CONTAINER_NAME 
     ? new AzureBlobStorage(process.env.AZURE_STORAGE_CONTAINER_NAME)
     : new LocalFileStorage();

let acceptedInitializer = new AcceptedInitializer(process.env.STYLEGUIDE_URL, process.env.EXCLUDE_PARTIALS, fileStorage);

acceptedInitializer.run()
.catch(err => {
    console.error(err);
    process.exit(1);
    }
);