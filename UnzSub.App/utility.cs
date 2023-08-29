using System.IO.Compression;

namespace UnzSub.App
{
    public static class Util
    {
        public static async Task UnzipFileAsync(string zipFile, string destinationPath, bool overwrite)
        {
            using FileStream inputStream = new(zipFile, FileMode.Open, FileAccess.Read);
            using ZipArchive archive = new(inputStream, ZipArchiveMode.Read, false);
            Console.WriteLine($"> Opening {zipFile} to go into {destinationPath}");
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            await Parallel.ForEachAsync(archive.Entries,  async (entry, token) =>
            {
                string fullOutputPath = Path.Combine(destinationPath, entry.FullName);
                if (!Directory.Exists(Path.GetDirectoryName(fullOutputPath)))
                {
                    string directoryPath = Path.GetDirectoryName(fullOutputPath);
                    Directory.CreateDirectory(directoryPath);
                }
                if (entry.Length == 0)
                {
                    return;
                }

                using Stream outputStream =
                    new FileStream(fullOutputPath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
                Console.Write($"{entry.Name}..");

                await entry.Open().CopyToAsync(outputStream, token);
                File.SetLastWriteTime(fullOutputPath, entry.LastWriteTime.LocalDateTime);
                File.SetCreationTime(fullOutputPath, entry.LastWriteTime.LocalDateTime);

            });
            Console.WriteLine($"> Unpacked {zipFile} to {destinationPath}");
        }
    }
}