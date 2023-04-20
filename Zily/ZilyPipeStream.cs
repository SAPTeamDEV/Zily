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

        // The optimized header feature is not working in pipe stream.
        // So we temporarily patch it...
        /// <inheritdoc/>
        public override (HeaderFlag flag, int length) ReadHeader(CancellationToken cancellationToken)
        {
            var flag = (HeaderFlag)ReadByte();
            int length = ReadByte() * 256;
            length += ReadByte();

            return (flag, length);
        }

        /// <inheritdoc/>
        public override byte[] CreateHeader(HeaderFlag flag, int length)
        {
            if (length > ushort.MaxValue)
            {
                throw new ArgumentException("Length is too long.");
            }

            return new byte[]
            {
                (byte)flag,
                (byte)(length / 256),
                (byte)(length & 255)
            };
        }

        /// <inheritdoc/>
        protected override bool ParseHelper(HeaderFlag flag, int length)
        {
            bool responseHandled = false;

            switch (flag)
            {
                default:
                    return false;
            }

            if (!responseHandled)
            {
                WriteCommand(HeaderFlag.Ok);
            }

            return true;
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

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var header = ReadHeader(cancellationToken);
                    Parse(header);
                }
                catch (Exception)
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
            WriteCommand(HeaderFlag.Disconnected);
            // _pipe.Close();
        }
    }
}
