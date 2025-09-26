using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class HorarioEspecifico : LayerSuperType
    {
        public Usuarios? Usuario { get; set; }
        public DateTime FechaEspecifica { get; set; }  // Fecha específica para el horario
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? Comentario { get; set; }
    }
}
