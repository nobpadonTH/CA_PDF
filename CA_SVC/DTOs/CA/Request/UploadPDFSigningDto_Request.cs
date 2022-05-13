using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.DTOs.CA.Request
{
    public class UploadPDFSigningDto_Request
    {
        [Required]
        public string PdfFileBase64 { get; set; }

        [Required]
        public string BillNo { get; set; }

        [Required]
        public string SaveFilePart { get; set; }
    }
}