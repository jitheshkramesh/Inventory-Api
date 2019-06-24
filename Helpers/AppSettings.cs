using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DempApiApp.Helpers
{
    public class AppSettings
    {
        //JWT token signature properties
        public string Site { get; set; }
        public string Audience { get; set; }
        public string ExpireTime { get; set; }
        public string Secret { get; set; }
    }
}
