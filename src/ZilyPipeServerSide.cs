using System;
using System.IO;
using System.IO.Pipes;

using Serilog;
using Serilog.Core;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyPipeServerSide : ZilyServerSide
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyPipeServerSide"/>.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="System.IO.Stream"/> with read and write permission.
        /// </param>
        /// <param name="logger">
        /// The application's logger. by default it uses the <see cref="Log.Logger"/>.
        /// </param>
        public ZilyPipeServerSide(Stream stream, ILogger logger = null) : base(stream, logger) { }

        /// <inheritdoc/>
        public override void Wait()
        {
            ((NamedPipeServerStream)Stream).WaitForConnection();
        }
    }
}