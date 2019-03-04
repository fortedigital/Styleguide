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
        const componentPage = await browser.newPage();
        componentPage.setDefaultNavigationTimeout(60000);
        await componentPage.goto(this.styleguideUrl + '/Component/' + partialName, { waitUntil: 'networkidle0' });
        const bodyHandle = await componentPage.$('body');
        const { width, height } = await bodyHandle.boundingBox();

        console.log('Screenshoting ' + partialName);
        await componentPage.screenshot({
            path: targetPath, clip: {
                x: 0,
                y: 0,
                width,
                height
            }
        });
    }

}