using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace fn_bidtravel_pnrfinisher_portal.models
{
    public class UserItem : TableEntity
    {
        public string UniqueID { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; } 

    }
}
