using System;
using System.Collections.Generic;
using System.Text;

namespace fn_bidtravel_pnrfinisher_portal.models
{
    public class ProcessedItem
    {
        public string Identifier { get; set; }
        public string PCC { get; set; }
        public DateTime DateTimeStamp { get; set; }
        public int Rules { get; set; }
        public int Retries { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
    }
}
