using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Models
{
    public class Setting
    {
        public int Id { get; set; }

        public bool TestMode { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public bool EnableSSL { get; set; }

        public string UserName { get; set; }

        public string UserDisplay { get; set; }

        public string Password { get; set; }

        public string TestModeRecipient { get; set; }

        public DateTime LastUpdate { get; set; }
    }
}
