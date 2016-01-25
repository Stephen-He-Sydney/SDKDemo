using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace RingCentral.ServiceModel
{
    public class Options
    {
        public bool addServer { get; set; }

        public bool addToken { get; set; }

        public string addMethod { get; set; }

        public Options()
        {
            addServer = true;
            addToken = false;
            addMethod = string.Empty;
        }
    }
}
