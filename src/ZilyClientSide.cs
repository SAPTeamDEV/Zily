using System;
using System.IO;

using Serilog;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyClientSide : ZilySide
    {
        /// <summary>
        /// Gets or Sets the other side client/server information.
        /// </summary>
        protected ISide ServerSide { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyClientSide"/>.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="System.IO.Stream"/> with read and write permission.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyClientSide(Stream stream, ILogger logger = null) : base(stream, logger) { }

        /// <inheritdoc/>
        public override void ParseHeader(ZilyHeader header)
        {
            switch (header.Flag)
            {
                case ZilyHeaderFlag.Disconnected:
                    Logger.Information("Zily server did shutdown");
                    break;
            }

            base.ParseHeader(header);
        }

        public override void ParseResponse(ZilyHeader header)
        {
            switch (LastRequest)
            {
                case ZilyHeaderFlag.SideIdentifier:
                    var otherSide = Parse(header.Text);
                    if (Protocol != otherSide.Protocol || Version.Major != otherSide.Version.Major)
                    {
                        Logger.Fatal("Cannot connect to the server");
                        Close();
                    }
                    else
                    {
                        ServerSide = otherSide;
                    }
                    break;
                case ZilyHeaderFlag.AesKey:
                    aesEnncryptor.Key = header.Buffer;
                    break;
                case ZilyHeaderFlag.AesIV:
                    aesEnncryptor.IV = header.Buffer;
                    break;
                default:
                    base.ParseResponse(header);
                    break;
            }
        }

        /// <summary>
        /// Establishes a Zily connection through a named pipe.
        /// </summary>
        public void Connect()
        {
            Status = ZilySideStatus.Connecting;
            Logger.Information("Establishing a Zily connection");
            Logger.Information("Requesting secret key");
            Send(new ZilyHeader(Encryption.None, ZilyHeaderFlag.AesKey));
            Logger.Information("Requesting IV");
            Send(new ZilyHeader(Encryption.None, ZilyHeaderFlag.AesIV));
            isSecured = true;
            Logger.Information("Secure connection established");

            okHeader = new ZilyHeader(Encryptor, ZilyHeaderFlag.Ok);
            Send(new ZilyHeader(Encryptor, ZilyHeaderFlag.SideIdentifier));

            if (Status == ZilySideStatus.Offline)
            {
                return;
            }

            WriteCommand(new ZilyHeader(Encryptor, ZilyHeaderFlag.Connected));
            Status = ZilySideStatus.Online;

            Logger.Information("Connected to {name}", ServerSide.Name);
        }
    }
}