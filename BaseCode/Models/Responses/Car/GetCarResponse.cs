using System.Collections.Generic;

namespace BaseCode.Models.Responses.Car
{
    public class GetCarResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<Dictionary<string, string>> Data { get; set; }
        public int CarId { get; set; }
    }
}