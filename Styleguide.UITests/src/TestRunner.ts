import * as puppeteer from 'puppeteer';
import { IFileStorage } from './IFileStorage';
import { IScreenshoter, Screenshoter } from './IScreenshoter';
import { IPartialDiscoveryService, PartialDiscoveryService } from './IPartialDiscoveryService';
import { IScreenshotComparer, ScreenshotComparer } from './IScreenshotComparer';

export default class TestRunner {

    private excludePartials: string[];

    private fileStorage : IFileStorage;
    private screenshoter : IScreenshoter;
    private partialDiscoveryService : IPartialDiscoveryService;
    private screenshotComparer : IScreenshotComparer;

    constructor(styleguideUrl: string, excludePartialsString: string, fileStorage: IFileStorage) {
        this.excludePartials = excludePartialsString ? excludePartialsString.split(';') : [];

        this.fileStorage = fileStorage;
        this.screenshoter = new Screenshoter(styleguideUrl);
        this.partialDiscoveryService = new PartialDiscoveryService(styleguideUrl);
        this.screenshotComparer = new ScreenshotComparer();
    }

    checkReferenceScreenshotsExistance = async (partialNames: string[]): Promise<string[]> => {
        let notExistingReferences : string[] = [];

        for (let i = 0; i < partialNames.length; i++) {
            let partialName = partialNames[i];
            if (this.excludePartials.indexOf(partialName) > -1) {
                continue;
            }

            let fileExists = await this.fileStorage.doesBlobExist('accepted/' + partialName + '.png');
            if (!fileExists) {
                notExistingReferences.push(partialName);
            }
        }

        return notExistingReferences;
    }

    async run(): Promise<void> {
        try {
            let browser = await puppeteer.launch();
            let partialNames = await this.partialDiscoveryService.fetchPartials(browser);
            console.log('Discovered ' + partialNames.length + ' partials');
    
            let notExistingReferencePartials = await this.checkReferenceScreenshotsExistance(partialNames);
            if (notExistingReferencePartials.length > 0) {
                console.log('Accepted screenshots are missing for following partials:', notExistingReferencePartials);
                process.exit(1);
            }
    
            let partialWithDifferences : string[] = [];
    
            for (let i = 0; i < partialNames.length; i++) {
                let partialName = partialNames[i];
                if (this.excludePartials.indexOf(partialName) > -1) {
                    continue;
                }
    
                await this.screenshoter.makeScreenshot(browser, partialName, 'screenshots/' + partialName + '.png');
                await this.fileStorage.uploadPartialBlob('current/' + partialName + '.png', 'screenshots/' + partialName + '.png');
                await this.fileStorage.downloadReferencePartialBlob('current/' + partialName + '.png', 'accepted/' + partialName + '.png')
    
                var ifDifferent = await this.screenshotComparer.compareScreenshots('screenshots/' + partialName + '.png', 'accepted/' + partialName + '.png', 'diff/' + partialName + '.png');
                if (ifDifferent) {
                    partialWithDifferences.push(partialName);    
                    await this.fileStorage.uploadDiffPartialBlob('diff/' + partialName + '.png', 'diff/' + partialName + '.png');
                }
            }
            
            await browser.close();

            if (partialWithDifferences.length > 0) {
                console.log(partialWithDifferences.length + ' differences found')
                console.log(partialWithDifferences);
                process.exit(1);
            }
        } catch(error) {
            console.error(error);
            process.exit(1);
        }
    }
}
