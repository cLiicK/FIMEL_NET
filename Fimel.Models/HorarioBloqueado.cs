using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class HorarioBloqueado : LayerSuperType
    {
        public Usuarios? Usuario { get; set; }
        public DateTime FechaBloqueo { get; set; }  // Fecha puntual bloqueada
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? Comentario { get; set; }
    }
}
