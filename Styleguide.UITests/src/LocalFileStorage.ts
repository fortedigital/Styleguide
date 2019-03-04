import { IFileStorage } from "./IFileStorage";
import * as fs from 'fs';

export default class LocalFileStorage implements IFileStorage {
    uploadPartialBlob = async (partialPath: string, targetPath: string): Promise<void> => {
        return;
    }
    
    downloadReferencePartialBlob = async (partialPath: string, targetPath: string): Promise<void> => {
        return;
    }
    
    uploadDiffPartialBlob = async (partialPath: string, targetPath: string): Promise<void> => {
        return;
    }
    
    doesBlobExist = async (partialPath: string): Promise<boolean> => {
        return new Promise<boolean>((resolve) => {
            fs.exists(partialPath, (exists: boolean) => {
                resolve(exists);
            });
        });
    }
}