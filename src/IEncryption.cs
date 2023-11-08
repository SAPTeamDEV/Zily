namespace SAPTeam.Zily
{
    public interface IEncryption
    {
        string Decrypt(byte[] cipherText);
        byte[] Encrypt(string plainText);
    }
}