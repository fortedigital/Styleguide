import App from './src/App';
require('dotenv').load();

let app = new App(process.env.STYLEGUIDE_URL, process.env.AZURE_STORAGE_CONTAINER_NAME, process.env.EXCLUDE_PARTIALS);

app.run()
    .catch(err => {
        throw err;
        process.exit(1);
    });