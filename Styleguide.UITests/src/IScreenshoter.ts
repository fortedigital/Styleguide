import { Browser } from "puppeteer";

export interface IScreenshoter {
    makeScreenshot(browser: Browser, partialName: string, targetPath: string): Promise<void>;
}

export class Screenshoter implements IScreenshoter {
    private styleguideUrl : string;

    constructor(styleguideUrl: string) {
        this.styleguideUrl = styleguideUrl;
    }
    
    makeScreenshot = async (browser: Browser, partialName: string, targetPath: string): Promise<void> => {
        return new Promise(async (resolve, reject) => {
            try {
                const componentPage = await browser.newPage();
                componentPage.setDefaultNavigationTimeout(60000);
                await componentPage.goto(this.styleguideUrl + '/Component/' + partialName, { waitUntil: 'networkidle0' });
                const bodyHandle = await componentPage.$('body');
                if (bodyHandle == null) {
                    throw new Error('No body element');
                }

                const boundingBox = await bodyHandle.boundingBox();
                if (boundingBox == null) {
                    throw new Error('No bounding box');
                }

                const { width, height } = boundingBox;
        
                console.log('Screenshoting ' + partialName, targetPath);
                await componentPage.screenshot({
                    path: targetPath, clip: {
                        x: 0,
                        y: 0,
                        width,
                        height
                    }
                });
                resolve();
            } catch (err) {
                console.error(err);
                reject();
            }
        });
    }
}