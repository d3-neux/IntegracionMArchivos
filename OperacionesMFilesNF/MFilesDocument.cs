using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OperacionesMFiles
{
    /// <summary>
    /// Representa un documento a ser indexado en MFiles
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

        public override string ToString()
        {
            return String.Format("{0}:{1}:{2}:{3}:{4}:{5}", CodigoERP, Empresa, NumDocumento, NumFacturaRetenida, RucEmisor, FechaEmision, Valor);
        }


    }
}