using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondiMailScheduler.Model
{
    public class DueNotice
    {
        public int Id { get; set; }

        public string Company { get; set; }

        public string Recipients { get; set; }

        public string Comitent { get; set; }

        public string Location { get; set; }

        public string ContentPersonal { get; set; }

        public string ContentVehiculos { get; set; }

        public string ContentEmpresa { get; set; }

        public bool Processed { get; set; }
    }
}
