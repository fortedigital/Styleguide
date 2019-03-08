import * as path from 'path';
import { PartialFolder } from './PartialFolder';

export default class PathTools {
    public static ImageFileExtension: string = '.png';

    public static GetPartialPath = (fileName: string, directoryName: PartialFolder = PartialFolder.Empty) : string => {
        return path.join(directoryName, fileName + PathTools.ImageFileExtension);
    }
}
 
