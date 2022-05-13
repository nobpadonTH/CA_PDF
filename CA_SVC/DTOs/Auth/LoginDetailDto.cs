using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.DTOs.Auth
{
    public class LoginDetailDto
    {
        public string Token { get; set; }
        public string SubjectId { get; set; }
        public string EmployeeCode { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string BranchId { get; set; }
        public string Branchname { get; set; }
    }
}