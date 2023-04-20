using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    internal class RequestFlagAttribute : FlagAttribute
    {
        public RequestFlagAttribute(bool isParameteless) : base(isParameteless)
        {
        }
    }
}
