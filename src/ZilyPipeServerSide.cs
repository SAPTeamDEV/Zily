using System;
using System.IO;
using System.IO.Pipes;

using Serilog.Core;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyPipeServerSide : ZilyServerSide
    {
        /// <inheritdoc/>
        public override void Wait()
        {
            ((NamedPipeServerStream)Parent.Stream).WaitForConnection();
        }
    }
}