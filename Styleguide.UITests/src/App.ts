import * as puppeteer from 'puppeteer';
import * as looksSame from 'looks-same';
import { IFileStorage } from './IFileStorage';

export default class App {

    private styleguideUrl: string;
    private browser: puppeteer.Browser;
    private excludePartials: string[];

    private fileStorage : IFileStorage;

    constructor(styleguideUrl: string, excludePartialsString: string, fileStorage: IFileStorage) {
        this.styleguideUrl = styleguideUrl;
        this.excludePartials = excludePartialsString ? excludePartialsString.split(';') : [];

        this.fileStorage = fileStorage;
    }

    async fetchPartials(): Promise<string[]> {

        const page = await this.browser.newPage();

        page.setDefaultNavigationTimeout(120000);
        await page.goto(this.styleguideUrl, { waitUntil: 'networkidle0' });

        let partialNames = await page.evaluate(() => {
            let data = [];
            let elements = document.getElementsByClassName('Tree-item');
            for (let i = 0; i < elements.length; i++) {
                data.push(elements[i].querySelector('a').getAttribute('data-component'));
            }

            return data;
        });

        return partialNames;
    }

    async makeSingleScreenshot(partialName: string): Promise<void> {
        const componentPage = await this.browser.newPage();
        componentPage.setDefaultNavigationTimeout(60000);
        await componentPage.goto(this.styleguideUrl + '/Component/' + partialName, { waitUntil: 'networkidle0' });
        const bodyHandle = await componentPage.$('body');
        const { width, height } = await bodyHandle.boundingBox();

        console.log('Screenshoting ' + partialName);
        await componentPage.screenshot({
            path: 'screenshots/' + partialName + '.png', clip: {
                x: 0,
                y: 0,
                width,
                height
            }
        });
    }

    async comparePartialScreenshots(partialName: string) : Promise<boolean> {
        return new Promise((resolve, reject) => {
            console.log('Comparing ' + partialName);
            looksSame('screenshots/' + partialName + '.png', 'accepted/' + partialName + '.png', (error: Error | null, result: any) => {
                if (error) {
                    reject(error);
                }
                if (result && !result.equal) {
                    console.log('Creating diff image for ' + partialName);
                    looksSame.createDiff({
                        reference: 'accepted/' + partialName + '.png',
                        current: 'screenshots/' + partialName + '.png',
                        diff: 'diff/' + partialName + '.png',
                        highlightColor: '#ff00ff',
                        strict: false, // strict comparsion
                        tolerance: 2.5,
                        antialiasingTolerance: 0,
                        ignoreAntialiasing: true, // ignore antialising by default
                        ignoreCaret: true
                      }, (err) => {
                        if (err != null) {
                          console.error('Error creating image diff for ', partialName)
                          process.exit(1);
                        }
                        resolve(true);
                      });
                } else {
                    resolve(false)
                }
            });
        })
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
        this.browser = await puppeteer.launch();
        let partialNames = await this.fetchPartials();
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

            await this.makeSingleScreenshot(partialName);
            await this.fileStorage.uploadPartialBlob('current/' + partialName + '.png', 'screenshots/' + partialName + '.png');
            await this.fileStorage.downloadReferencePartialBlob('current/' + partialName + '.png', 'accepted/' + partialName + '.png')

            var ifDifferent = await this.comparePartialScreenshots(partialName);
            if (ifDifferent) {
                partialWithDifferences.push(partialName);    
                await this.fileStorage.uploadDiffPartialBlob('diff/' + partialName + '.png', 'diff/' + partialName + '.png');
            }
        }
        
        await this.browser.close();

        if (partialWithDifferences.length > 0) {
            console.log(partialWithDifferences.length + ' differences found')
            console.log(partialWithDifferences);
            process.exit(1);
        }
    }
}
