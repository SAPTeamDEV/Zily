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
        [ResponseFlag(true)]
        Ok,

        /// <summary>
        /// Indicates an abnormal state. The error text must be sent with this header flag.
        /// </summary>
        [ResponseFlag(false)]
        Fail,

        /// <summary>
        /// Indicates a potential abnormal state.
        /// </summary>
        [ResponseFlag(false)]
        Warn,

        /// <summary>
        /// Indicates an established connection.
        /// </summary>
        [ResponseFlag(true)]
        Connected,

        /// <summary>
        /// Indicates an refused connection.
        /// </summary>
        [ResponseFlag(true)]
        Disconnected,

        /// <summary>
        /// Indicates a response that represents the protocol version.
        /// </summary>
        [ResponseFlag(false)]
        VersionInfo,

        /// <summary>
        /// A request for writing a text to the console.
        /// </summary>
        [RequestFlag(false)]
        Write,

        /// <summary>
        /// A request for getting the protocol version.
        /// </summary>
        [RequestFlag(true)]
        Version,
    }
}
