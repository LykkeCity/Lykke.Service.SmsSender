﻿using Newtonsoft.Json;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Twilio
{
    public class TwilioResponse
    {
        public string Sid { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        public bool AccountNotActive => ErrorCode == "10001" || ErrorCode == "10003" || ErrorCode == "20005" || ErrorCode == "21472" || ErrorCode == "90010";
    }
}
