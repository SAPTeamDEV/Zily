using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    internal class FlagAttribute : Attribute
    {
        public bool IsParameterless { get; set; }

        public FlagAttribute(bool isParameteless)
        {
            IsParameterless = isParameteless;
        }
    }
}
