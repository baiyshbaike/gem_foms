using System;
namespace Dialysis.Shared.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string ProfileName { get; set; }
        public string Fio { get; set; }
    }

    public class ResultResponse
    {
        public string Data { get; set; }
        public string RefreshToken { get; set; }
        public string ProfileName { get; set; }
        public string Fio { get; set; }
    }
}

