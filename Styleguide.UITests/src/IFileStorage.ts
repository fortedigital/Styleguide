export interface IFileStorage {
    doesBlobExist(partialPath : string) : Promise<boolean>;
    uploadBlobFromLocalFile(partialPath: string, targetPath: string): Promise<void>;
    downloadReferencePartialBlob(partialPath: string, targetPath: string): Promise<void>;
    clearBlobsInFolder(folderPath: string) : Promise<void>;
}



