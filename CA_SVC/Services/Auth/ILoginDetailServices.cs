using CA_SVC.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Services.Auth
{
    public interface ILoginDetailServices
    {
        LoginDetailDto GetClaim();
    }
}