using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Fimel.Models
{
    public class FimelDbContext : DbContext
    {
        public FimelDbContext(DbContextOptions<FimelDbContext> options) : base(options) { }

        public DbSet<Consultas> Consultas { get; set; }
        public DbSet<Pacientes> Pacientes { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Reservas> Reservas { get; set; }
        public DbSet<parExamenes> parExamenes { get; set; }
        public DbSet<parEspecialidades> parEspecialidades { get; set; }
        public DbSet<Instituciones> Instituciones { get; set; }
        public DbSet<parTiposConsultas> parTiposConsultas { get; set; }
        public DbSet<Bitacoras> Bitacoras { get; set; }
        public DbSet<Perfiles> Perfiles { get; set; }
        public DbSet<Documentos> Documentos { get; set; }
        public DbSet<BitacoraMensajerias> BitacoraMensajerias { get; set; }
        public DbSet<Config> Config { get; set; }
        public DbSet<HorarioAtencion> HorariosAtencion { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<ConfiguracionUsuario> ConfiguracionesUsuario { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Consultas>().ToTable("Consultas", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Pacientes>().ToTable("Pacientes", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Usuarios>().ToTable("Usuarios", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Reservas>().ToTable("Reservas", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<parExamenes>().ToTable("parExamenes", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<parEspecialidades>().ToTable("parEspecialidades", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Instituciones>().ToTable("Instituciones", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<parTiposConsultas>().ToTable("parTiposConsultas", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Bitacoras>().ToTable("Bitacoras", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Perfiles>().ToTable("Perfiles", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Documentos>().ToTable("Documentos", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<BitacoraMensajerias>().ToTable("BitacoraMensajerias", t => t.ExcludeFromMigrations());
        }
    }
}
