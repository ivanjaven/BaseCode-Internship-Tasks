using System;
using System.Collections.Generic;
using BaseCode.Models.Responses;

namespace BaseCode.Models.Responses.Roles
{
    public class GetRoleListResponse : GenericAPIResponse
    {
        public List<Dictionary<string, string>> Data { get; set; }
    }
}