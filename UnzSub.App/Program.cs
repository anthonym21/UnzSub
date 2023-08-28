using Microsoft.Extensions.Configuration;

using System.IO.Compression;

using UnzSub.App;

// load appsettings.json
var config = new ConfigurationBuilder()
 .AddJsonFile("appsettings.json", optional: false)
 .Build();

// Read and assign settings from appsettings.json
string baseDirectory = config["BaseDirectory"] ?? "/mnt/m/Win-Backup/Sam-Laptop/BackupFiles";
string outputDirectory = config["OutputDirectoryName"] ?? "Output";
bool overwrite = bool.Parse(config["Overwrite"] ?? "true");
string subDirectoryPrefix = config["SubDirectoryPrefix"] ?? "Backup Files *";
string zipFilePrefix = config["ZipFilePrefix"] ?? "Backup files *";

// Get all subdirectories that match the pattern
var directories = Directory.GetDirectories(baseDirectory, subDirectoryPrefix);
var pops = new ParallelOptions { MaxDegreeOfParallelism = 4 };
CancellationTokenSource source = new();
CancellationToken token = source.Token;
await Parallel.ForEachAsync(directories, pops, async (dir, token) =>
{
    Console.WriteLine($"Processing {dir}...");
    // Create the Output directory if it doesn't exist
    string outputDir = Path.Combine(dir, outputDirectory);
    Directory.CreateDirectory(outputDir);

    // Get all zip files in the subdirectory
    var zipFiles = Directory.GetFiles(dir, zipFilePrefix);
    await Task.WhenAll(zipFiles.Select(async zipFile => await Util.UnzipFileAsync(zipFile, outputDir, overwrite)));
});

/*
// Use Parallel.ForEach for multithreading
await Task.Run(() => Parallel.ForEach(directories, dir =>
{
    // Create the Output directory if it doesn't exist
    string outputDir = Path.Combine(dir, outputDirectory);
    Directory.CreateDirectory(outputDir);

    // Get all zip files in the subdirectory
    var zipFiles = Directory.GetFiles(dir, zipFilePrefix);


    // Unzip each file
    foreach (var zipFile in zipFiles)
    {
        string destinationPath = outputDir;
        try
        {
            Console.WriteLine($"Unzipping {zipFile} to {destinationPath}...");  // Output the current file being unzipped

            ZipFile.ExtractToDirectory(zipFile, destinationPath, overwrite);
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"The zip file {zipFile} appears to be invalid: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Permission denied while unzipping {zipFile}: {ex.Message}");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"An I/O error occurred while unzipping {zipFile}: {ex.Message}");
        }
        catch (Exception ex)
        {
            // This will catch any other exceptions
            Console.WriteLine($"An unexpected error occurred while unzipping {zipFile}: {ex.Message}");
           
        }
    }
}));*/