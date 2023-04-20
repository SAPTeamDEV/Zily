using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Text;
using Serilog;
using System.Threading;
using System.Runtime.InteropServices.ComTypes;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents a <see cref="ZilyStream"/> for communicating through an In-Out named pipe.
    /// </summary>
    public class ZilyPipeStream : ZilyStream
    {
        readonly PipeStream _pipe;
        bool _online;

        /// <summary>
        /// Gets or sets a value indicating whether this connection is online
        /// </summary>
        public bool IsOnline
        {
            get
            {
                return _online && _pipe.IsConnected;
            }
            set => _online = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyPipeStream"/>.
        /// </summary>
        /// <param name="pipeStream">
        /// An instance of the <see cref="PipeStream"/> that supports In-Out directions.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyPipeStream(PipeStream pipeStream, ILogger logger = null) : base(pipeStream, logger)
        {
            _pipe = pipeStream;

            if (!_pipe.CanRead || !_pipe.CanWrite)
            {
                throw new ArgumentException("The pipe stream must support read and write operation.");
            }
        }

        /// <summary>
        /// Listens to all incoming requests.
        /// </summary>
        /// <param name="suppressLogger">
        /// Determines whether the logger should be stopped during listening.
        /// </param>
        public void Listen(bool suppressLogger = true)
        {
            Listen(CancellationToken.None, suppressLogger);
        }

        /// <summary>
        /// Listens to all incoming requests.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token for terminating the listener.
        /// </param>
        /// <param name="suppressLogger">
        /// Determines whether the logger should be stopped during listening.
        /// </param>
        public void Listen(CancellationToken cancellationToken, bool suppressLogger = true)
        {
            logger.Information("Staring listener");
            ILogger _logger = null;
            if (suppressLogger)
            {
                _logger = logger;
                logger = Serilog.Core.Logger.None;
            }

            while (!cancellationToken.IsCancellationRequested && IsOnline)
            {
                try
                {
                    var header = ReadHeader();
                    Parse(header);
                }
                catch (IOException)
                {
                    break;
                }
            }

            if (suppressLogger)
            {
                logger = _logger;
            }

            logger.Information("Listener has stopped");
        }

        /// <inheritdoc/>
        public override void Close()
        {
            logger.Information("Closing connection");
            if (IsOnline)
            {
                WriteCommand(HeaderFlag.Disconnected);
                IsOnline = false;
            }
        }
    }
}
