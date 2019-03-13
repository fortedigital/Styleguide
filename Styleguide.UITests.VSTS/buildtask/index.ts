import tl = require('azure-pipelines-task-lib/task');
import TestRunner from 'StyleguideUiTests/src/TestRunner';
import AzureBlobStorage from 'StyleguideUiTests/src/AzureBlobStorage';
import LocalFileStorage from 'StyleguideUiTests/src/LocalFileStorage';


async function run() {
    try {
        const styleguideUrl: string = tl.getInput('StyleguideUrl', true);
        if (styleguideUrl == 'bad') {
            tl.setResult(tl.TaskResult.Failed, 'Bad input was given');
            return;
        }

        const excludePartials: string = tl.getInput('ExcludePartials');
        const azureStorageContainerName: string = tl.getInput('AzureStorageContainerName');
        const azureStorageAccountKey: string = tl.getInput('AzureStorageAccountKey');
        const azureStorageAccountName: string = tl.getInput("AzureStorageAccountName");

        let fileStorage = azureStorageContainerName
            ? new AzureBlobStorage(azureStorageContainerName, azureStorageAccountName, azureStorageAccountKey)
            : new LocalFileStorage();
        
        let testRunner = new TestRunner(styleguideUrl, excludePartials, fileStorage);
        testRunner.run()
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();
