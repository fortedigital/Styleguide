"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const tl = require("azure-pipelines-task-lib/task");
const TestRunner_1 = require("../../Styleguide.UITests/src/TestRunner");
const AzureBlobStorage_1 = require("../../Styleguide.UITests/src/AzureBlobStorage");
const LocalFileStorage_1 = require("../../Styleguide.UITests/src/LocalFileStorage");
function run() {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const styleguideUrl = tl.getInput('StyleguideUrl', true);
            if (styleguideUrl == 'bad') {
                tl.setResult(tl.TaskResult.Failed, 'Bad input was given');
                return;
            }
            const excludePartials = tl.getInput('ExcludePartials');
            const azureStorageContainerName = tl.getInput('AzureStorageContainerName');
            const azureStorageAccountKey = tl.getInput('AzureStorageAccountKey');
            const azureStorageAccountName = tl.getInput("AzureStorageAccountName");
            let fileStorage = azureStorageContainerName
                ? new AzureBlobStorage_1.default(azureStorageContainerName, azureStorageAccountName, azureStorageAccountKey)
                : new LocalFileStorage_1.default();
            let testRunner = new TestRunner_1.default(styleguideUrl, excludePartials, fileStorage);
            testRunner.run();
        }
        catch (err) {
            tl.setResult(tl.TaskResult.Failed, err.message);
        }
    });
}
run();
