using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Instituciones : LayerSuperType
    {
        public int? Rut { get; set; }
        public string? Dv { get; set; }
        public string? RazonSocial { get; set; }
        public string? Dirección { get; set; }
        public int? Telefono { get; set; }
        public string? Email { get; set; }
    }
}
