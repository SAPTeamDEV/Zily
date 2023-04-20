﻿using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;

using Serilog;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents a <see cref="ZilyPipeStream"/> to act as a pipe client.
    /// </summary>
    public class ZilyPipeClientStream : ZilyPipeStream
    {
        readonly NamedPipeClientStream _clientStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyPipeClientStream"/>.
        /// </summary>
        /// <param name="pipeClient">
        /// An instance of the <see cref="NamedPipeClientStream"/> that supports In-Out directions.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyPipeClientStream(NamedPipeClientStream pipeClient, ILogger logger = null) : base(pipeClient, logger)
        {
            _clientStream = pipeClient;
        }

        /// <summary>
        /// Establishes a Zily connection through a named pipe.
        /// </summary>
        public void Connect()
        {
            if (!_clientStream.IsConnected)
            {
                _clientStream.Connect();
            }

            Send(HeaderFlag.Version);
            if (StreamVersion.Major != API.Major)
            {
                // Will completed in the future
            }

            WriteCommand(HeaderFlag.Connected);
        }
    }
}