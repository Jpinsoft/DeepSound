using DeepSoundMac.Core.Utilities;

namespace DeepSoundMac.Core.Tests;

public class FileUtilitiesTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(512, "512 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1.0 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1572864, "1.50 MB")]
    public void FormatBytes_FormatsCorrectly(long bytes, string expected)
    {
        // Act
        string result = FileUtilities.FormatBytes(bytes);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetUniqueFilePath_ReturnsOriginalPath_WhenFileDoesNotExist()
    {
        // Arrange
        string tempDir = Path.GetTempPath();
        string uniqueName = $"test_{Guid.NewGuid()}.txt";

        // Act
        string result = FileUtilities.GetUniqueFilePath(tempDir, uniqueName);

        // Assert
        Assert.Equal(Path.Combine(tempDir, uniqueName), result);
    }

    [Fact]
    public void GetUniqueFilePath_AppendsCounter_WhenFileExists()
    {
        // Arrange
        string tempDir = Path.GetTempPath();
        string testFileName = $"test_unique_{Guid.NewGuid()}.txt";
        string existingPath = Path.Combine(tempDir, testFileName);
        
        try
        {
            // Create the file so it exists
            File.WriteAllText(existingPath, "test");

            // Act
            string result = FileUtilities.GetUniqueFilePath(tempDir, testFileName);

            // Assert
            string expectedName = Path.GetFileNameWithoutExtension(testFileName);
            Assert.Contains($"{expectedName} (1)", result);
            Assert.EndsWith(".txt", result);
        }
        finally
        {
            // Cleanup
            if (File.Exists(existingPath))
            {
                File.Delete(existingPath);
            }
        }
    }

    [Fact]
    public void SanitizeFileName_RemovesInvalidCharacters()
    {
        // Arrange - use null character which is invalid on all platforms
        string input = "test\0file.txt";

        // Act
        string result = FileUtilities.SanitizeFileName(input);

        // Assert - the result should not contain the null character
        // The actual sanitization splits on invalid chars and joins with "_"
        Assert.False(result.Contains('\0'));
        Assert.True(result.Length > 0);
    }
    
    [Fact]
    public void SanitizeFileName_ReturnsUnnamedFile_WhenEmpty()
    {
        // Act
        string result = FileUtilities.SanitizeFileName("");

        // Assert
        Assert.Equal("unnamed_file", result);
    }

    [Fact]
    public void SanitizeFileName_TruncatesLongFileNames()
    {
        // Arrange
        string longName = new string('a', 300) + ".txt";

        // Act
        string result = FileUtilities.SanitizeFileName(longName);

        // Assert
        Assert.True(result.Length <= 204); // 200 + ".txt"
        Assert.EndsWith(".txt", result);
    }
}
