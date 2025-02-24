using System;

namespace BaseCode.Models.Requests.Roles
{
    public class CreateRoleRequest
    {
        public int UserId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    }
}