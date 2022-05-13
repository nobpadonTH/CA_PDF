using CA_SVC.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CA_SVC.Clients
{
    /// <summary>
    /// SiamSmile's SMS Api
    /// </summary>
    /// <remarks>
    /// Uses for send a message to the vendor's SMS Gateways.
    /// Normally URL for UAT is http://uat.siamsmile.co.th:9215
    /// </remarks>
    public class SendSmsClient
    {
        private readonly RestClient _client;
        private readonly ServiceURL _configulation;

        // Loging message & return status
        private const string _clientTitle = "Send Sms Client";

        // Endpoint
        private const string _SendSMSListEndpoint = "/api/sms/SendSMSList";

        private const string _SendSMSV2Endpoint = "/api/sms/SendSMSV2";

        public SendSmsClient(IOptions<ServiceURL> configuration)
        {
            // Get configuration
            _configulation = configuration.Value;

            // Set client's configuration
            _client = new RestClient(_configulation.SendSmsApi);
            _client.UseNewtonsoftJson();

            // Check HttpRequest and set Client's Header Token
            _client.AddDefaultHeader("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcm9qZWN0aWQiOjl9.XX7R_Ik8pydv2e04ZvrDew8tlszSDrjvTEYKdgO4t7A");
        }

        /// <summary>
        /// Send multiple SMS message
        /// </summary>
        /// <param name="infomation">A required parameter for send multiple SMS</param>
        /// <returns>Response of sent SMS</returns>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task<SendSmsListResponse> SendSMSListAsycn(SendSmsListRequest infomation)
        {
            var listOfPhoneNo = infomation.Data.Select(_ => _.PhoneNo).ToArray();
            Log.Debug("{ApiClient} Attempt to send SMS [PhoneNo={PhoneNo}]", _clientTitle, String.Join(",", listOfPhoneNo));

            return SendSMSListAsycn(infomation, System.Threading.CancellationToken.None);
        }

        private async Task<SendSmsListResponse> SendSMSListAsycn(SendSmsListRequest infomation, CancellationToken none)
        {
            // Validation Information
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(infomation);
            Validator.ValidateObject(infomation, validationContext);

            var listOfPhoneNo = string.Join(",", infomation.Data.Select(_ => _.PhoneNo).ToArray());

            SendSmsListResponse responseObject;

            // If Configulation is in SMS disable mode
            if (!_configulation.SendSmsApiEnable)
            {
                Log.Information("{ApiClient} Configulation is in SMS disable mode", _clientTitle);

                // Mockup data SMS Response
                var mockUpString = "{\"status\": \"000\",\"detail\": \"OK\",\"referenceHeaderID\": \"150605222\"}";
                responseObject = JsonConvert.DeserializeObject<SendSmsListResponse>(mockUpString);
            }
            else
            {
                // Create request
                var request = new RestRequest(_SendSMSListEndpoint, DataFormat.Json);
                request.AddJsonBody(infomation);

                // Attemp request to Send SMS List
                var response = await Task.FromResult(_client.Post<SendSmsListResponse>(request));
                Log.Verbose("{ApiClient} Requested to API", _clientTitle);

                // Check data is valid
                if (response.Data is null)
                {
                    Log.Error(response.ErrorException, "{ApiClient} Got an error [Message={ErrorMessage}]", _clientTitle, response.ErrorMessage);
                    throw new InvalidOperationException(response.ErrorMessage, response.ErrorException);
                }

                Log.Debug("{ApiClient} [Response={Response}]", _clientTitle, JsonConvert.SerializeObject(response.Data));
                responseObject = response.Data;
            }

            // Return response
            Log.Information("{ApiClient} SMS have been sent [PhoneNo={PhoneNo},Status={Status}, Detail={Detail}, Transaction={Transaction}]", _clientTitle, listOfPhoneNo, responseObject.Status, responseObject.Detail, responseObject.ReferenceHeaderID);
            return responseObject;
        }

        /// <summary>
        /// Send a single SMS message
        /// </summary>
        /// <param name="infomation">Required parameter for send SMS</param>
        /// <returns>Response of sent SMS</returns>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task<SendSmsV2Response> SendSMSV2Asycn(SendSmsV2Request infomation)
        {
            Log.Debug("{ApiClient} Attempt to send SMS [PhoneNo={PhoneNo}]", _clientTitle, infomation.PhoneNo);
            return SendSMSV2Asycn(infomation, System.Threading.CancellationToken.None);
        }

        private async Task<SendSmsV2Response> SendSMSV2Asycn(SendSmsV2Request infomation, CancellationToken none)
        {
            // Validation Information
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(infomation);
            Validator.ValidateObject(infomation, validationContext);

            SendSmsV2Response responseObject;

            // If Configulation is in SMS disable mode
            if (!_configulation.SendSmsApiEnable)
            {
                Log.Information("{ApiClient} Configulation is in SMS disable mode", _clientTitle);

                // Mockup data SMS Response
                var mockUpString = "{\"status\": \"000\",\"detail\": \"OK\",\"language\": \"ENG\",\"usedCredit\": \"1\",\"sumPhone\": \"1\",\"transaction\": \"150605222\"}";
                responseObject = JsonConvert.DeserializeObject<SendSmsV2Response>(mockUpString);
            }
            else
            {
                // Create request
                var request = new RestRequest(_SendSMSV2Endpoint, DataFormat.Json);
                request.AddJsonBody(infomation);

                // Attemp request to Send SMS
                var response = await Task.FromResult(_client.Post<SendSmsV2Response>(request));
                Log.Verbose("{ApiClient} Requested to API", _clientTitle);

                // Check data is valid
                if (response.Data is null)
                {
                    Log.Error(response.ErrorException, "{ApiClient} Got an error [Message={ErrorMessage}]", _clientTitle, response.ErrorMessage);
                    throw response.ErrorException;
                }

                Log.Debug("{ApiClient} [Response={Response}]", _clientTitle, JsonConvert.SerializeObject(response.Data));
                responseObject = response.Data;
            }

            // Return response
            Log.Information("{ApiClient} SMS have been sent [PhoneNo={PhoneNo},Status={Status}, Detail={Detail}, Transaction={Transaction}]", _clientTitle, infomation.PhoneNo, responseObject.Status, responseObject.Detail, responseObject.Transaction);
            return responseObject;
        }
    }

    /// <summary>
    /// A required parameter for send multiple SMS
    /// </summary>

    public class SendSmsListRequest
    {
#nullable enable

        /// <summary>
        /// รหัสโปรเจคที่เรียกใช้ เช่น 1=n/a,3 = Duration
        /// </summary>
        //[JsonProperty("ProjectId")]
        public int? ProjectId { get; set; }

        /// <summary>
        /// รหัสประเภทของข้อความ
        /// </summary>
        //[JsonProperty("SMSTypeId")]
        public int? SMSTypeId { get; set; }

        /// <summary>
        /// หมายเหตุ
        /// </summary>
        //[JsonProperty("Remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// จำนวนเบอร์โทรทั้งหมด
        /// </summary>
        //[JsonProperty("Total")]
        public int? Total { get; set; }

        /// <summary>
        /// วันเวลาที่จะส่งข้อความ (ถ้าไม่กำหนดค่าหรือส่งค่าว่างมา ระบบจะทำการส่งข้อความให้ทันที)
        /// **SendDate ยังไม่เปิดใช้งาน
        /// </summary>
        //[JsonProperty("SendDate")]
        public string? SendDate { get; set; }

        /// <summary>
        /// ประกอบด้วย PhoneNo = เบอร์โทร,Message=ข้อความ
        /// </summary>
        //[JsonProperty("Data")]
        public SendSmsV2Body[]? Data { get; set; }

#nullable disable
    }

    public class SendSmsListResponse
    {
        //[JsonProperty("status")]
        public string Status { get; set; }

        //[JsonProperty("detail")]
        public string Detail { get; set; }

        //[JsonProperty("referenceHeaderID")]
        public string ReferenceHeaderID { get; set; }
    }

    public class SendSmsV2Body
    {
        //[JsonProperty("PhoneNo")]
        public string PhoneNo { get; set; }

        //[JsonProperty("Message")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Required parameter for send SMS
    /// </summary>
    public class SendSmsV2Request : IValidatableObject
    {
#nullable enable

        /// <summary>
        /// รหัสประเภทของข้อความ
        /// </summary>
        //[JsonProperty("SMSTypeId")]
        public int? SMSTypeId { get; set; }

        /// <summary>
        /// ข้อความ
        /// </summary>
        //[JsonProperty("Message")]
        public string? Message { get; set; }

        /// <summary>
        /// เบอร์โทร
        /// </summary>
        //[JsonProperty("PhoneNo")]
        public string? PhoneNo { get; set; }

        /// <summary>
        /// รหัสผู้ส่ง
        /// </summary>
        //[JsonProperty("CreatedById")]
        public int? CreatedById { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(this.Message))
            {
                yield return new ValidationResult("SMS's Message can't be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(this.PhoneNo))
            {
                yield return new ValidationResult("SMS's Phone No can't be null or empty.");
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(this.PhoneNo, @"(^[0][0-9]{9})$") == false)
            {
                yield return new ValidationResult("SMS's Phone No is invalid. Phone No must start with 0 and contain 10 digits of number.");
            }
        }

#nullable disable
    }

    public class SendSmsV2Response
    {
        //[JsonProperty("status")]
        public string Status { get; set; }

        //[JsonProperty("detail")]
        public string Detail { get; set; }

        //[JsonProperty("language")]
        public string Language { get; set; }

        //[JsonProperty("usedCredit")]
        public string UsedCredit { get; set; }

        //[JsonProperty("sumPhone")]
        public string SumPhone { get; set; }

        //[JsonProperty("transaction")]
        public string Transaction { get; set; }
    }
}