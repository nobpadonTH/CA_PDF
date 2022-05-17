using CA_SVC.Configurations;
using CA_SVC.DTOs.CA.Request;
using CA_SVC.DTOs.CA.Response;
using CA_SVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Services.CA
{
    public class CAServices : ICAServices
    {
        private const string TEXTSUCCESS = "Success";

        // Endpoint
        private const string _ca = "https://uat-sign.one.th/webservice/api/v2/signing/pdfSigning-V3";

        private readonly RestClient _client;
        private readonly ServiceURL _configulation;

        public CAServices(IOptions<ServiceURL> configuration)
        {
            // Get configuration
            _configulation = configuration.Value;

            // Set client's configuration
            _client = new RestClient(_configulation.SendSmsApi);
            _client.UseNewtonsoftJson();

            // Check HttpRequest and set Client's Header certificate
            var certFile = Path.Combine("D:\\Work\\CA\\smileTPA", "smileTPA.p12");
            var pass = "P@ssw0rd";
            X509Certificate2 certificate = new X509Certificate2(certFile, pass);
            _client.ClientCertificates = new X509CertificateCollection() { certificate };
        }

        public async Task<ServiceResponse<UploadFileArrayDto_Response>> GetFilesInDirectory(UploadPDFFromPartDto_Requesr part)
        {
            try
            {
                // If directory does not exist, don't even try
                if (!Directory.Exists(part.Part))
                    return ResponseResult.Failure<UploadFileArrayDto_Response>("Directory not Exists");

                string pdf = string.Empty;
                DirectoryInfo di = new DirectoryInfo(part.Part);
                FileInfo[] files = di.GetFiles("*.pdf");
                string[] arrPDF64 = { };
                List<string> pdf64 = new List<string>();
                MemoryStream ms = new MemoryStream();
                for (int i = 0; i < files.Length; i++)
                {
                    using (FileStream file = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read))
                        file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    pdf64.Add(Convert.ToBase64String(fileBytes));
                    arrPDF64 = pdf64.ToArray();
                }

                List<string> ListFilesPart = new List<string>();

                foreach (var item in pdf64)
                {
                    var obj = new
                    {
                        pdfData = item,
                        cadData = "sy1ORJpPOgb/vnoPFmONy1UJaQ//ge4pYxXPvscdQ7Lc3tcQV0yGXucxPjmGbJ4yylTfB1+PE56t62N26PKU0PV0NQOayAXjkhCjOXn0DihAQxECDLzYT15XkhlnjYnHtyEFnM4L/YrerOrqqiyq9hVA+Ptys7oVTdfIozAhXYY=",
                        certifyLevel = "CERTIFY",
                        overwriteOriginal = false
                    };

                    // Create request
                    var request = new RestRequest(_ca, DataFormat.Json);
                    request.AddJsonBody(obj);

                    // Attemp request
                    var response = await Task.FromResult(_client.Post<ResponsePDFDto_Response>(request));

                    // Check data is valid
                    if (response.Data is null)
                    {
                        throw new InvalidOperationException(response.StatusDescription, response.ErrorException);
                    }

                    var strPart = "D:\\Work\\CA\\smileTPA";
                    var fileName = $"file_{DateTime.Now.ToString("yyyyMMddHHmm")}.pdf";

                    // Try to create the directory.
                    if (!Directory.Exists(strPart))
                    {
                        Directory.CreateDirectory(strPart);
                    }

                    //create file
                    using (FileStream stream = File.Create(part + "\\" + fileName))
                    {
                        byte[] byteArray = Convert.FromBase64String(response.Data.pdfData);
                        stream.Write(byteArray, 0, byteArray.Length);
                    }
                    string fullPath = part + "\\" + fileName;
                    ListFilesPart.Add(fullPath);
                }

                var dto = new UploadFileArrayDto_Response
                {
                    IsResult = true,
                    Data = ListFilesPart.ToArray(),
                    Msg = TEXTSUCCESS
                };
                return ResponseResult.Success(dto);
            }
            catch (Exception ex)
            {
                return ResponseResult.Failure<UploadFileArrayDto_Response>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UploadFileDto_Response>> UploadFile(UploadPDFSigningDto_Request data)
        {
            try
            {
                //var fileExtensionPdf = Path.GetExtension(data.PdfFile.FileName);

                //if (fileExtensionPdf != ".pdf")
                //    return ResponseResult.Failure<UploadFileDto_Response>("Wrong File Type (access file .PDF)");

                var part = $"{data.SaveFilePart}\\{data.BillNo}";
                var fileName = $"{data.BillNo}_tmp.pdf";
                var fileNameCA = $"{data.BillNo}.pdf";

                // Try to create the directory.
                if (!Directory.Exists(part))
                {
                    Directory.CreateDirectory(part);
                }

                using (FileStream stream = File.Create(part + "\\" + fileName))
                {
                    byte[] byteArray = Convert.FromBase64String(data.PdfFileBase64);
                    stream.Write(byteArray, 0, byteArray.Length);
                }

                //เช็ค bill นี้เคยยิ่ง CA หรือยัง
                if (!File.Exists($"{data.SaveFilePart}\\{data.BillNo}\\{fileNameCA}"))
                {
                    string pdf = data.PdfFileBase64;
                    //using (var ms = new MemoryStream())
                    //{
                    //    data.PdfFile.CopyTo(ms);
                    //    var fileBytes = ms.ToArray();
                    //    pdf = Convert.ToBase64String(fileBytes);
                    //}

                    var obj = new
                    {
                        pdfData = pdf,
                        cadData = "sy1ORJpPOgb/vnoPFmONy1UJaQ//ge4pYxXPvscdQ7Lc3tcQV0yGXucxPjmGbJ4yylTfB1+PE56t62N26PKU0PV0NQOayAXjkhCjOXn0DihAQxECDLzYT15XkhlnjYnHtyEFnM4L/YrerOrqqiyq9hVA+Ptys7oVTdfIozAhXYY=",
                        certifyLevel = "CERTIFY",
                        overwriteOriginal = false,
                    };

                    // Create request
                    var request = new RestRequest(_ca, DataFormat.Json);
                    request.AddJsonBody(obj);

                    // Attemp request
                    var response = await Task.FromResult(_client.Post<ResponsePDFDto_Response>(request));

                    // Check data is valid
                    if (response.Data is null)
                    {
                        throw new InvalidOperationException(response.StatusDescription, response.ErrorException);
                    }

                    //create file
                    using (FileStream stream = File.Create(part + "\\" + fileNameCA))
                    {
                        byte[] byteArray = Convert.FromBase64String(response.Data.pdfData);
                        stream.Write(byteArray, 0, byteArray.Length);
                    }

                    string fullPath = $"{part}\\{fileNameCA}";
                    var dto = new UploadFileDto_Response
                    {
                        IsResult = true,
                        Data = fullPath,
                        Msg = TEXTSUCCESS
                    };

                    return ResponseResult.Success(dto);
                }
                else
                {
                    string fullPath = $"{data.SaveFilePart}\\{data.BillNo}\\{fileNameCA}";
                    var dto = new UploadFileDto_Response
                    {
                        IsResult = true,
                        Data = fullPath,
                        Msg = TEXTSUCCESS
                    };
                    return ResponseResult.Success(dto);
                }
            }
            catch (Exception ex)
            {
                return ResponseResult.Failure<UploadFileDto_Response>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UploadFileDto_Response>> UploadFile(UploadPDFSigningSignatureDto_Request data)
        {
            try
            {
                //if (fileExtensionPdf != ".pdf")
                //    return ResponseResult.Failure<UploadFileDto_Response>("Wrong File Type (access file .PDF)");

                //if (fileExtensionImg != ".jpeg" && fileExtensionImg != ".jpg")
                //    return ResponseResult.Failure<UploadFileDto_Response>("Wrong File Type (access file .jpeg or .jpg)");

                var part = $"{data.SaveFilePart}\\{data.BillNo}";
                var fileName = $"{data.BillNo}_tmp.pdf";
                var fileNameCA = $"{data.BillNo}.pdf";

                // Try to create the directory.
                if (!Directory.Exists(part))
                {
                    Directory.CreateDirectory(part);
                }

                using (FileStream stream = File.Create(part + "\\" + fileName))
                {
                    byte[] byteArray = Convert.FromBase64String(data.PdfFileBase64);
                    stream.Write(byteArray, 0, byteArray.Length);
                }

                string pdf = data.PdfFileBase64;
                //using (var ms = new MemoryStream())
                //{
                //    data.PdfFile.CopyTo(ms);
                //    var fileBytes = ms.ToArray();
                //    pdf = Convert.ToBase64String(fileBytes);
                //}

                string img = data.ImgFileBase64;
                //using (var ms = new MemoryStream())
                //{
                //    data.ImgFile.CopyTo(ms);
                //    var fileBytes = ms.ToArray();
                //    img = Convert.ToBase64String(fileBytes);
                //}

                var obj = new
                {
                    pdfData = pdf,
                    sadData = "",
                    cadData = "sy1ORJpPOgb/vnoPFmONy1UJaQ//ge4pYxXPvscdQ7Lc3tcQV0yGXucxPjmGbJ4yylTfB1+PE56t62N26PKU0PV0NQOayAXjkhCjOXn0DihAQxECDLzYT15XkhlnjYnHtyEFnM4L/YrerOrqqiyq9hVA+Ptys7oVTdfIozAhXYY=",
                    reason = "ทดสอบการลงนาม",
                    location = "TH",
                    certifyLevel = "NON-CERTIFY",
                    hashAlgorithm = "sha-256",
                    overwriteOriginal = true,
                    visibleSignature = "Graphics",
                    visibleSignaturePage = 1,
                    visibleSignatureRectangle = "0.7, 0.850, 0.2, 0.1",
                    visibleSignatureImagePath = img
                };

                // Create request
                var request = new RestRequest(_ca, DataFormat.Json);
                request.AddJsonBody(obj);

                // Attemp request
                var response = await Task.FromResult(_client.Post<ResponsePDFDto_Response>(request));

                // Check data is valid
                if (response.Data is null)
                {
                    throw new InvalidOperationException(response.StatusDescription, response.ErrorException);
                }

                //var part = "D:\\Work\\CA\\smileTPA";
                //var fileName = $"file_{DateTime.Now.ToString("yyyyMMddHHmm")}.pdf";

                // Try to create the directory.
                if (!Directory.Exists(part))
                {
                    Directory.CreateDirectory(part);
                }

                //create file

                using (FileStream stream = File.Create(part + "\\" + fileNameCA))
                {
                    byte[] byteArray = Convert.FromBase64String(response.Data.pdfData);
                    stream.Write(byteArray, 0, byteArray.Length);
                }
                string fullPath = $"{part}\\{fileNameCA}";
                var dto = new UploadFileDto_Response
                {
                    IsResult = true,
                    Data = fullPath,
                    Msg = TEXTSUCCESS
                };

                return ResponseResult.Success(dto);
            }
            catch (Exception ex)
            {
                return ResponseResult.Failure<UploadFileDto_Response>(ex.Message);
            }
        }

        public async Task<ServiceResponse<UploadFileDto_Response>> UploadFileNoSignature(UploadPDFSigningDto_Request data)
        {
            try
            {
                //var fileExtensionPdf = Path.GetExtension(data.PdfFile.FileName);

                //if (fileExtensionPdf != ".pdf")
                //    return ResponseResult.Failure<UploadFileDto_Response>("Wrong File Type (access file .PDF)");

                var part = $"{data.SaveFilePart}\\{data.BillNo}";
                var fileName = $"{data.BillNo}_tmp.pdf";
                var fileNameCA = $"{data.BillNo}.pdf";

                // Try to create the directory.
                if (!Directory.Exists(part))
                {
                    Directory.CreateDirectory(part);
                }

                string pdf = data.PdfFileBase64;
                using (FileStream stream = File.Create(part + "\\" + fileName))
                {
                    byte[] byteArray = Convert.FromBase64String(data.PdfFileBase64);
                    stream.Write(byteArray, 0, byteArray.Length);
                }

                var obj = new
                {
                    pdfData = pdf,
                    cadData = "sy1ORJpPOgb/vnoPFmONy1UJaQ//ge4pYxXPvscdQ7Lc3tcQV0yGXucxPjmGbJ4yylTfB1+PE56t62N26PKU0PV0NQOayAXjkhCjOXn0DihAQxECDLzYT15XkhlnjYnHtyEFnM4L/YrerOrqqiyq9hVA+Ptys7oVTdfIozAhXYY=",
                    certifyLevel = "NON-CERTIFY",
                    visibleSignature = "Invisible",
                    overwriteOriginal = true
                };

                // Create request
                var request = new RestRequest(_ca, DataFormat.Json);
                request.AddJsonBody(obj);

                // Attemp request
                var response = await Task.FromResult(_client.Post<ResponsePDFDto_Response>(request));

                // Check data is valid
                if (response.Data is null)
                {
                    throw new InvalidOperationException(response.StatusDescription, response.ErrorException);
                }

                //create file
                using (FileStream stream = File.Create(part + "\\" + fileNameCA))
                {
                    byte[] byteArray = Convert.FromBase64String(response.Data.pdfData);
                    stream.Write(byteArray, 0, byteArray.Length);
                }
                string fullPath = part + "\\" + fileNameCA;
                var dto = new UploadFileDto_Response
                {
                    IsResult = true,
                    Data = fullPath,
                    Msg = TEXTSUCCESS
                };

                return ResponseResult.Success(dto);
            }
            catch (Exception ex)
            {
                return ResponseResult.Failure<UploadFileDto_Response>(ex.Message);
            }
        }
    }
}