import * as looksSame from 'looks-same';

export interface IScreenshotComparer {
    compareScreenshots(actualPath: string, expectedPath: string, diffPath: string) : Promise<boolean>;
}

export class ScreenshotComparer implements IScreenshotComparer {
    async compareScreenshots(actualPath: string, expectedPath: string, diffPath: string): Promise<boolean> {
        return new Promise((resolve, reject) => {
            console.log('Comparing ' + actualPath);
            looksSame(actualPath, expectedPath, (error: Error | null, result: any) => {
                if (error) {
                    reject(error);
                }
                if (result && !result.equal) {
                    looksSame.createDiff({
                        reference: expectedPath,
                        current: actualPath,
                        diff: diffPath,
                        highlightColor: '#ff00ff',
                        strict: false, // strict comparsion
                        tolerance: 2.5,
                        antialiasingTolerance: 0,
                        ignoreAntialiasing: true, // ignore antialising by default
                        ignoreCaret: true
                      }, (err) => {
                        if (err != null) {
                          console.error('Error creating image diff for ', actualPath)
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
}