using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class HorarioAtencion : LayerSuperType
    {
        public Usuarios? Usuario { get; set; }
        public string DiaSemana { get; set; }  // Lunes, Martes, etc.
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? Comentario { get; set; }

    }
}
