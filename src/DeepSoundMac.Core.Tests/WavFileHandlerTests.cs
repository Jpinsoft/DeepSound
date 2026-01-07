using DeepSoundMac.Core.Audio;

namespace DeepSoundMac.Core.Tests;

public class WavFileHandlerTests
{
    [Fact]
    public void Write_Read_RoundTrip_PreservesData()
    {
        // Arrange
        var originalWav = new WavFile
        {
            NumChannels = 2,
            SampleRate = 44100,
            BitsPerSample = 16,
            AudioData = new byte[1000]
        };
        
        // Fill with some test data
        for (int i = 0; i < originalWav.AudioData.Length; i++)
        {
            originalWav.AudioData[i] = (byte)(i % 256);
        }

        using var stream = new MemoryStream();

        // Act
        WavFileHandler.Write(stream, originalWav);
        stream.Position = 0;
        var readWav = WavFileHandler.Read(stream);

        // Assert
        Assert.Equal(originalWav.NumChannels, readWav.NumChannels);
        Assert.Equal(originalWav.SampleRate, readWav.SampleRate);
        Assert.Equal(originalWav.BitsPerSample, readWav.BitsPerSample);
        Assert.Equal(originalWav.AudioData, readWav.AudioData);
    }

    [Fact]
    public void TotalSamples_CalculatesCorrectly_For16Bit()
    {
        // Arrange
        var wav = new WavFile
        {
            BitsPerSample = 16,
            AudioData = new byte[100] // 50 samples at 16-bit
        };

        // Act & Assert
        Assert.Equal(50, wav.TotalSamples);
    }

    [Fact]
    public void TotalSamples_CalculatesCorrectly_For8Bit()
    {
        // Arrange
        var wav = new WavFile
        {
            BitsPerSample = 8,
            AudioData = new byte[100] // 100 samples at 8-bit
        };

        // Act & Assert
        Assert.Equal(100, wav.TotalSamples);
    }

    [Fact]
    public void MaxHiddenDataBytes_CalculatesCorrectly()
    {
        // Arrange
        var wav = new WavFile
        {
            BitsPerSample = 16,
            AudioData = new byte[1600] // 800 samples, so 800 bits = 100 bytes
        };

        // Act & Assert
        Assert.Equal(100, wav.MaxHiddenDataBytes);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new WavFile
        {
            NumChannels = 2,
            SampleRate = 44100,
            BitsPerSample = 16,
            AudioData = new byte[] { 1, 2, 3, 4, 5 }
        };

        // Act
        var clone = WavFileHandler.Clone(original);
        clone.AudioData[0] = 255; // Modify clone

        // Assert
        Assert.Equal(1, original.AudioData[0]); // Original unchanged
        Assert.Equal(255, clone.AudioData[0]); // Clone modified
        Assert.Equal(original.NumChannels, clone.NumChannels);
        Assert.Equal(original.SampleRate, clone.SampleRate);
    }

    [Fact]
    public void Read_InvalidRiffHeader_ThrowsInvalidDataException()
    {
        // Arrange
        byte[] invalidData = "NOTARIFF"u8.ToArray();
        using var stream = new MemoryStream(invalidData);

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => WavFileHandler.Read(stream));
    }
}
