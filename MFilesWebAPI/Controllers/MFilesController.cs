using MFilesWebAPI.Models;
using OperacionesMFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Configuration;
using System.Web.Http;

namespace MFilesWebAPI.Controllers
{


    /// <summary>
    /// Integración de M-Files
    /// 
    /// </summary>
    public class MFilesController : ApiController
    {

        private static readonly string server    = WebConfigurationManager.AppSettings["MFILES_SERVER"].ToString();
        private static readonly string boveda    = WebConfigurationManager.AppSettings["MFILES_VAULT"].ToString();
        private static readonly string user      = WebConfigurationManager.AppSettings["MFILES_USER"].ToString();
        private static readonly string pass      = WebConfigurationManager.AppSettings["MFILES_PASS"].ToString();
        private static readonly int codigoERP    = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_ERP_ID"].ToString());

        private static Dictionary<string, int> IdPropiedades = new Dictionary<string, int>
            {
                ["codigoERP"] = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_ERP_ID"].ToString()),
                ["empresa"] = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_EMPRESA"].ToString()),
                ["numDocumento"] = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_NUM_DOCUMENTO"].ToString()),
                ["numFacturaRetenida"] = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_NUM_FACTURA_RETENIDA"].ToString()),
                ["rucEmisor"] = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_RUC_EMISOR"].ToString()),
                ["fechaEmision"] = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_FECHA_EMISION"].ToString()),
                ["valor"] = Int32.Parse(WebConfigurationManager.AppSettings["MFILES_VALOR"].ToString())
        };


        private static ConsultarDocumentos objConsultarDocs = new ConsultarDocumentos(server, boveda, user, pass, IdPropiedades);

        /// <summary>
        /// Obtiene los archivos relacionados al parámetro Código ERP como Lista de Objetos
        /// </summary>
        /// <param name="idERP">Código ERP</param>
        /// <returns>Una lista de tuplas con los bytes y extensión de cada archivo asociado
        /// </returns>

        public List<Tuple<byte[], string>> Get(string idERP)
        {
            return objConsultarDocs.GetFiles(codigoERP, idERP); ;
        }

        




        /// <summary>
        ///  Descarga un solo archivo relacionado al parámetro Código ERP
        /// </summary>
        /// <param name="idERP">Código ERP</param>
        /// <returns>HttpResponseMessage
        /// </returns>
        [HttpGet]
        [Route("api/MFiles/downloadFirst/")]
        public HttpResponseMessage GetDocFirstFile(string idERP)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);

            var archivosDescargados = objConsultarDocs.GetFiles(codigoERP, idERP);

            foreach (Tuple<byte[], string> item in archivosDescargados)
            {
                var file = item.Item1;
                var extension = item.Item2;

                string fileName = $@"{Guid.NewGuid()}." + extension; 

                System.Diagnostics.Debug.WriteLine($"\tFile: {fileName}");

                var fileMemStream = new MemoryStream(file);
                result.Content = new StreamContent(fileMemStream);

                var headers = result.Content.Headers;
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.FileName = fileName;
                headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                headers.ContentLength = fileMemStream.Length;
                
                return result;

            }
            return null;

        }

        /// <summary>
        /// Actualiza la información del Documento en Mfiles
        /// </summary>
        /// <param name="Documento"></param>

        [HttpPost] 
        public String Post([FromBody] MFilesDocument Documento)
        {
            var indexado = objConsultarDocs.IndexarDocumento(Documento.CodigoERP, Documento.Empresa, Documento.NumDocumento, Documento.NumFacturaRetenida, Documento.RucEmisor, Documento.FechaEmision, Documento.Valor);

            if (indexado != null)
                return $"Documento indexado exitosamente ID {indexado} - { DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}";
            else
                return "Documento no indexado " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");

        }




        /*
        /// <summary>
        /// Descarga los archivos relacionados al parámetro Código ERP en formato ZIP
        /// </summary>
        /// <param name="idERP">Código ERP</param>
        /// <returns>HttpResponseMessage con el archivo ZIP</returns>
        [HttpGet]
        [Route("api/MFiles/downloadZip/{id}")]
        public HttpResponseMessage GetDocFiles(string idERP)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var archivosDescargados = objConsultarDocs.GetFiles(codigoERP, idERP);


            
            using (var compressedFileStream = new MemoryStream())
            {
                //Create an archive and store the stream in memory.
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
                {
                    foreach (Tuple<byte[], string> item in archivosDescargados)
                    {
                        //Create a zip entry for each attachment
                        var file = item.Item1;
                        var extension = item.Item2;

                        string fileName = $@"{Guid.NewGuid()}." + extension;
                        var zipEntry = zipArchive.CreateEntry(fileName);

                        //Get the stream of the attachment
                        using (var originalFileStream = new MemoryStream(file))
                        using (var zipEntryStream = zipEntry.Open())
                        {
                            //Copy the attachment stream to the zip entry stream
                            originalFileStream.CopyTo(zipEntryStream);
                        }
                    }
                }


                
                string zipFileName = $@"{Guid.NewGuid()}.zip";

                result.Content = new StreamContent(compressedFileStream);

                var headers = result.Content.Headers;
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.FileName = zipFileName;
                headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                headers.ContentLength = compressedFileStream.Length;
                //return new FileContentResult(compressedFileStream.ToArray(), "application/zip") { FileDownloadName = "Filename.zip" };
            }

            return result;

        }*/



    }
}
