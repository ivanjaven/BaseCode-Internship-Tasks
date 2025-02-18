namespace BaseCode.Models.Responses.Customer
{
    public class GetCustomerProfileResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public string Birthday { get; set; }
        public AddressResponse Address { get; set; }
    }

    public class AddressResponse
    {
        public string House_No { get; set; }
        public string Barangay { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string ZIP { get; set; }
    }
}