using System.Text;
using DeepSoundMac.Core.Audio;
using DeepSoundMac.Core.Encryption;
using DeepSoundMac.Core.Models;

namespace DeepSoundMac.Core.Steganography;

/// <summary>
/// Provides audio steganography functionality using LSB (Least Significant Bit) encoding.
/// </summary>
public static class AudioSteganography
{
    // Magic bytes to identify DeepSound data
    private static readonly byte[] MagicBytes = "DEEPSND"u8.ToArray();
    private const byte Version = 1;
    
    /// <summary>
    /// Encodes secret files into a WAV audio file.
    /// </summary>
    /// <param name="carrierWav">The carrier WAV file.</param>
    /// <param name="secretFiles">The files to hide.</param>
    /// <param name="password">Optional password for encryption.</param>
    /// <returns>A new WAV file with hidden data.</returns>
    public static WavFile Encode(WavFile carrierWav, IEnumerable<SecretFile> secretFiles, string? password = null)
    {
        ArgumentNullException.ThrowIfNull(carrierWav);
        ArgumentNullException.ThrowIfNull(secretFiles);
        
        // Serialize the payload
        byte[] payload = SerializePayload(secretFiles.ToList());
        
        // Optionally encrypt
        bool isEncrypted = !string.IsNullOrEmpty(password);
        if (isEncrypted)
        {
            payload = AesEncryption.Encrypt(payload, password!);
        }
        
        // Build the complete data to hide
        byte[] dataToHide = BuildDataPacket(payload, isEncrypted);
        
        // Check capacity
        if (dataToHide.Length * 8 > carrierWav.TotalSamples)
        {
            throw new InvalidOperationException(
                $"The carrier audio file is too small. " +
                $"Maximum capacity: {carrierWav.MaxHiddenDataBytes} bytes ({carrierWav.TotalSamples} bits), " +
                $"Required: {dataToHide.Length} bytes ({dataToHide.Length * 8} bits).");
        }
        
        // Clone the carrier and embed data
        var outputWav = WavFileHandler.Clone(carrierWav);
        EmbedDataLsb(outputWav.AudioData, dataToHide, carrierWav.BitsPerSample);
        
        return outputWav;
    }
    
    /// <summary>
    /// Decodes secret files from a WAV audio file.
    /// </summary>
    /// <param name="stegoWav">The WAV file containing hidden data.</param>
    /// <param name="password">Password for decryption (if encrypted).</param>
    /// <returns>The extracted secret files.</returns>
    public static SecretPayload Decode(WavFile stegoWav, string? password = null)
    {
        ArgumentNullException.ThrowIfNull(stegoWav);
        
        // Extract the data packet
        byte[] extractedData = ExtractDataPacket(stegoWav.AudioData, stegoWav.BitsPerSample);
        
        // Parse the data packet
        int offset = 0;
        
        // Verify magic bytes
        for (int i = 0; i < MagicBytes.Length; i++)
        {
            if (extractedData[offset + i] != MagicBytes[i])
            {
                throw new InvalidDataException("No hidden data found or invalid format.");
            }
        }
        offset += MagicBytes.Length;
        
        // Version
        byte version = extractedData[offset++];
        if (version != Version)
        {
            throw new InvalidDataException($"Unsupported data version: {version}");
        }
        
        // Flags
        byte flags = extractedData[offset++];
        bool isEncrypted = (flags & 0x01) != 0;
        
        // Payload length
        int payloadLength = BitConverter.ToInt32(extractedData, offset);
        offset += 4;
        
        // Extract payload
        byte[] payload = new byte[payloadLength];
        Array.Copy(extractedData, offset, payload, 0, payloadLength);
        
        // Decrypt if needed
        if (isEncrypted)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("The hidden data is encrypted. Please provide a password.");
            }
            
