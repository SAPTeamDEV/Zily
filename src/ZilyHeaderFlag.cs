using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents header flags for identifying zily packets.
    /// </summary>
    public class ZilyHeaderFlag
    {
        /// <summary>
        /// An unknown state.
        /// </summary>
        public const int Unknown = 0;

        /// <summary>
        /// Indicates a normal state
        /// </summary>
        public const int Ok = 1;

        /// <summary>
        /// Indicates an abnormal state. The error text must be sent with this header flag.
        /// </summary>
        public const int Fail = 2;

        /// <summary>
        /// Indicates a potential abnormal state.
        /// </summary>
        public const int Warn = 3;

        /// <summary>
        /// Indicates an established connection.
        /// </summary>
        public const int Connected = 4;

        /// <summary>
        /// Indicates an refused connection.
        /// </summary>
        public const int Disconnected = 5;

        /// <summary>
        /// Contains a packet with console message
        /// </summary>
        public const int Write = 6;
    }
}
