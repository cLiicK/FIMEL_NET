using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class parExamenes : LayerSuperType
    {
        public int? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}
