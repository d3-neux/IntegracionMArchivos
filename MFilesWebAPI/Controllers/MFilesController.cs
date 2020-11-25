using OperacionesMFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Configuration;
using System.Web.Http;
using NLog;
using Newtonsoft.Json;

namespace MFilesWebAPI.Controllers
{


    /// <summary>
    /// Integración de M-Files
    /// 
    /// </summary>
    public class MFilesController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private static readonly string server    = WebConfigurationManager.AppSettings["MFILES_SERVER"].ToString();
        private static readonly string boveda    = WebConfigurationManager.AppSettings["MFILES_VAULT"].ToString();
        private static readonly string user      = WebConfigurationManager.AppSettings["MFILES_USER"].ToString();
        private static readonly string pass      = WebConfigurationManager.AppSettings["MFILES_PASS"].ToString();



        private static readonly Dictionary<string, int> IdPropiedades = new Dictionary<string, int>
        {
            ["CodigoERP"]           = Int32.Parse(WebConfigurationManager.AppSettings["CodigoERP"].ToString()),
            ["Empresa"]             = Int32.Parse(WebConfigurationManager.AppSettings["Empresa"].ToString()),
            ["NumDocumento"]        = Int32.Parse(WebConfigurationManager.AppSettings["NumDocumento"].ToString()),
            ["NumRetencion"]        = Int32.Parse(WebConfigurationManager.AppSettings["NumRetencion"].ToString()),
            ["NumFactura"]          = Int32.Parse(WebConfigurationManager.AppSettings["NumFactura"].ToString()),
            ["RucEmisor"]           = Int32.Parse(WebConfigurationManager.AppSettings["RucEmisor"].ToString()),
            ["FechaEmision"]        = Int32.Parse(WebConfigurationManager.AppSettings["FechaEmision"].ToString()),
            ["Valor"]               = Int32.Parse(WebConfigurationManager.AppSettings["Valor"].ToString()),
            ["Clase"]               = Int32.Parse(WebConfigurationManager.AppSettings["Clase"].ToString()),
            
            ["Estado"]              = Int32.Parse(WebConfigurationManager.AppSettings["Estado"].ToString()),
            ["IDEstado"]            = Int32.Parse(WebConfigurationManager.AppSettings["IDEstado"].ToString())
        };

        private static readonly IntegracionMFiles objIntegracionMFiles = new IntegracionMFiles(server, boveda, user, pass, IdPropiedades);

        /// <summary>
        /// Obtiene tupla de bytes (archivo) y extensión del documento relacionado al Código ERP
        /// </summary>
        /// <param name="codigoERP">Código ERP</param>
        /// <returns>Una lista de tuplas con los bytes y extensión de cada archivo asociado</returns>
        
        // API/MFiles/{ID}
        [HttpGet]
        [Route("api/MFiles/")]
        public Tuple<byte[], string> Get(string codigoERP)
        {
            //Devuelve el objeto como respuesta
            return objIntegracionMFiles.GetFile(IdPropiedades["CodigoERP"], codigoERP); ;
        }

        /// <summary>
        ///  Descarga el archivo relacionado al Código ERP
        /// </summary>
        /// <param name="codigoERP">Código ERP</param>
        /// <returns>HttpResponseMessage con el archivo como contenido </returns>
        [HttpGet]
        [Route("api/MFiles/downloadFile/")]
        public HttpResponseMessage GetDocFirstFile(string codigoERP)
        {
            //Descarga los archivos usando el Código ERP
            var archivosDescargados = objIntegracionMFiles.GetFile(IdPropiedades["CodigoERP"], codigoERP);

            //Obtiene el archivo en bytes y la extención, si es null el mensaje de error es extraído desde Item2
            var file = archivosDescargados.Item1;
            var extension = archivosDescargados.Item2;
            
            HttpResponseMessage result;
            
            if (file == null)
            {
                result = new HttpResponseMessage(HttpStatusCode.NotFound);
                result.Content = new StringContent(extension);
            }
            else
            {
                //Genera un nombre único para el archivo
                string fileName = $@"{Guid.NewGuid()}." + extension;
                System.Diagnostics.Debug.WriteLine($"\tArchivo descargado: {fileName}");


                //Se crea el archivo y se lo asigna al mensaje de respuesta
                var fileMemStream = new MemoryStream(file);

                result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(fileMemStream);

                //se define el header de la respuesta

                var headers = result.Content.Headers;
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.FileName = fileName;
                headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                headers.ContentLength = fileMemStream.Length;
            }
            return result;
        }

        /// <summary>
        /// Actualiza la información de un Documento en M-Files que coincida con el Código ERP
        /// </summary>
        /// <param name="Documento">Objeto con la información a actualizar del documento</param>
        /// <returns>Cadena con el resultado de la indexación</returns>
        [HttpPut]
        [Route("api/MFiles/IndexarDocumento")]
        public String Put([FromBody] Documento Documento)
        {
            String resultado = objIntegracionMFiles.IndexarDocumento(Documento);
            logger.Info("IndexarDocumento: " + resultado + " -- Request BODY: " + JsonConvert.SerializeObject(Documento));
            return resultado;
        }

        /// <summary>
        /// Actualiza la información de una Factura en M-Files que coincida con el Código ERP
        /// </summary>
        /// <param name="Documento">Objeto con la información a actualizar de la Factura</param>
        /// <returns>Cadena con el resultado de la indexación</returns>
        [HttpPut]
        [Route("api/MFiles/IndexarFactura")]
        public String Put([FromBody] Factura Documento)
        {
            //Devuelve el resultado de la indexación
            //Devuelve el resultado de la indexación
            String resultado = objIntegracionMFiles.IndexarDocumento(Documento);
            logger.Info("IndexarFactura: " + resultado + " -- Request BODY: " + JsonConvert.SerializeObject(Documento));
            return resultado;
        }

        /// <summary>
        /// Actualiza la información de una Retención en M-Files que coincida con el Código ERP
        /// </summary>
        /// <param name="Documento">Objeto con la información a actualizar de la Retención</param>
        /// <returns>Cadena con el resultado de la indexación</returns>
        [HttpPut]
        [Route("api/MFiles/IndexarRetencion")]
        public String Put([FromBody] Retencion Documento)
        {
            //Devuelve el resultado de la indexación
            String resultado = objIntegracionMFiles.IndexarDocumento(Documento);

            logger.Info("IndexarRetencion: " + resultado + " -- Request BODY: " + JsonConvert.SerializeObject(Documento));

            return resultado;
        }
    }
}
