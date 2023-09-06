using Microsoft.Extensions.Configuration;
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
var directories = Directory.EnumerateDirectories(baseDirectory, subDirectoryPrefix);
CancellationToken token = new CancellationTokenSource().Token;
// Use Parallel.ForEach for multithreading
foreach(var dir in directories)
{
    Console.WriteLine($"Processing {dir}...");
    // Create the Output directory if it doesn't exist
    string outputDir = Path.Combine(dir, outputDirectory);
    if (!Directory.Exists(outputDir))
        Directory.CreateDirectory(outputDir);

    // Get all zip files in the subdirectory
    var zipFiles = Directory.EnumerateFiles(dir, zipFilePrefix);

    // Unzip each file
    foreach (var zipFile in zipFiles)
    {
        string destinationPath = outputDir;
        try
        {
            Console.WriteLine($"\r\n****************************Unzipping {zipFile} to {destinationPath}...**********************************\r\n");  // Output the current file being unzipped

            await Util.UnzipFileAsync(zipFile, destinationPath, overwrite, token);
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
}