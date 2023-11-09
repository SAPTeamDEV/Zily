using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents a standard header for zily packets.
    /// </summary>
    public class ZilyHeader
    {
        /// <summary>
        /// Gets or Sets the packet flag.
        /// </summary>
        public int Flag { get; set; }

        /// <summary>
        /// Gets the packet text length.
        /// </summary>
        public int Length => Buffer.Length;

        /// <summary>
        /// Gets or Sets the packet text.
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                Buffer = value == null ? Array.Empty<byte>() : encryptor.Encrypt(value);
                text = value;
            }
        }

        public byte[] Buffer { get; set; }

        string text;
        IEncryption encryptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="flag">
        /// The packet flag.
        /// </param>
        /// <param name="text">
        /// The packet text.
        /// </param>
        public ZilyHeader(IEncryption encryptor, int flag, string text = null)
        {
            this.encryptor = encryptor;
            Flag = flag;
            Text = text;
        }

        public ZilyHeader(int flag, byte[] buffer, string text = null)
        {
            Flag = flag;
            Buffer = buffer;
            this.text = text;

            encryptor = Encryption.None;
        }

        /// <summary>
        /// Parses and creates a new instance of the <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="stream">
        /// The zily connection stream.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="ZilyHeader"/>.
        /// </returns>
        public static ZilyHeader Read(IEncryption encryptor, Stream stream)
        {
            int flag;
            string text = null;

            while (true)
            {
                int data = stream.ReadByte();
                if (data != -1)
                {
                    flag = data;
                    break;
                }
            }

            int length = Math.Max(0, (stream.ReadByte() * 256) + stream.ReadByte());
            byte[] buffer = new byte[length];

            if (length > 0)
            {
                stream.Read(buffer, 0, length);
                
                if (encryptor != Encryption.None)
                {
                    text = encryptor.Decrypt(buffer);
                }
            }

            return new ZilyHeader(flag, buffer, text);
        }

        /// <summary>
        /// Converts the header data to byte array.
        /// </summary>
        /// <returns>
        /// An array contains the flag, length and encoded text.
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public virtual byte[] ToByteArray()
        {
            if (Length > ushort.MaxValue)
            {
                throw new ArgumentException("Length is too long.");
            }

            return new byte[]
            {
                (byte)Flag,
                (byte)(Length / 256),
                (byte)(Length & 255)
            }.Concat(Buffer)
            .ToArray();
        }
    }
}