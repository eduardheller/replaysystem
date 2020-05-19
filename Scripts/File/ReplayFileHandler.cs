using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace IREX.ReplaySystem.File
{
    public class ReplayFileHandler
    {

        private readonly string _directory;
        private readonly string _fileExtension;
        public static string CurrentReplayFileName { get; set; }

        public ReplayFileHandler(string directory, string fileExtension)
        {
            this._directory = directory;
            this._fileExtension = fileExtension;
        }
    
        public void SaveReplayStreamToFile(string filename,  System.IO.Stream stream)
        {
            var directory =  Application.dataPath + "/" + _directory;
            var path = directory + filename + _fileExtension;
            
            Debug.Log(directory);
            if (!SimpleFileBrowser.FileBrowserHelpers.DirectoryExists(directory))
                SimpleFileBrowser.FileBrowserHelpers.CreateFolderInDirectory(Application.dataPath, _directory);
            
            
            using (var file = new FileStream(path,
                FileMode.Create, FileAccess.Write))
            {
                using (DeflateStream deflateZipStream = new DeflateStream(file, CompressionMode.Compress))
                {
                    try
                    {
                        var bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, (int)stream.Length);
                        deflateZipStream.Write(bytes, 0, bytes.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    
        public MemoryStream ReadReplayHeaderFromFile(string filename)
        {
            var ms = new MemoryStream();
            var directory =  Application.dataPath + "/" + _directory;
            var path = directory + filename + _fileExtension;
            using (var file = new FileStream(path,
                FileMode.Open, FileAccess.ReadWrite))
            {
                using (var decompressionStream = new DeflateStream(file, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(ms);
                    decompressionStream.Close();
                }
  
            }
            return ms;
        }
    
        public MemoryStream GetReplayStreamFromFile(string filename)
        {
            var ms = new MemoryStream();
            var directory =  Application.dataPath + "/" + _directory;
            var path = directory + filename + _fileExtension;
            using (var file = new FileStream(path,
                FileMode.Open, FileAccess.ReadWrite))
            {
                using (var decompressionStream = new DeflateStream(file, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(ms);
                    decompressionStream.Close();
                }
  
            }
            return ms;
        }

        public bool ReplayExist(string filename)
        {

            var directory =  Application.dataPath + "/" + _directory;
            var path = directory + filename + _fileExtension;
            if (System.IO.File.Exists(path)) return true;
            Debug.Log("Replayfile in: "+path+" does not exist");
            return false;

        }
    
    
    


    }
}
