using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify connection sides.
    /// </summary>
    /// <seealso cref="ISide" />
    public class Side : ISide
    {
        /// <inheritdoc/>
        public string Protocol { get; }

        /// <inheritdoc/>
        public Version Version { get; }

        /// <inheritdoc/>
        public string[] Identifiers { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Side"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this side.
        /// </param>
        public Side(string protocol)
        {
            Protocol = protocol;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Side"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this side.
        /// </param>
        /// <param name="name">
        /// The name of the side.
        /// </param>
        public Side(string protocol, string name)
        {
            Protocol = protocol;
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Side"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this side.
        /// </param>
        /// <param name="name">
        /// The name of the side.
        /// </param>
        /// <param name="identifiers">
        /// The identifiers of the side.
        /// </param>
        public Side(string protocol, string name, string[] identifiers)
        {
            Protocol = protocol;
            Name = name;
            Identifiers = identifiers;
        }
    }
}
