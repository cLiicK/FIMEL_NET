using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Documentos : LayerSuperType
    {
        public int Id_Paciente { get; set; }
        public string? Nombre { get; set; }
        public string? Formato { get; set; }
        public string? DocumentoBase64 { get; set; }
        public int? UsuarioCreacion { get; set; }
    }
}
