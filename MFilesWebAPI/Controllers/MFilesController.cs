using OperacionesMFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Configuration;
using System.Web.Http;
using MFilesDocument = OperacionesMFiles.MFilesDocument;

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

        private static readonly Dictionary<string, int> IdPropiedades = new Dictionary<string, int>
        {
            ["codigoERP"]           = Int32.Parse(WebConfigurationManager.AppSettings["CODIGO_ERP"].ToString()),
            ["empresa"]             = Int32.Parse(WebConfigurationManager.AppSettings["EMPRESA"].ToString()),
            ["numDocumento"]        = Int32.Parse(WebConfigurationManager.AppSettings["NUM_DOCUMENTO"].ToString()),
            ["numFacturaRetenida"]  = Int32.Parse(WebConfigurationManager.AppSettings["NUM_FACTURA_RETENIDA"].ToString()),
            ["rucEmisor"]           = Int32.Parse(WebConfigurationManager.AppSettings["RUC_EMISOR"].ToString()),
            ["fechaEmision"]        = Int32.Parse(WebConfigurationManager.AppSettings["FECHA_EMISION"].ToString()),
            ["valor"]               = Int32.Parse(WebConfigurationManager.AppSettings["VALOR"].ToString())
        };

        private static IntegracionMFiles objConsultarDocs = new IntegracionMFiles(server, boveda, user, pass, IdPropiedades);

        /// <summary>
        /// Obtiene tupla de bytes (archivo) y extensión del documento relacionado al código ERP
        /// </summary>
        /// <param name="codigoERP">Código ERP</param>
        /// <returns>Una lista de tuplas con los bytes y extensión de cada archivo asociado</returns>
        
        // API/MFiles/{ID}
        [HttpGet]
        [Route("api/MFiles/")]
        public Tuple<byte[], string> Get(string codigoERP)
        {
            //Devuelve el objeto como respuesta
            return objConsultarDocs.GetFile(IdPropiedades["codigoERP"], codigoERP); ;
        }


        /// <summary>
        ///  Descarga el archivo relacionado al código ERP
        /// </summary>
        /// <param name="codigoERP">Código ERP</param>
        /// <returns>HttpResponseMessage con el archivo como contenido </returns>
        [HttpGet]
        [Route("api/MFiles/downloadFile/")]
        public HttpResponseMessage GetDocFirstFile(string codigoERP)
        {
            
            //Descarga los archivos usando el Código ERP
            var archivosDescargados = objConsultarDocs.GetFile(IdPropiedades["codigoERP"], codigoERP);

            //Obtiene el archivo en bytes y la extención
            var file = archivosDescargados.Item1;
            var extension = archivosDescargados.Item2;
            
            //Genera un nombre único para el archivo
            string fileName = $@"{Guid.NewGuid()}." + extension;
            System.Diagnostics.Debug.WriteLine($"\tArchivo descargado: {fileName}");

            //Se crea el archivo y se lo asigna al mensaje de respuesta

            var fileMemStream = new MemoryStream(file);
            
            var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(fileMemStream);

            //se define el header de la respuesta

            var headers = result.Content.Headers;
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.FileName = fileName;
                headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                headers.ContentLength = fileMemStream.Length;

            return result;
        }

        /// <summary>
        /// Actualiza la información del Documento en Mfiles
        /// </summary>
        /// <param name="Documento">Objeto con la información a actualizar del documento</param>

        [HttpPut]
        [Route("api/MFiles/")]
        public String Post([FromBody] MFilesDocument Documento)
        {
           //Devuelve el resultado de la indexación
            return objConsultarDocs.IndexarDocumento(Documento);
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
