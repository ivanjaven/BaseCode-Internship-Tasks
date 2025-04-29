using BaseCode.Models.Requests;

namespace BaseCode.Models.Requests.Car
{
    public class GetCarByNameRequest : PaginationRequest
    {
        public string CarName { get; set; }
    }
}