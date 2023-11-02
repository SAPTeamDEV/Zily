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
        public virtual string Protocol { get; }

        /// <inheritdoc/>
        public virtual Version Version { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Side"/>.
        /// </summary>
        public Side()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Side"/>.
        /// </summary>
        /// <param name="protocol">
        /// The name of protocol implemented by this side.
        /// </param>
        /// <param name="version">
        /// The version of the side.
        /// </param>
        /// <param name="name">
        /// The name of the side.
        /// </param>
        public Side(string protocol, Version version, string name)
        {
            Protocol = protocol;
            Version = version;
            Name = name;
        }

        /// <summary>
        /// Gets the string identifier of this Side.
        /// </summary>
        /// <returns></returns>
        public string GetIdentifier()
        {
            return $"{Protocol};{Version.ToString()};{Name}";
        }

        /// <summary>
        /// Parses an string identifier to an instance of <see cref="Side"/>.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static Side Parse(string identifier)
        {
            string[] data = identifier.Split(';');

            return new Side(data[0], Version.Parse(data[1]), data[2]);
        }
    }
}
