using DeepSoundMac.Core.Encryption;

namespace DeepSoundMac.Core.Tests;

public class AesEncryptionTests
{
    [Fact]
    public void Encrypt_Decrypt_RoundTrip_ReturnsOriginalData()
    {
        // Arrange
        byte[] originalData = "Hello, World! This is a test message."u8.ToArray();
        string password = "TestPassword123!";

        // Act
        byte[] encrypted = AesEncryption.Encrypt(originalData, password);
        byte[] decrypted = AesEncryption.Decrypt(encrypted, password);

        // Assert
        Assert.Equal(originalData, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesLargerOutput_DueToSaltAndPadding()
    {
        // Arrange
        byte[] originalData = "Test"u8.ToArray();
        string password = "password";

        // Act
        byte[] encrypted = AesEncryption.Encrypt(originalData, password);

        // Assert
        Assert.True(encrypted.Length > originalData.Length);
        // Should have at least 32 bytes salt + some padding
        Assert.True(encrypted.Length >= 32 + 16);
    }

    [Fact]
    public void Decrypt_WithWrongPassword_ThrowsException()
    {
        // Arrange
        byte[] originalData = "Secret data"u8.ToArray();
        string correctPassword = "correct";
        string wrongPassword = "wrong";

        byte[] encrypted = AesEncryption.Encrypt(originalData, correctPassword);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => AesEncryption.Decrypt(encrypted, wrongPassword));
    }

    [Fact]
    public void Encrypt_WithEmptyData_Succeeds()
    {
        // Arrange
        byte[] emptyData = [];
        string password = "password";

        // Act
        byte[] encrypted = AesEncryption.Encrypt(emptyData, password);
        byte[] decrypted = AesEncryption.Decrypt(encrypted, password);

        // Assert
        Assert.Empty(decrypted);
    }

    [Fact]
    public void Encrypt_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        string password = "password";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AesEncryption.Encrypt(null!, password));
    }

    [Fact]
    public void Encrypt_WithEmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        byte[] data = "test"u8.ToArray();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AesEncryption.Encrypt(data, ""));
    }

    [Fact]
    public void EncryptString_DecryptToString_RoundTrip_Succeeds()
    {
        // Arrange
        string originalText = "Hello, World! 日本語 🎉";
        string password = "UnicodePassword™";

        // Act
        byte[] encrypted = AesEncryption.EncryptString(originalText, password);
        string decrypted = AesEncryption.DecryptToString(encrypted, password);

        // Assert
        Assert.Equal(originalText, decrypted);
    }

    [Fact]
    public void Encrypt_SameData_ProducesDifferentOutput_DueToRandomSalt()
    {
        // Arrange
        byte[] data = "Same data"u8.ToArray();
        string password = "password";

        // Act
        byte[] encrypted1 = AesEncryption.Encrypt(data, password);
        byte[] encrypted2 = AesEncryption.Encrypt(data, password);

        // Assert
        Assert.NotEqual(encrypted1, encrypted2);
    }
}
