using System;

namespace BaseCode.Models.Requests.Roles
{
    public class GetRoleRequest
    {
        public int UserId { get; set; }
        public int? RoleId { get; set; }
    }
}