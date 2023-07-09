using System;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilySide : Side
    {
        int last_request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilySide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        public ZilySide(string protocol) : base(protocol, "Zily Side", new string[]{"zily"})
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilySide"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this zily side.
        /// </param>
        /// <param name="name">
        /// The name of this zily instance.
        /// </param>
        public ZilySide(string protocol, string name) : base(protocol, name, new string[]{"zily", name})
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilySide"/>.
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
        public ZilySide(string protocol, string name, string[] identifiers) : base(protocol, name, identifiers)
        {
            
        }

        /// <summary>
        /// Parses the given <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="header">
        /// The parsed header object.
        /// </param>
        public virtual void ParseHeader(ZilyHeader header)
        {
            
        }
    }
}