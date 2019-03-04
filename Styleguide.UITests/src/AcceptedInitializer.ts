import * as puppeteer from 'puppeteer';
import { IFileStorage } from "./IFileStorage";
import { IPartialDiscoveryService, PartialDiscoveryService } from "./IPartialDiscoveryService";
import { IScreenshoter, Screenshoter } from './IScreenshoter';

export default class AcceptedInitializer {
    private fileStorage: IFileStorage;
    private partialDiscoveryService: IPartialDiscoveryService;
    private screenshoter: IScreenshoter;
    private excludePartials: string[];

    constructor(styleguideUrl: string, excludePartialsString: string, fileStorage: IFileStorage) {
        this.fileStorage = fileStorage;
        this.partialDiscoveryService = new PartialDiscoveryService(styleguideUrl);
        this.screenshoter = new Screenshoter(styleguideUrl);
        this.excludePartials = excludePartialsString ? excludePartialsString.split(';') : [];
    }

    run = async () : Promise<void> =>  {
        try {
            let browser = await puppeteer.launch();
            let partialNames = await this.partialDiscoveryService.fetchPartials(browser);
            console.log('Discovered ' + partialNames.length + ' partials');
    
            for (let i = 0; i < partialNames.length; i++) {
                let partialName = partialNames[i];
                if (this.excludePartials.indexOf(partialName) > -1) {
                    continue;
                }
    
                await this.screenshoter.makeScreenshot(browser, partialName, 'accepted/' + partialName + '.png');
                await this.fileStorage.uploadPartialBlob('accepted/' + partialName + '.png', 'accepted/' + partialName + '.png');
            }
    
            await browser.close();
        } catch (error) {
            console.error(error);
            process.exit(1);
        }
        
        return;
    }
}
