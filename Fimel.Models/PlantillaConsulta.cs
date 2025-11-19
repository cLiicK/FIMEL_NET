using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class PlantillaConsulta : LayerSuperType
    {
        public string? Titulo { get; set; }
        public string? Contenido { get; set; }
        public string? TipoPlantilla { get; set; }
        public Usuarios Usuario { get; set; }
    }
}
