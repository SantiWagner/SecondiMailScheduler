using SecondiMailScheduler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Data
{
    public class SettingsContext : DbContext
    {
        public SettingsContext() : base("MainDatabase")
        {
        }

        public DbSet<Setting> Settings { get; set; }

    }
}