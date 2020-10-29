using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OperacionesMFiles
{
    /// <summary>
    /// Representa un documento a ser indexado en MFiles
    /// </summary>
    interface MFilesDocument
    {
        public string CodigoERP { get; set; } = "";
        public string Empresa { get; set; } = "";
        public string Departamento { get; set; } = "";
        //public string NumFactura { get; set; } = "";
        //public string NumRetencion { get; set; } = "";
        //public string NumDocumento { get; set; } = "";
        public string RucEmisor{ get; set; } = "";
        public string FechaEmision { get; set; } = "";
        public string Valor { get; set; } = "";

        /*public override string ToString()
        {
            return String.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", CodigoERP, Empresa, Departamento, NumFactura, NumRetencion, NumDocumento, NumFacturaRetenida, RucEmisor, FechaEmision, Valor);
        }*/


    }


    public class Factura: MFilesDocument
    {
        public string NumFactura { get; set; } = "";
    }

    public class Retencion: MFilesDocument
    {
        public string NumFactura { get; set; } = "";
        public string NumRetencion { get; set; } = "";
    }

    public class Documento : MFilesDocument
    {
        public string NumFactura { get; set; } = "";
        public string NumRetencion { get; set; } = "";
    }
}