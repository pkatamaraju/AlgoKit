using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlgoKit.Data.Core.Models
{
    public class Signal
    {
        [JsonProperty(PropertyName = "scriptName")]
        public string ScriptName { get; set; }

        //[JsonProperty(PropertyName = "ClosePrice")]
        //public int ClosePrice { get; set; }

        //[JsonProperty(PropertyName = "time")]
        //public DateTime time { get; set; }


        [JsonProperty(PropertyName = "buyOrSell")]
        public string BuyOrSell { get; set; }


        [JsonProperty(PropertyName = "regularOrScalping")]
        public string RegularOrScalping { get; set; }

        [JsonProperty(PropertyName = "period")]
        public string Period { get; set; }

    }
}
