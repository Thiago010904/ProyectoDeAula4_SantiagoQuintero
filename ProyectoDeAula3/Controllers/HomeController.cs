using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Web.Mvc;
using ModeloNegocio;

namespace ProyectoDeAula3.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = "Data Source=|DataDirectory|/dproyect4.db;Version=3;";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AgregarIdeaNegocio()
        {
            return View(new IdeaNegocioModel());
        }

        public ActionResult IdeaDepartamentos()
        {
            IdeaNegocioModel ideaDestacada = BuscarIdeaDestacada();
            return View(ideaDestacada);
        }

        public IdeaNegocioModel BuscarIdeaDestacada()
        {
            IdeaNegocioModel ideaDestacada = null;
            int maxDepartamentosBeneficiados = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM ideanegocio", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ideaNegocio = new IdeaNegocioModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                NombreIdea = reader["nombreidea"].ToString(),
                                ImpactoSocial = reader["impactosocial"].ToString(),
                                Inversion = Convert.ToDouble(reader["inversion"]),
                                Ingresos = Convert.ToDouble(reader["ingresos"]),
                                Herramientas4RI = reader["herramientasri"].ToString(),
                            };

                            ideaNegocio.Departamentos = CargarDepartamentos(ideaNegocio.Id, connection);
                            ideaNegocio.Equipos = CargarEquipos(ideaNegocio.Id, connection);

                            if (ideaNegocio.Departamentos.Count > maxDepartamentosBeneficiados)
                            {
                                maxDepartamentosBeneficiados = ideaNegocio.Departamentos.Count;
                                ideaDestacada = ideaNegocio;
                            }
                        }
                    }
                }
            }

            return ideaDestacada;
        }



        public ActionResult MostrarIdeaNegocio()
        {
            List<IdeaNegocioModel> ideas = new List<IdeaNegocioModel>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM ideanegocio", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ideaNegocio = new IdeaNegocioModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                NombreIdea = reader["nombreidea"].ToString(),
                                ImpactoSocial = reader["impactosocial"].ToString(),
                                Inversion = Convert.ToDouble(reader["inversion"]),
                                Ingresos = Convert.ToDouble(reader["ingresos"]),
                                Herramientas4RI = reader["herramientasri"].ToString(),
                            };

                            ideaNegocio.Departamentos = CargarDepartamentos(ideaNegocio.Id, connection);
                            ideaNegocio.Equipos = CargarEquipos(ideaNegocio.Id, connection);

                            ideas.Add(ideaNegocio);
                        }
                    }
                }
            }

            return View(ideas);
        }

        private List<IntegrantesEquipo> CargarEquipos(int ideaNegocioId, SQLiteConnection connection)
        {
            List<IntegrantesEquipo> equipos = new List<IntegrantesEquipo>();

            using (var cmd = new SQLiteCommand("SELECT e.* FROM integrantesequipo AS e WHERE e.ideanegocio_id = @ideanegocio", connection))
            {
                cmd.Parameters.AddWithValue("@ideanegocio", ideaNegocioId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var equipo = new IntegrantesEquipo
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Nombre = reader["nombre"].ToString(),
                            Apellidos = reader["apellido"].ToString(),
                            Rol = reader["rol"].ToString(),
                            Email = reader["email"].ToString(),
                        };
                        equipos.Add(equipo);
                    }
                }
            }

            return equipos;
        }

        private List<Departamento> CargarDepartamentos(int ideaNegocioId, SQLiteConnection connection)
        {
            List<Departamento> departamentos = new List<Departamento>();

            using (var cmd = new SQLiteCommand("SELECT d.* FROM departamento AS d INNER JOIN ideanegocio_departamento AS id ON d.Id = id.departamento_id WHERE id.ideanegocio_id = @ideanegocio", connection))
            {
                cmd.Parameters.AddWithValue("@ideanegocio", ideaNegocioId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var departamento = new Departamento
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Nombre = reader["nombre"].ToString(),
                        };
                        departamentos.Add(departamento);
                    }
                }
            }

            return departamentos;
        }

        [HttpPost]
        public ActionResult AgregarIdeaNegocio(IdeaNegocioModel ideaNegocio)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                int codigo = 0;

                using (var cmd = new SQLiteCommand("INSERT INTO ideanegocio (nombreidea, impactosocial, inversion, ingresos, herramientasri) VALUES (@nombre, @impacto, @inversion, @ingresos, @herramientas); SELECT last_insert_rowid();", connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", ideaNegocio.NombreIdea);
                    cmd.Parameters.AddWithValue("@impacto", ideaNegocio.ImpactoSocial);
                    cmd.Parameters.AddWithValue("@inversion", ideaNegocio.Inversion);
                    cmd.Parameters.AddWithValue("@ingresos", ideaNegocio.Ingresos);
                    cmd.Parameters.AddWithValue("@herramientas", ideaNegocio.Herramientas4RI);

                    codigo = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (var equipo in ideaNegocio.Equipos)
                {
                    using (var cmdEquipo = new SQLiteCommand("INSERT INTO integrantesequipo (nombre, apellido, rol, email, ideanegocio_id) VALUES (@nombre, @apellido, @rol, @email, @ideanegocio);", connection))
                    {
                        cmdEquipo.Parameters.AddWithValue("@nombre", equipo.Nombre);
                        cmdEquipo.Parameters.AddWithValue("@apellido", equipo.Apellidos);
                        cmdEquipo.Parameters.AddWithValue("@rol", equipo.Rol);
                        cmdEquipo.Parameters.AddWithValue("@email", equipo.Email);
                        cmdEquipo.Parameters.AddWithValue("@ideanegocio", codigo);

                        cmdEquipo.ExecuteNonQuery();
                    }
                }

                List<int> departamentoIds = new List<int>();
                foreach (var departamento in ideaNegocio.Departamentos)
                {
                    using (var cmdDepartamento = new SQLiteCommand("INSERT INTO departamento (nombre) VALUES (@nombre); SELECT last_insert_rowid();", connection))
                    {
                        cmdDepartamento.Parameters.AddWithValue("@nombre", departamento.Nombre);
                        int departamentoId = Convert.ToInt32(cmdDepartamento.ExecuteScalar());
                        departamentoIds.Add(departamentoId);
                    }
                }

                foreach (var departamentoId in departamentoIds)
                {
                    using (var cmdRelacion = new SQLiteCommand("INSERT INTO ideanegocio_departamento (ideanegocio_id, departamento_id) VALUES (@ideanegocio, @departamento);", connection))
                    {
                        cmdRelacion.Parameters.AddWithValue("@ideanegocio", codigo);
                        cmdRelacion.Parameters.AddWithValue("@departamento", departamentoId);

                        cmdRelacion.ExecuteNonQuery();
                    }
                }
            }

            TempData["Mensaje"] = "Nueva idea de negocio agregada con éxito.";

            return RedirectToAction("MostrarIdeaNegocio");
        }

        public ActionResult EditarEquipo(int equipoId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SQLiteCommand("SELECT * FROM integrantesequipo WHERE id = @equipoId", connection))
                {
                    cmd.Parameters.AddWithValue("@equipoId", equipoId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var equipo = new IntegrantesEquipo
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Nombre = reader["nombre"].ToString(),
                                Apellidos = reader["apellido"].ToString(),
                                Rol = reader["rol"].ToString(),
                                Email = reader["email"].ToString(),
                            };
                            return View(equipo);
                        }
                    }
                }
            }

            return HttpNotFound("El equipo no se encontró o no existe.");
        }

        [HttpPost]
        public ActionResult ActualizarEquipo(IntegrantesEquipo equipo)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = new SQLiteCommand("UPDATE integrantesequipo SET nombre = @nombre, apellido = @apellido, rol = @rol, email = @email WHERE id = @equipoId", connection))
                    {
                        cmd.Parameters.AddWithValue("@nombre", equipo.Nombre);
                        cmd.Parameters.AddWithValue("@apellido", equipo.Apellidos);
                        cmd.Parameters.AddWithValue("@rol", equipo.Rol);
                        cmd.Parameters.AddWithValue("@email", equipo.Email);
                        cmd.Parameters.AddWithValue("@equipoId", equipo.Id);

                        cmd.ExecuteNonQuery();
                    }

                    TempData["Mensaje"] = "Equipo actualizado con éxito.";
                    return RedirectToAction("MostrarIdeaNegocio"); // Redirige a la vista que muestra los detalles de la idea de negocio.
                }
            }

            return View(equipo);
        }

        // Acción para eliminar un equipo
        [HttpPost]
        public ActionResult EliminarEquipo(int equipoId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var cmd = new SQLiteCommand("DELETE FROM integrantesequipo WHERE id = @equipoId", connection))
                {
                    cmd.Parameters.AddWithValue("@equipoId", equipoId);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Mensaje"] = "Equipo eliminado con éxito.";
            return RedirectToAction("MostrarIdeaNegocio");
        }
        public ActionResult IdeaMayorIngresos()
        {
            IdeaNegocioModel ideaMayorIngresos = null;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT * FROM ideanegocio ORDER BY ingresos DESC LIMIT 1", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ideaMayorIngresos = new IdeaNegocioModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                NombreIdea = reader["nombreidea"].ToString(),
                                ImpactoSocial = reader["impactosocial"].ToString(),
                                Inversion = Convert.ToDouble(reader["inversion"]),
                                Ingresos = Convert.ToDouble(reader["ingresos"]),
                                Herramientas4RI = reader["herramientasri"].ToString(),
                            };
                        }
                    }
                }
            }

            return View(ideaMayorIngresos);
        }

        public ActionResult IdeasMasRentables()
        {
            List<IdeaNegocioModel> ideasMasRentables = new List<IdeaNegocioModel>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT *, (ingresos - inversion) AS rentabilidad FROM ideanegocio ORDER BY rentabilidad DESC LIMIT 3", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ideaNegocio = new IdeaNegocioModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                NombreIdea = reader["nombreidea"].ToString(),
                                ImpactoSocial = reader["impactosocial"].ToString(),
                                Inversion = Convert.ToDouble(reader["inversion"]),
                                Ingresos = Convert.ToDouble(reader["ingresos"]),
                                Herramientas4RI = reader["herramientasri"].ToString(),
                            };

                            ideasMasRentables.Add(ideaNegocio);
                        }
                    }
                }
            }

            return View(ideasMasRentables);
        }

        public ActionResult VerIdeasMasRentables()
        {
            return RedirectToAction("IdeasMasRentables");
        }

        public ActionResult IdeasConMasImpactoDepartamentos()
        {
            List<IdeaNegocioModel> ideasConMasImpacto = new List<IdeaNegocioModel>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT i.* FROM ideanegocio AS i " +
                                                   "INNER JOIN ideanegocio_departamento AS id ON i.id = id.ideanegocio_id " +
                                                   "GROUP BY i.id HAVING COUNT(id.departamento_id) > 3", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ideaNegocio = new IdeaNegocioModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                NombreIdea = reader["nombreidea"].ToString(),
                                ImpactoSocial = reader["impactosocial"].ToString(),
                                Inversion = Convert.ToDouble(reader["inversion"]),
                                Ingresos = Convert.ToDouble(reader["ingresos"]),
                                Herramientas4RI = reader["herramientasri"].ToString(),
                            };

                            ideasConMasImpacto.Add(ideaNegocio);
                        }
                    }
                }
            }

            return View(ideasConMasImpacto);
        }

        public ActionResult VerIdeasConMasImpactoDepartamentos()
        {
            return RedirectToAction("IdeasConMasImpactoDepartamentos");
        }

        public ActionResult SumaTotalIngresosInversion()
        {
            double sumaIngresos = 0;
            double sumaInversion = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmdIngresos = new SQLiteCommand("SELECT SUM(ingresos) FROM ideanegocio", connection))
                {
                    sumaIngresos = Convert.ToDouble(cmdIngresos.ExecuteScalar());
                }

                using (var cmdInversion = new SQLiteCommand("SELECT SUM(inversion) FROM ideanegocio", connection))
                {
                    sumaInversion = Convert.ToDouble(cmdInversion.ExecuteScalar());
                }
            }

            ViewBag.SumaTotalIngresos = sumaIngresos;
            ViewBag.SumaTotalInversion = sumaInversion;

            return View();
        }

        public ActionResult IdeasConInteligenciaArtificial()
        {
            List<IdeaNegocioModel> ideasConIA = new List<IdeaNegocioModel>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT id, nombreidea FROM ideanegocio WHERE LOWER(herramientasri) LIKE '%inteligencia artificial%'", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ideaNegocio = new IdeaNegocioModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                NombreIdea = reader["nombreidea"].ToString(),
                            };

                            ideasConIA.Add(ideaNegocio);
                        }
                    }
                }
            }

            return View(ideasConIA);
        }

        public ActionResult IdeasDesarrolloSostenible()
        {
            List<IdeaNegocioModel> ideasDesarrolloSostenible = new List<IdeaNegocioModel>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SQLiteCommand("SELECT id, nombreidea FROM ideanegocio WHERE LOWER(impactosocial) LIKE '%desarrollo sostenible%'", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var ideaNegocio = new IdeaNegocioModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                NombreIdea = reader["nombreidea"].ToString(),
                            };

                            ideasDesarrolloSostenible.Add(ideaNegocio);
                        }
                    }
                }
            }

            return View(ideasDesarrolloSostenible);
        }

        public ActionResult IdeasMayoresAlPromedio()
        {
            List<IdeaNegocioModel> ideas = new List<IdeaNegocioModel>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Calcular el promedio de ingresos de todas las ideas
                using (var cmdPromedio = new SQLiteCommand("SELECT AVG(ingresos) AS PromedioIngresos FROM ideanegocio", connection))
                {
                    using (var reader = cmdPromedio.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            double promedioIngresos = Convert.ToDouble(reader["PromedioIngresos"]);

                            // Obtiene las ideas de negocio cuyos ingresos son mayores que el promedio
                            using (var cmd = new SQLiteCommand("SELECT * FROM ideanegocio WHERE ingresos > @promedio", connection))
                            {
                                cmd.Parameters.AddWithValue("@promedio", promedioIngresos);
                                using (var readerIdeas = cmd.ExecuteReader())
                                {
                                    while (readerIdeas.Read())
                                    {
                                        var ideaNegocio = new IdeaNegocioModel
                                        {
                                            Id = Convert.ToInt32(readerIdeas["id"]),
                                            NombreIdea = readerIdeas["nombreidea"].ToString(),
                                            ImpactoSocial = readerIdeas["impactosocial"].ToString(),
                                            Inversion = Convert.ToDouble(readerIdeas["inversion"]),
                                            Ingresos = Convert.ToDouble(readerIdeas["ingresos"]),
                                            Herramientas4RI = readerIdeas["herramientasri"].ToString(),
                                        };
                                        ideas.Add(ideaNegocio);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return View(ideas);
        }




    }
}
