using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Usuarios : LayerSuperType
    {
        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? Usuario { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? RequiereCambioClave { get; set; }
        public int IdPerfil { get; set; }
        public int? IdInstitucion { get; set; }

        public Perfiles Perfil { get; set; } 
    }
}
