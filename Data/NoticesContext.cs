using SecondiMailScheduler.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Data
{
    public class NoticesContext : DbContext
    {
        public NoticesContext() : base("MainDatabase")
        {
        }

        public DbSet<DueNotice> Notices { get; set; }

    }
}
