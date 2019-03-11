import { IFileStorage } from "./IFileStorage";
import * as fs from 'fs';
import * as path from 'path';
import {
    SharedKeyCredential,
    StorageURL,
    ServiceURL,
    ContainerURL,
    Aborter,
    BlobURL,
    BlockBlobURL,
    uploadFileToBlockBlob,
} from '@azure/storage-blob';
import { ContainerListBlobFlatSegmentResponse } from "@azure/storage-blob/typings/lib/generated/lib/models";

export default class AzureBlobStorage implements IFileStorage {

    private containerURL: ContainerURL;

    constructor(containerName: string, accountName: string, accountKey: string) {
        const sharedKeyCredential = new SharedKeyCredential(accountName, accountKey);
        const pipeline = StorageURL.newPipeline(sharedKeyCredential);
        const serviceUrl = new ServiceURL(`https://${accountName}.blob.core.windows.net`, pipeline);
        this.containerURL = ContainerURL.fromServiceURL(serviceUrl, containerName);
    }

    doesBlobExist = async (partialPath: string): Promise<boolean> => {
        return new Promise<boolean>(async (resolve) => {
            const blobUrl = BlobURL.fromContainerURL(this.containerURL, partialPath);
            try {
                await blobUrl.getProperties(Aborter.none);
                resolve(true);
            } catch {
                resolve(false);
            }
        });
    }

    uploadBlobFromLocalFile = async (partialPath: string, targetPath: string): Promise<void> => {
        const blobURL = BlobURL.fromContainerURL(this.containerURL, targetPath);
        const blobBlockUrl = BlockBlobURL.fromBlobURL(blobURL);
        await uploadFileToBlockBlob(Aborter.none, partialPath, blobBlockUrl);
    }

    downloadReferencePartialBlob = async (partialPath: string, targetPath: string): Promise<void> => {
        return new Promise(async (resolve) => {
            const blobUrl = BlobURL.fromContainerURL(this.containerURL, partialPath);

            const downloadResponse = await blobUrl.download(Aborter.none, 0);
            const content = downloadResponse.readableStreamBody;
            if (!content) {
                throw new Error('no content from ' + partialPath)
            }

            let writeStream = fs.createWriteStream(targetPath);
            content.pipe(writeStream);

            writeStream.once("close", (fd: number) => {
                resolve();
            })
        });

    }

    clearBlobsInFolder = async (folderPath: string): Promise<void> => {
        let marker = undefined;
        do {
            let listBlobsResponse: ContainerListBlobFlatSegmentResponse = await this.containerURL.listBlobFlatSegment(
                Aborter.none,
                marker
            );

            marker = listBlobsResponse.nextMarker;
            for (const blob of listBlobsResponse.segment.blobItems) {
                if (path.dirname(blob.name) != folderPath) {
                    continue;
                }
                
                let blobUrl = BlobURL.fromContainerURL(this.containerURL, blob.name);
                blobUrl.delete(Aborter.none);
            }
        } while (marker);
    }
}