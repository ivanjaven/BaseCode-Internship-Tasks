using BaseCode.Models.Requests;

namespace BaseCode.Models.Requests.Car
{
    public class GetAllCarsRequest : PaginationRequest
    {
        public string Status { get; set; } = "A";
    }
}