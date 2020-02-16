using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Models
{
    public class Event
    {
        public int Id { get; set; }

        public DateTime _TimeStamp { get; set; }

        public int Code { get; set; }

        public string Content { get; set; }
    }
}
