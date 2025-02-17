using System;

namespace BaseCode.Models.Tools
{
    public static class OTPGenerator
    {
        public static string GenerateOTP(string userId)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            char[] otp = new char[7];

            for (int i = 0; i < 7; i++)
            {
                otp[i] = chars[random.Next(chars.Length)];
            }

            char lastDigit = userId.Length > 0 ? userId[userId.Length - 1] : '0';

            return new string(otp) + lastDigit;
        }
    }
}