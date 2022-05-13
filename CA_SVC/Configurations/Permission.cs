using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Configurations
{
    public class Permission
    {
        public const string Base = "Bese";

        public static AuthorizationPolicy BasePermission()
            => new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("employee_code")
                .RequireClaim("employee_firstname")
                .RequireClaim("employee_lastname")
                .RequireClaim("employee_branchid")
                .RequireClaim("employee_branchname")
                .Build();

        public const string Read = "Read";

        public static AuthorizationPolicy ReadPermission()
            => new AuthorizationPolicyBuilder()
                .RequireClaim("permission", "employee:read")
                .Build();

        public const string Write = "Write";

        public static AuthorizationPolicy WritePermission()
            => new AuthorizationPolicyBuilder()
                .RequireClaim("permission", "employee:write")
                .Build();

        public const string ReadOrWrite = "ReadOrWrite";

        public static AuthorizationPolicy ReadOrWritePermission()
            => new AuthorizationPolicyBuilder()
                .RequireClaim("permission", "employee:read", "employee:write")
                .Build();

        public const string Delete = "Delete";

        public static AuthorizationPolicy DeletePermission()
            => new AuthorizationPolicyBuilder()
            .RequireClaim("permission", "employee:delete")
            .Build();
    }
}