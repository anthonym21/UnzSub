using System;
using System.IO;


using SharpCompress.Common;
using SharpCompress.IO;
using SharpCompress.Readers;
using SharpCompress.Readers.Zip;

using SharpCompress.Writers;

using ZipArchive = SharpCompress.Archives.Zip.ZipArchive;

namespace UnzSub.App
{
    public static class Util
    {
        public static async Task UnzipFileAsync(string zipFile, string destinationPath, bool overwrite, CancellationToken token = default)
        {
            //Use ForwardOnlyStream instead of FileStream.
            await Task.Run(() =>
            {
                using var stream = new FileStream(zipFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920);
                using var archive = ZipReader.Open(stream);
                while (archive.MoveToNextEntry())
                {

                    var entry = archive.Entry;
                    if (entry.IsDirectory)
                    {
                        return;
                    }

                    Console.Write($"{entry.Key}..");
                    var fullOutputPath = Path.Combine(destinationPath, entry.Key);
                    if (!Directory.Exists(Path.GetDirectoryName(fullOutputPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
                    }
                    archive.WriteEntryToDirectory(destinationPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = overwrite,
                        PreserveFileTime = true
                    });
                }

                /*   var entry = archive.Entry;
                   if (entry.IsDirectory)
                       continue;
                   Console.Write($"{entry.Key}..");
                   var fullOutputPath = Path.Combine(destinationPath, entry.Key);
                   if (!Directory.Exists(Path.GetDirectoryName(fullOutputPath)))
                   {
                       Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath)!);
                   }
                   archive.WriteEntryToDirectory(destinationPath, new ExtractionOptions()
                   {
                       ExtractFullPath = true,
                       Overwrite = overwrite,
                       PreserveFileTime = true
                   });   */
            }  
            ,token);

            Console.WriteLine($"> Unzipped {zipFile} to {destinationPath}");
        }
    }
}