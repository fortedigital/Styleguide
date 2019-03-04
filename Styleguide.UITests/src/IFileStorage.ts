export interface IFileStorage {
    doesBlobExist(partialPath : string) : Promise<boolean>;
    uploadPartialBlob(partialPath: string, targetPath: string): Promise<void>;
    downloadReferencePartialBlob(partialPath: string, targetPath: string): Promise<void>;
    uploadDiffPartialBlob(partialPath: string, targetPath: string) : Promise<void>;
}



