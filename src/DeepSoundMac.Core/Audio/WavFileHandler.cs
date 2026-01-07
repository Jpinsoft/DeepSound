namespace DeepSoundMac.Core.Audio;

/// <summary>
/// Represents WAV audio file data.
/// </summary>
public class WavFile
{
    public short NumChannels { get; set; }
    public int SampleRate { get; set; }
    public short BitsPerSample { get; set; }
    public byte[] AudioData { get; set; } = [];
    
    /// <summary>
    /// Gets the total number of samples in the audio data.
    /// </summary>
    public int TotalSamples => AudioData.Length / (BitsPerSample / 8);
    
    /// <summary>
    /// Gets the maximum amount of data (in bytes) that can be hidden in this audio file.
    /// Uses LSB steganography (1 bit per sample).
    /// </summary>
    public int MaxHiddenDataBytes => TotalSamples / 8;
}

/// <summary>
/// Provides functionality for reading and writing WAV audio files.
/// </summary>
public static class WavFileHandler
{
    private const int HeaderSize = 44;
    
    /// <summary>
    /// Reads a WAV file from the specified path.
    /// </summary>
    public static WavFile Read(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        
        using var stream = File.OpenRead(filePath);
        return Read(stream);
    }
    
    /// <summary>
    /// Reads a WAV file from a stream.
    /// </summary>
    public static WavFile Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        
        using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
        
        // RIFF header
        string riffHeader = new(reader.ReadChars(4));
        if (riffHeader != "RIFF")
        {
            throw new InvalidDataException("Invalid WAV file: Missing RIFF header.");
        }
        
        _ = reader.ReadInt32(); // fileSize (unused)
        
        string waveHeader = new(reader.ReadChars(4));
        if (waveHeader != "WAVE")
        {
            throw new InvalidDataException("Invalid WAV file: Missing WAVE header.");
        }
        
        // fmt subchunk
        string fmtHeader = new(reader.ReadChars(4));
        if (fmtHeader != "fmt ")
        {
            throw new InvalidDataException("Invalid WAV file: Missing fmt header.");
        }
        
        int fmtChunkSize = reader.ReadInt32();
        short audioFormat = reader.ReadInt16();
        
        if (audioFormat != 1) // PCM
        {
            throw new InvalidDataException("Only PCM WAV files are supported.");
        }
        
        var wavFile = new WavFile
        {
            NumChannels = reader.ReadInt16(),
            SampleRate = reader.ReadInt32()
        };
        
        _ = reader.ReadInt32(); // byteRate (unused)
        _ = reader.ReadInt16(); // blockAlign (unused)
        wavFile.BitsPerSample = reader.ReadInt16();
        
        // Skip any extra format bytes
        if (fmtChunkSize > 16)
        {
            reader.ReadBytes(fmtChunkSize - 16);
        }
        
        // Find data chunk (skip any other chunks)
        const int MaxChunksToSearch = 100; // Prevent infinite loop
        int chunksSearched = 0;
        
        while (chunksSearched < MaxChunksToSearch)
        {
            if (stream.Position >= stream.Length - 8)
            {
                throw new InvalidDataException("No data chunk found in WAV file.");
            }
            
            string chunkId = new(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();
            
            if (chunkId == "data")
            {
                wavFile.AudioData = reader.ReadBytes(chunkSize);
                break;
            }
            else
            {
                // Skip this chunk
                if (chunkSize > 0 && stream.Position + chunkSize <= stream.Length)
                {
                    reader.ReadBytes(chunkSize);
                }
                else
                {
                    throw new InvalidDataException($"Invalid chunk size: {chunkSize}");
                }
            }
            
            chunksSearched++;
        }
        
        if (chunksSearched >= MaxChunksToSearch)
        {
            throw new InvalidDataException("No data chunk found after searching maximum chunks.");
        }
        
        return wavFile;
    }
    
    /// <summary>
    /// Writes a WAV file to the specified path.
    /// </summary>
    public static void Write(string filePath, WavFile wavFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(wavFile);
        
        using var stream = File.Create(filePath);
        Write(stream, wavFile);
    }
    
    /// <summary>
    /// Writes a WAV file to a stream.
    /// </summary>
    public static void Write(Stream stream, WavFile wavFile)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(wavFile);
        
        using var writer = new BinaryWriter(stream, System.Text.Encoding.Default, leaveOpen: true);
        
        int dataSize = wavFile.AudioData.Length;
        int fileSize = HeaderSize + dataSize - 8;
        int byteRate = wavFile.SampleRate * wavFile.NumChannels * (wavFile.BitsPerSample / 8);
        short blockAlign = (short)(wavFile.NumChannels * (wavFile.BitsPerSample / 8));
        
        // RIFF header
        writer.Write("RIFF".ToCharArray());
        writer.Write(fileSize);
        writer.Write("WAVE".ToCharArray());
        
        // fmt subchunk
        writer.Write("fmt ".ToCharArray());
        writer.Write(16); // Subchunk1Size for PCM
        writer.Write((short)1); // AudioFormat (PCM)
        writer.Write(wavFile.NumChannels);
        writer.Write(wavFile.SampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(wavFile.BitsPerSample);
        
        // data subchunk
        writer.Write("data".ToCharArray());
        writer.Write(dataSize);
        writer.Write(wavFile.AudioData);
    }
    
    /// <summary>
    /// Creates a copy of the WAV file with the same properties.
    /// </summary>
    public static WavFile Clone(WavFile source)
    {
        return new WavFile
        {
            NumChannels = source.NumChannels,
            SampleRate = source.SampleRate,
            BitsPerSample = source.BitsPerSample,
            AudioData = (byte[])source.AudioData.Clone()
        };
    }
}
