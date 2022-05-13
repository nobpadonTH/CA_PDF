using CA_SVC.DTOs.CA.Request;
using CA_SVC.DTOs.CA.Response;
using CA_SVC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Services.CA
{
    public interface ICAServices
    {
        Task<ServiceResponse<UploadFileDto_Response>> UploadFile(UploadPDFSigningDto_Request data);

        Task<ServiceResponse<UploadFileDto_Response>> UploadFile(UploadPDFSigningSignatureDto_Request data);

        Task<ServiceResponse<UploadFileDto_Response>> UploadFileNoSignature(UploadPDFSigningDto_Request data);

        Task<ServiceResponse<UploadFileArrayDto_Response>> GetFilesInDirectory(UploadPDFFromPartDto_Requesr part);
    }
}