using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

using Serilog;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents a <see cref="ZilyPipeStream"/> to act as a pipe server.
    /// </summary>
    public class ZilyPipeServerStream : ZilyPipeStream
    {
        readonly NamedPipeServerStream _serverStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyPipeServerStream"/>.
        /// </summary>
        /// <param name="pipeServer">
        /// An instance of the <see cref="NamedPipeServerStream"/> that supports In-Out directions.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyPipeServerStream(NamedPipeServerStream pipeServer, ILogger logger = null) : base(pipeServer, logger)
        {
            _serverStream = pipeServer;
        }

        /// <summary>
        /// Waits until a client connects to the server and then establishes the Zily connection.
        /// </summary>
        public void Accept()
        {
            logger.Debug("Waiting for client");
            _serverStream.WaitForConnection();
            logger.Information("A new client connected to the pipe server");
            logger.Information("Establishing a Zily connection");

            while (true)
            {
                var header = ReadHeader();
                if (header.flag != HeaderFlag.Connected)
                {
                    Parse(header);
                }
                else
                {
                    ParseResponse(header);
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public override bool ParseResponse(HeaderFlag flag, int length)
        {
            if (!base.ParseResponse(flag, length))
            {
                switch (flag)
                {
                    case HeaderFlag.Connected:
                        logger.Information("Zily client successfully connected");
                        break;
                    case HeaderFlag.Disconnected:
                        logger.Error("The client has disconnected from the server");
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            base.Close();
            _serverStream.Disconnect();
            logger.Information("Server is closed");
        }
    }
}
