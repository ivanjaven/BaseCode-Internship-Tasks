using System;

namespace BaseCode.Models.Requests.Roles
{
    public class AssignPermissionRequest
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }
}