            try
            {
                payload = AesEncryption.Decrypt(payload, password);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to decrypt. The password may be incorrect.", ex);
            }
        }
        
        // Deserialize the payload
        var files = DeserializePayload(payload);
        
        return new SecretPayload
        {
            Files = files,
            IsEncrypted = isEncrypted
        };
    }
    
    /// <summary>
    /// Checks if a WAV file contains hidden data.
    /// </summary>
    public static bool ContainsHiddenData(WavFile wavFile)
    {
        try
        {
            // Extract just enough to check magic bytes
            byte[] header = ExtractBytesLsb(wavFile.AudioData, 0, MagicBytes.Length, wavFile.BitsPerSample);
            
            for (int i = 0; i < MagicBytes.Length; i++)
            {
                if (header[i] != MagicBytes[i])
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Checks if the hidden data is encrypted.
    /// </summary>
    public static bool IsEncrypted(WavFile wavFile)
    {
        if (!ContainsHiddenData(wavFile))
        {
            return false;
        }
        
        // Extract header including flags
        int headerSize = MagicBytes.Length + 1 + 1; // magic + version + flags
        byte[] header = ExtractBytesLsb(wavFile.AudioData, 0, headerSize, wavFile.BitsPerSample);
        
        byte flags = header[MagicBytes.Length + 1];
        return (flags & 0x01) != 0;
    }
    
    private static byte[] BuildDataPacket(byte[] payload, bool isEncrypted)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        // Magic bytes
        writer.Write(MagicBytes);
        
        // Version
        writer.Write(Version);
        
        // Flags (bit 0 = encrypted)
        byte flags = 0;
        if (isEncrypted) flags |= 0x01;
        writer.Write(flags);
        
        // Payload length
        writer.Write(payload.Length);
        
        // Payload
        writer.Write(payload);
        
        return ms.ToArray();
    }
    
    private static byte[] SerializePayload(List<SecretFile> files)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, Encoding.UTF8);
        
        // Number of files
        writer.Write(files.Count);
        
        foreach (var file in files)
        {
            // File name length and name
            byte[] nameBytes = Encoding.UTF8.GetBytes(file.FileName);
            writer.Write(nameBytes.Length);
            writer.Write(nameBytes);
            
            // File data length and data
            writer.Write(file.Data.Length);
            writer.Write(file.Data);
        }
        
        return ms.ToArray();
    }
    
    private static List<SecretFile> DeserializePayload(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms, Encoding.UTF8);
        
        var files = new List<SecretFile>();
        
        int fileCount = reader.ReadInt32();
        
        for (int i = 0; i < fileCount; i++)
        {
            // File name
            int nameLength = reader.ReadInt32();
            byte[] nameBytes = reader.ReadBytes(nameLength);
            string fileName = Encoding.UTF8.GetString(nameBytes);
            
            // File data
            int dataLength = reader.ReadInt32();
            byte[] fileData = reader.ReadBytes(dataLength);
            
            files.Add(new SecretFile
            {
                FileName = fileName,
                Data = fileData
            });
        }
        
        return files;
    }
    
    private static void EmbedDataLsb(byte[] audioData, byte[] dataToHide, short bitsPerSample)
    {
        int bytesPerSample = bitsPerSample / 8;
        int totalBits = dataToHide.Length * 8;
        int maxSamples = audioData.Length / bytesPerSample;
        int iterations = Math.Min(totalBits, maxSamples);
        
        for (int i = 0; i < iterations; i++)
        {
            int sampleOffset = i * bytesPerSample;
            int byteIndex = i / 8;
            int bitPosition = 7 - (i % 8);
            
            byte bit = (byte)((dataToHide[byteIndex] >> bitPosition) & 1);
            
            // Modify the LSB of the sample
            audioData[sampleOffset] = (byte)((audioData[sampleOffset] & 0xFE) | bit);
        }
    }
    
    private static byte[] ExtractDataPacket(byte[] audioData, short bitsPerSample)
    {
        // First extract header to determine payload size
        int headerSize = MagicBytes.Length + 1 + 1 + 4; // magic + version + flags + length
        byte[] header = ExtractBytesLsb(audioData, 0, headerSize, bitsPerSample);
        
        // Get payload length
        int payloadLength = BitConverter.ToInt32(header, MagicBytes.Length + 2);
        
        // Now extract full data
        int totalSize = headerSize + payloadLength;
        return ExtractBytesLsb(audioData, 0, totalSize, bitsPerSample);
    }
    
    private static byte[] ExtractBytesLsb(byte[] audioData, int startBit, int byteCount, short bitsPerSample)
    {
        int bytesPerSample = bitsPerSample / 8;
        byte[] result = new byte[byteCount];
        int totalBits = byteCount * 8;
        
        for (int i = 0; i < totalBits; i++)
        {
            int sampleIndex = startBit + i;
            int sampleOffset = sampleIndex * bytesPerSample;
            
            if (sampleOffset >= audioData.Length)
            {
                throw new InvalidDataException("Not enough audio data to extract hidden content.");
            }
            
            byte bit = (byte)(audioData[sampleOffset] & 1);
            int byteIndex = i / 8;
            int bitPosition = 7 - (i % 8);
            
            result[byteIndex] |= (byte)(bit << bitPosition);
        }
        
        return result;
    }
}
