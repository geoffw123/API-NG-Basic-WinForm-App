using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace API_NG_App.TO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;

    public class MarketCatalogue
    {
        [JsonProperty(PropertyName = "marketId")]
        public string MarketId { get; set; }

        [JsonProperty(PropertyName = "marketName")]
        public string MarketName { get; set; }

        [JsonProperty(PropertyName = "marketStartTime")]
        public DateTime MarketStartTime { get; set; }

        [JsonProperty(PropertyName = "totalMatched")]    // #GW added this more discrepancies between example code and API docs
        public double TotalMatched { get; set; }

        [JsonProperty(PropertyName = "description")]
        public MarketDescription Description { get; set; }

        [JsonProperty(PropertyName = "runners")]
        public List<RunnerDescription> Runners { get; set; }

        [JsonProperty(PropertyName = "eventType")]
        public EventType EventType { get; set; }

        [JsonProperty(PropertyName = "event")]
        public Event Event { get; set; }

        [JsonProperty(PropertyName = "competition")]
        public Competition Competition { get; set; }

        public override string ToString()
        {
            // well, don't bother displaying event/event type/competition
            var sb = new StringBuilder().AppendFormat("{0}", "MarketCatalogue")
                        .AppendFormat(" : Market={0}[{1}]", MarketId, MarketName);

            if (Description != null)
            {
                sb.AppendFormat(" : {0}", Description);
            }

            if (Runners != null && Runners.Count > 0)
            {
                int idx = 0;
                foreach (var runner in Runners)
                {
                    sb.AppendFormat(" : Runner[{0}]={1}", idx++, runner);
                }
            }

            return sb.ToString();
        }
    }
}
