using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Enums
    {
        public enum EnumTiposDocumento
        {
            RUT,
            PASAPORTE,
            DNIExtranjero,
            OTRO
        }

        public enum EnumPerfiles
        {
            Administrador = 1,
            Especialista = 2,
            Administrativo = 3
        }
    }
}
