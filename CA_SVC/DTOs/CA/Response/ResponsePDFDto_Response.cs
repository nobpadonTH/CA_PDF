using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.DTOs.CA.Response
{
    public class ResponsePDFDto_Response
    {
        public int? responseCode { get; set; }
        public string responseMessage { get; set; }
        public string pdfData { get; set; }
    }
}