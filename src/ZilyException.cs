using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents errors that occur in a Zily session.
    /// </summary>
    internal class ZilyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyException"/> class.
        /// </summary>
        public ZilyException()
            : base("There is a problem in Zily session.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ZilyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ZilyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZilyException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="context">The context.</param>
        public ZilyException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }
    }
}
