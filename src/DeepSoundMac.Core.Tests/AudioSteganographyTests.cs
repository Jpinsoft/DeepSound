using DeepSoundMac.Core.Audio;
using DeepSoundMac.Core.Models;
using DeepSoundMac.Core.Steganography;

namespace DeepSoundMac.Core.Tests;

public class AudioSteganographyTests
{
    private static WavFile CreateTestWavFile(int audioDataSize = 10000)
    {
        return new WavFile
        {
            NumChannels = 2,
            SampleRate = 44100,
            BitsPerSample = 16,
            AudioData = new byte[audioDataSize]
        };
    }

    [Fact]
    public void Encode_Decode_RoundTrip_ReturnsOriginalFiles()
    {
        // Arrange
        var carrier = CreateTestWavFile(50000);
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "test.txt", Data = "Hello, World!"u8.ToArray() },
            new() { FileName = "data.bin", Data = new byte[] { 1, 2, 3, 4, 5 } }
        };

        // Act
        var encodedWav = AudioSteganography.Encode(carrier, secretFiles);
        var decoded = AudioSteganography.Decode(encodedWav);

        // Assert
        Assert.Equal(2, decoded.Files.Count);
        Assert.Equal("test.txt", decoded.Files[0].FileName);
        Assert.Equal("Hello, World!"u8.ToArray(), decoded.Files[0].Data);
        Assert.Equal("data.bin", decoded.Files[1].FileName);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, decoded.Files[1].Data);
    }

    [Fact]
    public void Encode_Decode_WithEncryption_RoundTrip_Succeeds()
    {
        // Arrange
        var carrier = CreateTestWavFile(50000);
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "secret.txt", Data = "Top Secret Data"u8.ToArray() }
        };
        string password = "SecurePassword123!";

        // Act
        var encodedWav = AudioSteganography.Encode(carrier, secretFiles, password);
        var decoded = AudioSteganography.Decode(encodedWav, password);

        // Assert
        Assert.Single(decoded.Files);
        Assert.True(decoded.IsEncrypted);
        Assert.Equal("secret.txt", decoded.Files[0].FileName);
        Assert.Equal("Top Secret Data"u8.ToArray(), decoded.Files[0].Data);
    }

    [Fact]
    public void ContainsHiddenData_AfterEncoding_ReturnsTrue()
    {
        // Arrange
        var carrier = CreateTestWavFile();
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "test.txt", Data = "Test"u8.ToArray() }
        };

        // Act
        var encodedWav = AudioSteganography.Encode(carrier, secretFiles);

        // Assert
        Assert.True(AudioSteganography.ContainsHiddenData(encodedWav));
    }

    [Fact]
    public void ContainsHiddenData_EmptyCarrier_ReturnsFalse()
    {
        // Arrange
        var carrier = CreateTestWavFile();

        // Act & Assert
        Assert.False(AudioSteganography.ContainsHiddenData(carrier));
    }

    [Fact]
    public void IsEncrypted_WithEncryptedData_ReturnsTrue()
    {
        // Arrange
        var carrier = CreateTestWavFile(50000);
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "test.txt", Data = "Test"u8.ToArray() }
        };

        // Act
        var encodedWav = AudioSteganography.Encode(carrier, secretFiles, "password");

        // Assert
        Assert.True(AudioSteganography.IsEncrypted(encodedWav));
    }

    [Fact]
    public void IsEncrypted_WithUnencryptedData_ReturnsFalse()
    {
        // Arrange
        var carrier = CreateTestWavFile();
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "test.txt", Data = "Test"u8.ToArray() }
        };

        // Act
        var encodedWav = AudioSteganography.Encode(carrier, secretFiles);

        // Assert
        Assert.False(AudioSteganography.IsEncrypted(encodedWav));
    }

    [Fact]
    public void Decode_WithWrongPassword_ThrowsException()
    {
        // Arrange
        var carrier = CreateTestWavFile(50000);
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "test.txt", Data = "Test"u8.ToArray() }
        };

        var encodedWav = AudioSteganography.Encode(carrier, secretFiles, "correct");

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => AudioSteganography.Decode(encodedWav, "wrong"));
    }

    [Fact]
    public void Decode_EncryptedWithoutPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var carrier = CreateTestWavFile(50000);
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "test.txt", Data = "Test"u8.ToArray() }
        };

        var encodedWav = AudioSteganography.Encode(carrier, secretFiles, "password");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => AudioSteganography.Decode(encodedWav));
    }

    [Fact]
    public void Encode_CarrierTooSmall_ThrowsInvalidOperationException()
    {
        // Arrange
        var smallCarrier = CreateTestWavFile(100); // Very small
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "large.txt", Data = new byte[1000] } // Too large for carrier
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            AudioSteganography.Encode(smallCarrier, secretFiles));
    }

    [Fact]
    public void Decode_NoHiddenData_ThrowsInvalidDataException()
    {
        // Arrange
        var carrier = CreateTestWavFile();

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => AudioSteganography.Decode(carrier));
    }

    [Fact]
    public void Encode_MultipleFiles_PreservesOrder()
    {
        // Arrange
        var carrier = CreateTestWavFile(100000);
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "file1.txt", Data = "First"u8.ToArray() },
            new() { FileName = "file2.txt", Data = "Second"u8.ToArray() },
            new() { FileName = "file3.txt", Data = "Third"u8.ToArray() }
        };

        // Act
        var encodedWav = AudioSteganography.Encode(carrier, secretFiles);
        var decoded = AudioSteganography.Decode(encodedWav);

        // Assert
        Assert.Equal(3, decoded.Files.Count);
        Assert.Equal("file1.txt", decoded.Files[0].FileName);
        Assert.Equal("file2.txt", decoded.Files[1].FileName);
        Assert.Equal("file3.txt", decoded.Files[2].FileName);
    }

    [Fact]
    public void Encode_UnicodeFileNames_PreservesCorrectly()
    {
        // Arrange
        var carrier = CreateTestWavFile(50000);
        var secretFiles = new List<SecretFile>
        {
            new() { FileName = "文档.txt", Data = "中文内容"u8.ToArray() },
            new() { FileName = "ドキュメント.txt", Data = "日本語"u8.ToArray() }
        };

        // Act
        var encodedWav = AudioSteganography.Encode(carrier, secretFiles);
        var decoded = AudioSteganography.Decode(encodedWav);

        // Assert
        Assert.Equal("文档.txt", decoded.Files[0].FileName);
        Assert.Equal("ドキュメント.txt", decoded.Files[1].FileName);
    }
}
