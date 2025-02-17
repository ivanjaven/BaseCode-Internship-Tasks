namespace BaseCode.Models.Requests
{
    public class OTPValidationRequest
    {
        public string UserId { get; set; }
        public string OTPCode { get; set; }
    }
}
