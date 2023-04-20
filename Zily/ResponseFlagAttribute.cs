using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    internal class ResponseFlagAttribute : FlagAttribute
    {
        public ResponseFlagAttribute(bool isParameteless) : base(isParameteless)
        {
        }
    }
}
