using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.DTOs.CA.Request
{
    public class UploadPDFFromPartDto_Requesr
    {
        [Required]
        public string Part { get; set; }
    }
}