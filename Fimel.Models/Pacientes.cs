using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fimel.Models
{
    public class Pacientes : LayerSuperType
    {
        public int? Rut { get; set; }
        public string? Dv { get; set; }
        public string? Nombres { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public int? Celular { get; set; }
        public string? Email { get; set; }
        public string? Nacionalidad { get; set; }
        public string? Prevision { get; set; }
        public string? AntFamiliares { get; set; }
        public string? AntPersonales { get; set; }
        public string? AntQuirurgicos { get; set; }
        public string? Tabaco { get; set; }
        public string? DescTabaco { get; set; }
        public string? Alcohol { get; set; }
        public string? DescAlcohol { get; set; }
        public string? Drogas { get; set; }
        public string? DescDrogas { get; set; }
        public string? Alergias { get; set; }
        public string? DescAlergias { get; set; }
        public int? Gesta { get; set; }
        public int? Parto { get; set; }
        public int? Aborto { get; set; }
        public string? Menarquia { get; set; }
        public string? Menopausia { get; set; }
        public string? Religion { get; set; }
        public string? RegimenAlimenticio { get; set; }
        public string? Medicamentos { get; set; }
        public int? UsuarioCreacion { get; set; }
        public string? SexoBiologico { get; set; }
        public string? IdentidadGenero { get; set; }
        public string? OrientacionSexual { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
    }
}
