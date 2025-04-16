using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Reservas : LayerSuperType
    {
        public int? IdUsuario { get; set; }
        public DateTime? FechaHora { get; set; }
        public int? IdTipoConsulta { get; set; }
        public int? IdEspecialidad { get; set; }
        public int? IdPaciente { get; set; }
    }
}
