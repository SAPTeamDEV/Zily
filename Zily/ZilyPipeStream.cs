using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Text;
using Serilog;
using System.Threading;

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
                Ok();
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool ParseResponse(HeaderFlag flag, int length)
        {
            if (!base.ParseResponse(flag, length))
            {
                switch (flag)
                {
                    case HeaderFlag.Connected:
                        break;
                    case HeaderFlag.Disconnected:
                        break;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Listens to all incoming requests.
        /// </summary>
        public void Listen()
        {
            Listen(CancellationToken.None);
        }

        /// <summary>
        /// Listens to all incoming requests.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token for terminating the listener.
        /// </param>
        public void Listen(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var header = ReadHeader(cancellationToken);
                Parse(header);
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            _pipe.Close();
        }
    }
}
