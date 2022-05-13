using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CA_SVC.Configurations
{
    public class ServiceURL
    {
        public string ShortLinkApi { get; set; }
        public string SendSmsApi { get; set; }
        public bool SendSmsApiEnable { get; set; } = false;
    }
}