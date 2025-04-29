using System;
namespace BaseCode.Models.Tools
{
    public static class OTPGenerator
    {
        public static string GenerateOTP()
        {
            var random = new Random();
            int otp = random.Next(100000, 1000000);
            return otp.ToString();
        }
    }
}