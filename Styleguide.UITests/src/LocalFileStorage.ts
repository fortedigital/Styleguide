import { IFileStorage } from "./IFileStorage";
import * as fs from 'fs';
import * as path from 'path';
import PathTools from "./PathTools";

export default class LocalFileStorage implements IFileStorage {
    uploadBlobFromLocalFile = async (partialPath: string, targetPath: string): Promise<void> => {
        return;
    }
    
    downloadReferencePartialBlob = async (partialPath: string, targetPath: string): Promise<void> => {
        return;
    }
    
    doesBlobExist = async (partialPath: string): Promise<boolean> => {
        return new Promise<boolean>((resolve) => {
            fs.exists(partialPath, (exists: boolean) => {
                resolve(exists);
            });
        });
    }

    clearBlobsInFolder = async (folderPath: string) : Promise<void> => {
        fs.readdir(folderPath, (err, files) => {
            for (const file of files) {
                if (file.indexOf(PathTools.ImageFileExtension) == -1) {
                    continue;
                }

                fs.unlink(path.join(folderPath, file), err => {
                    if (err) throw err;
                })
            }
        })
    }
}