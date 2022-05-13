using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Configurations
{
    public class OAuthSetting
    {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public Dictionary<string, string> Scopes { get; set; }
    }
}