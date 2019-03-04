import { IFileStorage } from "./IFileStorage";
import * as azure from 'azure-storage';

export default class AzureBlobStorage implements IFileStorage {
    
    private azureStorageContainerName : string;
    private blobService: azure.BlobService;

    constructor(azureStorageContainerName: string) {
        this.azureStorageContainerName = azureStorageContainerName;
        this.blobService = azure.createBlobService();
    }

    doesBlobExist = async (partialPath: string): Promise<boolean> => {
        return new Promise<boolean>((resolve => {
            this.blobService.doesBlobExist(this.azureStorageContainerName, partialPath, (error:Error, result: azure.BlobService.BlobResult) => {
                resolve(result.exists);
            });
        }));
    }

    uploadPartialBlob = async (partialPath: string, targetPath: string): Promise<void> => {
        await this.blobService.createBlockBlobFromLocalFile(this.azureStorageContainerName, partialPath, targetPath, (error) => {
            if (error != null) {
                console.error('Error uploading file', error)
            }
        });
    }

    downloadReferencePartialBlob = async (partialPath: string, targetPath: string): Promise<void> => {
        return new Promise((resolve, reject) => {
            this.blobService.getBlobToLocalFile(this.azureStorageContainerName, partialPath, targetPath, (error: Error) => {
                if (error != null) {
                    console.error('Error downloading file', error);
                    reject(error);
                } else {
                    resolve();
                }
            })
        });
        
    }

    uploadDiffPartialBlob = async (partialPath: string, targetPath: string) : Promise<void> => {
        await this.blobService.createBlockBlobFromLocalFile(this.azureStorageContainerName, partialPath, targetPath, (error) => {
            if (error != null) {
                console.error('Error uploading file', error)
            }
        });
    }
}