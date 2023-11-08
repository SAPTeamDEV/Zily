using System;
using System.IO;

using Serilog;
using Serilog.Core;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyServerSide : ZilySide
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyServerSide"/>.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="System.IO.Stream"/> with read and write permission.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyServerSide(Stream stream, ILogger logger = null) : base(stream, logger) { }

        /// <summary>
        /// Waits until a client connects to the server and then establishes the Zily connection.
        /// </summary>
        public void Accept()
        {
            bool everConnected = false;

            logger.Debug("Waiting for client");
            Wait();

            while (true)
            {
                var header = ZilyHeader.Parse(Stream);
                if (!everConnected)
                {
                    logger.Information("A new client connected to the pipe server");
                    logger.Information("Establishing a Zily connection");
                    everConnected = true;
                }

                ParseHeader(header);

                if (header.Flag == ZilyHeaderFlag.Connected)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Waits before starting client acceptation.
        /// </summary>
        public virtual void Wait()
        {

        }
    }
}