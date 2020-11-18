using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace OperacionesMFiles
{
    /// <summary>
    /// Clase base representando con los campos generales de cada tipo
    /// </summary>
    
    public class MFilesDocument
    {
        /// <value>Código del ERP/value>
        public string CodigoERP { get; set; } 
        public string Empresa { get; set; }
        public string Departamento { get; set; }
        public string RucEmisor{ get; set; }
        public string FechaEmision { get; set; }
        
    }

    /// <summary>
    /// Representa una Factura
    /// </summary>
    public class Factura :MFilesDocument
    {
        public string NumFactura { get; set; }
        public string Valor { get; set; }
        /// <summary>
        /// Genera una lista con el nombre de la propiedad, el valor y el tipo
        /// </summary>
        /// <returns>Lista con tupla de 3 items</returns>
        public List<(string,string,string)> GetMFilesProperties()
        {
            var lista = new List<(string, string, string)>();

            lista.Add(("Empresa", Empresa, "Text"));
            lista.Add(("Departamento", Departamento, "Text"));
            lista.Add(("NumFactura", NumFactura, "Text"));
            lista.Add(("RucEmisor", RucEmisor, "Text"));
            lista.Add(("FechaEmision", FechaEmision, "Date"));
            lista.Add(("Valor", Valor.ToString(), "Floating"));
            return lista;
        }
    }

    /// <summary>
    /// Representa una retencion
    /// </summary>
    public class Retencion : MFilesDocument
    {
        public string NumFactura { get; set; }
        public string NumRetencion { get; set; }

        /// <summary>
        /// Genera una lista con el nombre de la propiedad, el valor y el tipo
        /// </summary>
        /// <returns>Lista con tupla de 3 items</returns>
        public List<(string, string, string)> GetMFilesProperties()
        {
            var lista = new List<(string, string, string)>();

            lista.Add(("Empresa", Empresa, "Text"));
            lista.Add(("Departamento", Departamento, "Text"));
            lista.Add(("NumFactura", NumFactura, "Text"));
            lista.Add(("NumRetencion", NumRetencion, "Text"));
            lista.Add(("RucEmisor", RucEmisor, "Text"));
            lista.Add(("FechaEmision", FechaEmision, "Date"));

            return lista;
        }
    }


    /// <summary>
    /// Representa otro tipo de documento
    /// </summary>
    public class Documento : MFilesDocument
    {
        public string NumDocumento { get; set; }

        /// <summary>
        /// Genera una lista con el nombre de la propiedad, el valor y el tipo
        /// </summary>
        /// <returns>Lista con tupla de 3 items</returns>
        public List<(string, string, string)> GetMFilesProperties()
        {
            var lista = new List<(string, string, string)>();

            lista.Add(("Empresa", Empresa, "Text"));
            lista.Add(("Departamento", Departamento, "Text"));
            lista.Add(("NumDocumento", NumDocumento, "Text"));
            lista.Add(("FechaEmision", FechaEmision, "Date"));
            lista.Add(("RucEmisor", RucEmisor, "Text"));

            return lista;
        }
    }
}