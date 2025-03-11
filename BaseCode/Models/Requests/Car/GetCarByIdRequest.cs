using System;

namespace BaseCode.Models.Requests.Car
{
    public class GetCarByIdRequest
    {
        public int UserId { get; set; }
        public int CarId { get; set; }
    }
}