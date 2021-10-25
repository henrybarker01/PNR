using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace fn_bidtravel_pnrfinisher_portal.models
{
    public class RunHistoryItem : TableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        //public DateTime DateTimeStamp { get; set; } = DateTime.UtcNow;

        public string RuleName { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string Locator { get; set; }
        public string Mandatory { get; set; }
        public string Segment { get; set; }
    }
}
