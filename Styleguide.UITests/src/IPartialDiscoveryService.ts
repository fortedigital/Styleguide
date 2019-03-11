import { Browser } from "puppeteer";

export interface IPartialDiscoveryService {
    fetchPartials(browser: Browser): Promise<string[]>;
}

export class PartialDiscoveryService implements IPartialDiscoveryService {
    
    private styleguideUrl : string;
    constructor(styleguideUrl: string) {
        this.styleguideUrl = styleguideUrl;
    }

    fetchPartials = async (browser: Browser): Promise<string[]> => {
        const page = await browser.newPage();

        page.setDefaultNavigationTimeout(120000);
        await page.goto(this.styleguideUrl, { waitUntil: 'networkidle0' });

        let partialNames = await page.evaluate(() => {
            let data: string[] = [];
            let elements = document.getElementsByClassName('Tree-item');
            for (let i = 0; i < elements.length; i++) {
                let element = elements[i].querySelector('a');

                if (element == null) continue;
                let partialName = element.getAttribute('data-component');

                if (partialName == null) continue;
                data.push(partialName);
            }

            return data;
        });

        return partialNames;
    }

}