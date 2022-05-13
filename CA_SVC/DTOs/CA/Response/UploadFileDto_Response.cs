using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.DTOs.CA.Response
{
    public class UploadFileDto_Response
    {
        public bool? IsResult { get; set; }
        public string Data { get; set; }
        public string Msg { get; set; }
    }
}