using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses.Customer
{
    public class CreateCustomerResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
    }
}
