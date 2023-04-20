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
        /// Indicates a normal state
        /// </summary>
        Ok = 1,

        /// <summary>
        /// Indicates an abnormal state. The error text must be sent with this header flag.
        /// </summary>
        Fail = 2,

        /// <summary>
        /// Indicates a potential abnormal state.
        /// </summary>
        Warn = 3,

        /// <summary>
        /// Indicates an established connection.
        /// </summary>
        Connected = 4,

        /// <summary>
        /// Indicates an refused connection.
        /// </summary>
        Disconnected = 5,

        /// <summary>
        /// Indicates a response that represents the protocol version.
        /// </summary>
        VersionInfo = 6,

        /// <summary>
        /// A request for writing a text to the console.
        /// </summary>
        Write = 10,

        /// <summary>
        /// A request for getting the protocol version.
        /// </summary>
        Version = 11,

        /// <summary>
        /// An unsupported flag.
        /// </summary>
        Unsupported = 99,
    }
}
