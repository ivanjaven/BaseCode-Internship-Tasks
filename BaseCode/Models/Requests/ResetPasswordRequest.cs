namespace BaseCode.Models.Requests
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }         
        public string CurrentPassword { get; set; } 
        public string NewPassword { get; set; }    
    }
}
