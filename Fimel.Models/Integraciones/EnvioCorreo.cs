using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models.Integraciones
{
    public class EnvioCorreo
    {
        public List<string> Destinatarios { get; set; }
        public List<string>? CC { get; set; }
        public List<string>? CCO { get; set; }
        public string? Asunto { get; set; }
        public string? CuerpoCorreo { get; set; }
    }
}
