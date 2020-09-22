using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELS_Server
{
    public class CachedData
    {
        public IDictionary<string, object> light { get; set; }

        public IDictionary<string, object> siren { get; set; }
    }
}
