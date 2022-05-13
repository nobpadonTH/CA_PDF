using CA_SVC.DTOs.Auth;
using MassTransit;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Services.Auth
{
    public class LoginDetailServices : ILoginDetailServices
    {
        public LoginDetailServices(IHttpContextAccessor accessor)
        {
            loginClaim = new LoginDetailDto();

            if (accessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var token)) CheckToken(token.ToString()[7..]);
            else throw new ArgumentException($"'{nameof(accessor)}' cannot be null or whitespace.", nameof(accessor));
        }

        public LoginDetailServices(Headers header)
        {
            loginClaim = new LoginDetailDto();

            if (header.TryGetHeader("Bearer", out var token)) CheckToken(token.ToString());
            else throw new ArgumentException($"'{nameof(header)}' cannot be null or whitespace.", nameof(header));
        }

        public LoginDetailServices(string token)
        {
            loginClaim = new LoginDetailDto();

            CheckToken(token);
        }

        public string Token { get; private set; }

        private JwtSecurityToken JwtToken { get; set; }
        private LoginDetailDto loginClaim { get; set; }

        private void CheckToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException($"'{nameof(token)}' cannot be null or whitespace.", nameof(token));

            JwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            if (!string.IsNullOrWhiteSpace(JwtToken.Subject)) loginClaim.SubjectId = JwtToken.Subject;
            else throw new ArgumentException($"'Subject' cannot be null or whitespace.");

            loginClaim.EmployeeCode = CheckClaim("employee_code");
            loginClaim.Firstname = CheckClaim("employee_firstname");
            loginClaim.Lastname = CheckClaim("employee_lastname");
            loginClaim.BranchId = CheckClaim("employee_branchid");
            loginClaim.Branchname = CheckClaim("employee_branchname");
            loginClaim.Token = token;

            Token = token;
        }

        private string CheckClaim(string @type)
        {
            if (!JwtToken.Claims.Where(_ => _.Type == @type).Any()) throw new ArgumentException($"'{type}' cannot be null or whitespace.");

            return JwtToken.Claims.Where(_ => _.Type == @type).First().Value;
        }

        public LoginDetailDto GetClaim()
        {
            return loginClaim;
        }
    }
}