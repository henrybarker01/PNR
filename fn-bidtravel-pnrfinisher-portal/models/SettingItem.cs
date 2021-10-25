using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace fn_bidtravel_pnrfinisher_portal.models
{
    public class SettingItem : TableEntity
    {
        public string  Value {get;set;}
    }
}
