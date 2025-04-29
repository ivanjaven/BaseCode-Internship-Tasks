using BaseCode.Models.Responses;
using System.Collections.Generic;

namespace BaseCode.Models.Responses.Car
{
    public class GetCarsResponse : GenericAPIResponse
    {
        public List<Dictionary<string, string>> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}