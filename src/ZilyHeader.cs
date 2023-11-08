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
        public int Length => unicode_text.Length;

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
                unicode_text = value == null ? Array.Empty<byte>() : aes.Encrypt(value);
                text = value;
            }
        }

        byte[] unicode_text;
        string text;
        AesEncryption aes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="flag">
        /// The packet flag.
        /// </param>
        /// <param name="text">
        /// The packet text.
        /// </param>
        public ZilyHeader(AesEncryption aes, int flag, string text = null)
        {
            this.aes = aes;
            Flag = flag;
            Text = text;
        }

        private ZilyHeader(int flag, string text, byte[] unicode_text, AesEncryption aes)
        {
            this.aes = aes;
            Flag = flag;
            this.text = text;
            this.unicode_text = unicode_text;
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
        public static ZilyHeader Parse(Stream stream, AesEncryption aes)
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
                text = aes.Decrypt(buffer);
            }

            return new ZilyHeader(flag, text, buffer, aes);
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
            }.Concat(unicode_text)
            .ToArray();
        }
    }
}