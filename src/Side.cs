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
        public List<string> Identifiers { get; }

        /// <inheritdoc/>
        public string Name { get; }
    }
}
