using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    public class Encryption : IEncryption
    {
        public static IEncryption None {  get; } = new Encryption();

        public string Decrypt(byte[] cipherText)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(string plainText)
        {
            throw new NotImplementedException();
        }
    }
}
