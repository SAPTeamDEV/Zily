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
        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyPipeServerSide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        public ZilyPipeServerSide(string protocol) : base(protocol, "Zily Side", new string[]{"zily"})
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyPipeServerSide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        /// <param name="name">
        /// The name of this zily instance.
        /// </param>
        public ZilyPipeServerSide(string protocol, string name) : base(protocol, name, new string[]{"zily", name})
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyPipeServerSide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        /// <param name="name">
        /// The name of this zily instance.
        /// </param>
        /// <param name="identifiers">
        /// The identifiers of this zily instance.
        /// </param>
        public ZilyPipeServerSide(string protocol, string name, string[] identifiers) : base(protocol, name, identifiers)
        {
            
        }

        public override void Wait()
        {
            ((NamedPipeServerStream)zs.Stream).WaitForConnection();
        }
    }
}