using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web.Mvc;

public class MostrarIdeaController : Controller
{
    private static List<IdeaNegocioModel> ideasDeNegocio = new List<IdeaNegocioModel>();

    [HttpPost]
    public ActionResult ProcesarFormulario(IdeaNegocioModel ideaNegocio)
    {
        string nombreIdea = ideaNegocio.Nombre;
        string impactoSocial = ideaNegocio.ImpactoSocial;
        double inversion = ideaNegocio.Inversion;
        double ingresos = ideaNegocio.Ingresos;
        string herramientas4RI = ideaNegocio.Herramientas4RI;
        List<Departamento> departamentos = ideaNegocio.Departamentos;
        List<IntegrantesEquipo> integrantes = ideaNegocio.Equipos;


        ideasDeNegocio.Add(ideaNegocio);

        return RedirectToAction("AccionConfirmacion");
    }

    public ActionResult AccionConfirmacion()
    {
        return View();
    }
}
