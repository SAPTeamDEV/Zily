using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Text;
using Serilog;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents a <see cref="ZilyStream"/> for communicating through an In-Out named pipe.
    /// </summary>
    public class ZilyPipeStream : ZilyStream
    {
        /// <summary>
        /// Gets the underlying <see cref="PipeStream"/>.
        /// </summary>
        public PipeStream Pipe { get; }

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
            Pipe = pipeStream;

            if (!Pipe.CanRead || !Pipe.CanWrite)
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
        /// Sends a command to the stream, then waits for receiving response and parses it.
        /// </summary>
        /// <param name="flag">
        /// The header flag. Header flags are stored in the <see cref="HeaderFlag"/>.
        /// </param>
        /// <param name="text">
        /// The text (argument) for the requested action or response to a request.
        /// </param>
        public void Send(HeaderFlag flag, string text = null)
        {
            WriteCommand(flag, text);
            Pipe.WaitForPipeDrain();
            Parse();
        }
    }
}
