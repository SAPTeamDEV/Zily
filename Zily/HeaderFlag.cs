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
        /// It requests the server to write a text to console.
        /// </summary>
        Write = 10,
    }
}
