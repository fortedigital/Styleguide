import { IFileStorage } from "./IFileStorage";
import * as azure from 'azure-storage';

export default class AzureBlobStorage implements IFileStorage {
    
    private azureStorageContainerName : string;
    private blobService: azure.BlobService;

    constructor(azureStorageContainerName: string) {
        this.azureStorageContainerName = azureStorageContainerName;
        this.blobService = azure.createBlobService();
    }

    async doesBlobExist(partialPath: string): Promise<boolean> {
        return new Promise<boolean>((resolve => {
            this.blobService.doesBlobExist(this.azureStorageContainerName, partialPath, (error:Error, result: azure.BlobService.BlobResult) => {
                resolve(result.exists);
            });
        }));
    }

    async uploadPartialBlob(partialPath: string, targetPath: string): Promise<void> {
        await this.blobService.createBlockBlobFromLocalFile(this.azureStorageContainerName, partialPath, targetPath, (error) => {
            if (error != null) {
                console.error('Error uploading file', error)
            }
        });
    }

    async downloadReferencePartialBlob(partialPath: string, targetPath: string): Promise<void> {
        return new Promise((resolve, reject) => {
            this.blobService.getBlobToLocalFile(this.azureStorageContainerName, partialPath, targetPath, (error: Error) => {
                if (error != null) {
                    console.error('Error downloading file', error);
                    reject(error);
                } else {
                    resolve();
                }
            })
        })
        
    }

    async uploadDiffPartialBlob(partialPath: string, targetPath: string) : Promise<void> {
        await this.blobService.createBlockBlobFromLocalFile(this.azureStorageContainerName, partialPath, targetPath, (error) => {
            if (error != null) {
                console.error('Error uploading file', error)
            }
        });
    }
}