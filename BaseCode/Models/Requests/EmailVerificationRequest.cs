namespace BaseCode.Models.Requests
{
    public class EmailVerificationRequest
    {
        public string Email { get; set; }
        public int? UserId { get; set; }
    }
}