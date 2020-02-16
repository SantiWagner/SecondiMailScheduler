using SecondiMailScheduler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Data
{
    class EventsContext : DbContext
    {
        public EventsContext() : base("MainDatabase")
        {
        }

        public DbSet<Event> Events { get; set; }

    }
}
