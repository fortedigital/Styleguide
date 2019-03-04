import { IFileStorage } from "./IFileStorage";
import * as fs from 'fs';

export default class LocalFileStorage implements IFileStorage {
    async uploadPartialBlob(partialPath: string, targetPath: string): Promise<void> {
        return;
    }
    
    async downloadReferencePartialBlob(partialPath: string, targetPath: string): Promise<void> {
        return;
    }
    
    async uploadDiffPartialBlob(partialPath: string, targetPath: string): Promise<void> {
        return;
    }
    
    async doesBlobExist(partialPath: string): Promise<boolean> {
        return new Promise<boolean>((resolve) => {
            fs.exists(partialPath, (exists: boolean) => {
                resolve(exists);
            });
        });
    }
}