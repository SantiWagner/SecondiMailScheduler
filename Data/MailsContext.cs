using SecondiMailScheduler.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Data
{
    public class MailsContext : DbContext
    {
        public MailsContext() : base("MainDatabase")
        {
        }

        public DbSet<MailSending> MailsSendings { get; set; }

    }
}
