using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace QuickDesk.Application.ResponseDtos
{
    public class BaseDto
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("RequestMessage")]
        public string RequestMessage { get; set; }
    }
}
