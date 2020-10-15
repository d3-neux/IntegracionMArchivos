using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MFilesWebAPI.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MFilesDocument
    {
        public string CodigoERP { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string NumDocumento { get; set; } = "";
        public string NumFacturaRetenida{ get; set; } = "";
        public string RucEmisor{ get; set; } = "";
        public string FechaEmision { get; set; } = "";
        public string Valor { get; set; } = "";

    }
}