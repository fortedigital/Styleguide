import * as puppeteer from 'puppeteer';
import * as azure from 'azure-storage';
import * as looksSame from 'looks-same';

export default class App {

    private styleguideUrl: string;
    private azureStorageContainerName: string;
    private browser: puppeteer.Browser;
    private blobService: azure.BlobService;
    private excludePartials: string[];
    private useAzureStorage: boolean;

    constructor(styleguideUrl: string, azureStorageContainerName: string, excludePartialsString: string) {
        this.styleguideUrl = styleguideUrl;
        this.azureStorageContainerName = azureStorageContainerName;
        this.excludePartials = excludePartialsString ? excludePartialsString.split(';') : [];
        this.useAzureStorage = !!azureStorageContainerName;
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

    async uploadPartialBlob(partialName: string): Promise<void> {
        console.log('Uploading ' + partialName);
        await this.blobService.createBlockBlobFromLocalFile(this.azureStorageContainerName, 'current/' + partialName + '.png', 'screenshots/' + partialName + '.png', (error) => {
            if (error != null) {
                console.error('Error uploading file', error)
            }
        });
    }

    async uploadDiffPartialBlob(partialName: string) : Promise<void> {
        console.log('Uploading diff for' + partialName);
        await this.blobService.createBlockBlobFromLocalFile(this.azureStorageContainerName, 'diff/' + partialName + '.png', 'diff/' + partialName + '.png', (error) => {
            if (error != null) {
                console.error('Error uploading file', error)
            }
        });
    }

    async downloadReferencePartialBlob(partialName: string): Promise<void> {
        return new Promise((resolve, reject) => {
            console.log('Downloading reference screenshot ' + partialName);
            this.blobService.getBlobToLocalFile(this.azureStorageContainerName, 'accepted/' + partialName + '.png', 'accepted/' + partialName + '.png', (error: Error) => {
                if (error != null) {
                    console.error('Error downloading file', error);
                    reject(error);
                } else {
                    resolve();
                }
            })
        })
        
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

    async run(): Promise<void> {
        this.browser = await puppeteer.launch();
        this.blobService = azure.createBlobService();
        let partialNames = await this.fetchPartials();
        console.log('Discovered ' + partialNames.length + ' partials');
        let partialWithDifferences : string[] = [];

        for (let i = 0; i < partialNames.length; i++) {
            let partialName = partialNames[i];
            if (this.excludePartials.indexOf(partialName) > -1) {
                continue;
            }
            await this.makeSingleScreenshot(partialName);

            if (this.useAzureStorage) {
                await this.uploadPartialBlob(partialName);
                await this.downloadReferencePartialBlob(partialName)
            }

            var ifDifferent = await this.comparePartialScreenshots(partialName);
            if (ifDifferent) {
                partialWithDifferences.push(partialName);    
                if (this.useAzureStorage) {
                    await this.uploadDiffPartialBlob(partialName);
                }
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
