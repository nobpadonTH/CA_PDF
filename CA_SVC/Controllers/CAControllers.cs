using CA_SVC.DTOs.CA.Request;
using CA_SVC.Services.CA;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Controllers
{
    public class CAControllers : ControllerBase
    {
        private readonly ICAServices _cAServices;

        public CAControllers(ICAServices cAServices)
        {
            _cAServices = cAServices;
        }

        /// <summary>
        ///   PDF Signing
        /// </summary>
        /// <returns>
        ///     List of Personal by JSON format
        /// </returns>
        /// <response code="200"> Success </response>
        /// <response code="400"> Bad Request </response>
        /// <response code="401"> Unauthorize </response>
        /// <response code="403"> Forbidden </response>
        /// <response code="404"> Not Found </response>
        /// <response code="500"> Internal Server Error </response>
        [HttpPost("uploadpdfsigning")]
        public async Task<IActionResult> UploadFile([FromForm] UploadPDFSigningDto_Request upload) => base.Ok(await _cAServices.UploadFile(upload));

        /// <summary>
        ///   PDF Signing
        ///
        /// แบบแสดงภาพรูปลายเซ็น
        /// </summary>
        /// <returns>
        ///     List of Personal by JSON format
        /// </returns>
        /// <response code="200"> Success </response>
        /// <response code="400"> Bad Request </response>
        /// <response code="401"> Unauthorize </response>
        /// <response code="403"> Forbidden </response>
        /// <response code="404"> Not Found </response>
        /// <response code="500"> Internal Server Error </response>
        [HttpPost("uploadpdfsigningsignature")]
        public async Task<IActionResult> UploadFile([FromForm] UploadPDFSigningSignatureDto_Request upload) => base.Ok(await _cAServices.UploadFile(upload));

        /// <summary>
        ///   PDF Signing
        ///
        /// แบบไม่แสดงภาพรูปลายเซ็น
        /// </summary>
        /// <returns>
        ///     List of Personal by JSON format
        /// </returns>
        /// <response code="200"> Success </response>
        /// <response code="400"> Bad Request </response>
        /// <response code="401"> Unauthorize </response>
        /// <response code="403"> Forbidden </response>
        /// <response code="404"> Not Found </response>
        /// <response code="500"> Internal Server Error </response>
        [HttpPost("uploadpdfsigningnosignature")]
        public async Task<IActionResult> UploadFileNoSignature([FromForm] UploadPDFSigningDto_Request upload) => base.Ok(await _cAServices.UploadFileNoSignature(upload));

        [HttpGet("uploadfile/part")]
        public async Task<IActionResult> GetFilesInDirectory([FromBody] UploadPDFFromPartDto_Requesr part) => base.Ok(await _cAServices.GetFilesInDirectory(part));
    }
}