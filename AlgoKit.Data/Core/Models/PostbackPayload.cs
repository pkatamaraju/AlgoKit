using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoKit.Data.Core.Models
{
    public class PostbackPayload
    {

        public string user_id { get; set; }
        public string unfilled_quantity { get; set; }
        public Int64 app_id { get; set; }

        public string checksum { get; set; }

        public string placed_by { get; set; }
        public string order_id { get; set; }
        public string exchange_order_id { get; set; }
        public string parent_order_id { get; set; }
        public string status { get; set; }
        public string status_message { get; set; }
        public string status_message_raw { get; set; }
        public string order_timestamp { get; set; }
        public string exchange_update_timestamp { get; set; }
        public string exchange_timestamp { get; set; }
        public string variety { get; set; }
        public string exchange { get; set; }
        public string tradingsymbol { get; set; }
        public uint instrument_token { get; set; }
        public string order_type { get; set; }
        public string transaction_type { get; set; }
        public string validity { get; set; }
        public string product { get; set; }
        public Int64 quantity { get; set; }
        public Int64 disclosed_quantity { get; set; }
        public decimal price { get; set; }
        public decimal trigger_price { get; set; }
        public decimal average_price { get; set; }
        public Int64 filled_quantity { get; set; }
        public Int64 pending_quantity { get; set; }
        public Int64 cancelled_quantity { get; set; }
        public string market_protection { get; set; }
        public string meta { get; set; }
        public string tag { get; set; }
        public string guid { get; set; }
    }

}
