using System;

namespace BaseCode.Models.Requests.Roles
{
    public class AssignUserRoleRequest
    {
        public int RequestUserId { get; set; } 
        public int TargetUserId { get; set; } 
        public int RoleId { get; set; }
    }
}