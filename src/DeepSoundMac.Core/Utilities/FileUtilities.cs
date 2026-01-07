namespace DeepSoundMac.Core.Utilities;

/// <summary>
/// Provides common file-related utility methods.
/// </summary>
public static class FileUtilities
{
    /// <summary>
    /// Formats a byte count into a human-readable string.
    /// </summary>
    /// <param name="bytes">The number of bytes to format.</param>
    /// <returns>A formatted string like "1.5 KB" or "2.3 MB".</returns>
    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }

    /// <summary>
    /// Gets a unique file path by appending a number if a file already exists.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="fileName">The original file name.</param>
    /// <returns>A unique file path that doesn't exist.</returns>
    public static string GetUniqueFilePath(string directoryPath, string fileName)
    {
        string outputPath = Path.Combine(directoryPath, fileName);

        if (!File.Exists(outputPath))
        {
            return outputPath;
        }

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        int counter = 1;

        do
        {
            string candidateFileName = $"{fileNameWithoutExtension} ({counter}){extension}";
            outputPath = Path.Combine(directoryPath, candidateFileName);
            counter++;
        } while (File.Exists(outputPath));

        return outputPath;
    }

    /// <summary>
    /// Sanitizes a file name by replacing or removing invalid characters.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>A sanitized file name safe for use on the file system.</returns>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return "unnamed_file";
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Ensure the file name isn't too long (max 255 characters for most file systems)
        const int maxLength = 200;
        if (sanitized.Length > maxLength)
        {
            string extension = Path.GetExtension(sanitized);
            string nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
            int maxNameLength = maxLength - extension.Length;
            sanitized = nameWithoutExt[..Math.Min(nameWithoutExt.Length, maxNameLength)] + extension;
        }

        return sanitized;
    }
}
