using System;

namespace BaseCode.Models.Requests.Car
{
    public class GetAllCarsRequest
    {
        public int UserId { get; set; }
        public string Status { get; set; } = "A"; 
    }
}