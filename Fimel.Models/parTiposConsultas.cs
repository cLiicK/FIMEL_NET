using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class parTiposConsultas : LayerSuperType
    {
        public string? Descripcion { get; set; }
        public int? IdEspecialidad { get; set; }
    }
}
