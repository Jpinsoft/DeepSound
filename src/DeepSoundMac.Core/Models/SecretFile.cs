using DeepSoundMac.Core.Utilities;

namespace DeepSoundMac.Core.Models;

/// <summary>
/// Represents a secret file to be hidden or extracted.
/// </summary>
public class SecretFile
{
    /// <summary>
    /// The original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// The file data.
    /// </summary>
    public byte[] Data { get; set; } = [];
    
    /// <summary>
    /// Creates a SecretFile from a file path.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when file path is null, empty, or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="IOException">Thrown when the file cannot be read.</exception>
    public static SecretFile FromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null, empty, or whitespace.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' does not exist.", filePath);
        }

        try
        {
            return new SecretFile
            {
                FileName = Path.GetFileName(filePath),
                Data = File.ReadAllBytes(filePath)
            };
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or NotSupportedException)
        {
            throw new IOException($"Failed to read secret file from path '{filePath}'.", ex);
        }
    }
    
    /// <summary>
    /// Saves the secret file to the specified directory.
    /// If a file with the same name exists, a unique name with a counter will be used.
    /// </summary>
    /// <returns>The actual path where the file was saved.</returns>
    public string SaveTo(string directoryPath)
    {
        string outputPath = FileUtilities.GetUniqueFilePath(directoryPath, FileName);
        File.WriteAllBytes(outputPath, Data);
        return outputPath;
    }
}

/// <summary>
/// Represents a payload containing multiple secret files.
/// </summary>
public class SecretPayload
{
    /// <summary>
    /// The list of secret files in this payload.
    /// </summary>
    public List<SecretFile> Files { get; set; } = [];
    
    /// <summary>
    /// Whether the payload is encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }
}
