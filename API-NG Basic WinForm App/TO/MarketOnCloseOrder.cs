using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace API_NG_App.TO
{
    public class MarketOnCloseOrder
    {
        [JsonProperty(PropertyName = "size")]
        public double Size { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                        .AppendFormat("Size={0}", Size)
                        .ToString();
        }
    }
}
