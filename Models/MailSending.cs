using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Model
{
    public class MailSending
    {
        public int Id { get; set; }

        public string Recipients { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }

        public string Company { get; set; }

        public string Location { get; set; }

        public string Comitent { get; set; }

        public DateTime SentOn { get; set; }

    }
}
