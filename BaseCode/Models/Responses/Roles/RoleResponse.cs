using System;
using BaseCode.Models.Responses;

namespace BaseCode.Models.Responses.Roles
{
    public class RoleResponse : GenericAPIResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    }
}