using System;
using System.Collections.Generic;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Represents a standard for identify and establish connection between similar sides..
    /// </summary>
    public interface ISide
    {
        /// <summary>
        /// Gets the protocol implemented by the <see cref="ISide"/>.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Gets the version of the <see cref="ISide"/>.
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// Gets the identifiers of the <see cref="ISide"/>.
        /// </summary>
        string[] Identifiers { get; }

        /// <summary>
        /// Gets the name of the <see cref="ISide"/>.
        /// </summary>
        string Name { get; }
    }
}