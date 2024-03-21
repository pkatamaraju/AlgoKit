using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoKit.Data.Core.Models
{
    public class InstrumentPosition
    {
        public DateTime Time { get; set; }
        public uint InstrumentToken { get; set; }

        public int Quantity { get; set; }
        public Decimal AveragePrice { get; set; }

        public String Status { get; set; }

        public decimal TriggerPrice { get; set; }
    }
}
