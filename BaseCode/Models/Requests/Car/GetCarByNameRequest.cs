using System;

namespace BaseCode.Models.Requests.Car
{
    public class GetCarByNameRequest
    {
        public int UserId { get; set; }
        public string CarName { get; set; }
    }
}