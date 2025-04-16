using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class BitacoraMensajerias : LayerSuperType
    {
        public string Destinatarios { get; set; }
        public string CC { get; set; }
        public string CCO { get; set; }
        public string Asunto { get; set; }
        public string CuerpoCorreo { get; set; }
    }
}
