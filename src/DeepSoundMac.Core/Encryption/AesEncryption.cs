using System.Security.Cryptography;
using System.Text;

namespace DeepSoundMac.Core.Encryption;

/// <summary>
/// Provides AES-256 encryption and decryption functionality.
/// </summary>
public static class AesEncryption
{
    private const int KeySize = 256;
    private const int BlockSize = 128;
    private const int SaltSize = 32;
    private const int Iterations = 100000;

    /// <summary>
    /// Encrypts data using AES-256 with the provided password.
    /// </summary>
    /// <param name="data">The data to encrypt.</param>
    /// <param name="password">The password used for encryption.</param>
    /// <returns>The encrypted data with salt prepended.</returns>
    public static byte[] Encrypt(byte[] data, string password)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrEmpty(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);
        byte[] key = deriveBytes.GetBytes(KeySize / 8);
        byte[] iv = deriveBytes.GetBytes(BlockSize / 8);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

        // Prepend salt to encrypted data
        byte[] result = new byte[salt.Length + encryptedData.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(encryptedData, 0, result, salt.Length, encryptedData.Length);

        return result;
    }

    /// <summary>
    /// Decrypts data using AES-256 with the provided password.
    /// </summary>
    /// <param name="encryptedData">The encrypted data with salt prepended.</param>
    /// <param name="password">The password used for decryption.</param>
    /// <returns>The decrypted data.</returns>
    public static byte[] Decrypt(byte[] encryptedData, string password)
    {
        ArgumentNullException.ThrowIfNull(encryptedData);
        ArgumentException.ThrowIfNullOrEmpty(password);

        if (encryptedData.Length < SaltSize)
        {
            throw new ArgumentException("Encrypted data is too short.", nameof(encryptedData));
        }

        // Extract salt from the beginning
        byte[] salt = new byte[SaltSize];
        Buffer.BlockCopy(encryptedData, 0, salt, 0, SaltSize);

        // Extract actual encrypted data
        byte[] cipherData = new byte[encryptedData.Length - SaltSize];
        Buffer.BlockCopy(encryptedData, SaltSize, cipherData, 0, cipherData.Length);

        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);
        byte[] key = deriveBytes.GetBytes(KeySize / 8);
        byte[] iv = deriveBytes.GetBytes(BlockSize / 8);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);
    }

    /// <summary>
    /// Encrypts a string using AES-256.
    /// </summary>
    public static byte[] EncryptString(string plainText, string password)
    {
        byte[] data = Encoding.UTF8.GetBytes(plainText);
        return Encrypt(data, password);
    }

    /// <summary>
    /// Decrypts encrypted data back to a string.
    /// </summary>
    public static string DecryptToString(byte[] encryptedData, string password)
    {
        byte[] decrypted = Decrypt(encryptedData, password);
        return Encoding.UTF8.GetString(decrypted);
    }
}
