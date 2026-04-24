using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class ConfiguracionUsuario : LayerSuperType
    {
        public Usuarios Usuario { get; set; }
        public TimeSpan DuracionBloqueHorario { get; set; }
        public string? TokenPublico { get; set; }
        public int DiasAvisoPrevioControl { get; set; } = 5;
    }
}
