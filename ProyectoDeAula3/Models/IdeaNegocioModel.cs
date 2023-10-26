using System;
using System.Collections.Generic;

namespace ModeloNegocio
{
    public class IdeaNegocioModel
    {
        public int Id { get; set; }
        public string NombreIdea { get; set; }
        public string ImpactoSocial { get; set; }
        public double Inversion { get; set; }
        public double Ingresos { get; set; }
        public string Herramientas4RI { get; set; }
        public List<Departamento> Departamentos { get; set; } = new List<Departamento>();
        public List<IntegrantesEquipo> Equipos { get; set; } = new List<IntegrantesEquipo>();
    }

    public class Departamento
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
    }

    public class IntegrantesEquipo
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        public String Apellidos { get; set; }
        public String Rol { get; set; }
        public String Email { get; set; }
    }
}
