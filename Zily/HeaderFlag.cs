using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents header flags for sending a request or responding to a request.
    /// </summary>
    public enum HeaderFlag
    {
        /// <summary>
        /// An unknown flag.
        /// </summary>
        Unknown,

        /// <summary>
        /// Indicates a normal state
        /// </summary>
        Ok,

        /// <summary>
        /// Indicates an abnormal state. The error text must be sent with this header flag.
        /// </summary>
        Fail,

        /// <summary>
        /// Indicates a potential abnormal state.
        /// </summary>
        Warn,

        /// <summary>
        /// Indicates an established connection.
        /// </summary>
        Connected,

        /// <summary>
        /// Indicates an refused connection.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Indicates a response that represents the protocol version.
        /// </summary>
        VersionInfo,

        /// <summary>
        /// A request for writing a text to the console.
        /// </summary>
        Write = 10,

        /// <summary>
        /// A request for getting the protocol version.
        /// </summary>
        Version = 11,
    }
}
