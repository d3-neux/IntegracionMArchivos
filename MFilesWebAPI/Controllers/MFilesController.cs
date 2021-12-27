using OperacionesMFiles;
using System;
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
        private static readonly string server = WebConfigurationManager.AppSettings["MFILES_SERVER"].ToString();
        private static readonly string boveda = WebConfigurationManager.AppSettings["MFILES_VAULT"].ToString();
        private static readonly string user = WebConfigurationManager.AppSettings["MFILES_USER"].ToString();
        private static readonly string pass = WebConfigurationManager.AppSettings["MFILES_PASS"].ToString();
        private static readonly string dbConnection = WebConfigurationManager.AppSettings["DB_CONNECTION"].ToString();

        private static readonly IntegracionMFiles objIntegracionMFiles = new IntegracionMFiles(server, boveda, user, pass, dbConnection);

        /// <summary>
        /// Obtiene un objeto que representa los resultados de una búsqueda
        /// </summary>
        /// <param name="documento">Parámetros de búsqueda según </param>
        /// <returns>Una lista de tuplas con los bytes y extensión de cada archivo asociado</returns>

        
        /*[HttpGet]
        [Route("api/MFiles/GetDinersDocuments/")]
        public Object GetDinersDocuments(DinersSearchDocument documento)
        {
            try
            {
                documento.initialize();
                System.Diagnostics.Debug.WriteLine("JSON: " + JsonConvert.SerializeObject(documento));
                var documents = objIntegracionMFiles.GetDinersDocumentsRedo(documento, true);
                return documents;
            }
            catch (Exception e)
            {
                return new OperacionesMFiles.ErrorClass("12", "JSON de request no es válido");
            }
        }*/

        
        [HttpPost]
        [Route("api/MFiles/GetPostDinersDocuments/")]
        public Object GetPostDinersDocuments(DinersSearchDocument documento)
        {
            try 
            { 
                documento.initialize();
                System.Diagnostics.Debug.WriteLine("JSON: " + JsonConvert.SerializeObject(documento));
                var documents = objIntegracionMFiles.GetDinersDocumentsRedo(documento, true);

                logger.Info("GetPostDinersDocuments / Request BODY: " + JsonConvert.SerializeObject(documento));

                return documents;
            }
            catch (Exception e)
            {
                return new OperacionesMFiles.ErrorClass("12", "JSON de request no es válido");
            }
        }
        

        /// <summary>
        /// Obtiene tupla de bytes (archivo) y extensión del documento relacionado al Código ERP
        /// </summary>
        /// <param name="documento">Código ERP</param>
        /// <returns>Una lista de tuplas con los bytes y extensión de cada archivo asociado</returns>

        //// API/MFiles/{ID}
        //[HttpGet]
        //[Route("api/MFiles/")]
        //public Tuple<byte[], string> Get(MFilesSearchDocument documento)
        //{
        //    System.Diagnostics.Debug.WriteLine("JSON: " + JsonConvert.SerializeObject(documento));

        //    return objIntegracionMFiles.GetFile(documento); ;
        //}
        
            /*
        [HttpPost]
        [Route("api/MFiles/GetFilesAndMetadata/")]
        public List<MFilesDocument> GetFilesAndMetadata(MFilesSearchDocument documento)
        {
            System.Diagnostics.Debug.WriteLine("JSON: " + JsonConvert.SerializeObject(documento));
            var resultado = objIntegracionMFiles.GetFilesAndMetadata(documento, true);
            logger.Info($"GetFilesAndMetadata: Request BODY: {JsonConvert.SerializeObject(documento)} Results count: {resultado.Count}");
            return resultado;
        }

        [HttpPost]
        [Route("api/MFiles/GetOnlyMetadata/")]
        public List<MFilesDocument> GetOnlyMetadata(MFilesSearchDocument documento)
        {
            //Devuelve el objeto como respuesta
            System.Diagnostics.Debug.WriteLine("JSON: " + JsonConvert.SerializeObject(documento));
            var resultado = objIntegracionMFiles.GetFilesAndMetadata(documento, false);
            logger.Info($"GetOnlyMetadata: Request BODY: {JsonConvert.SerializeObject(documento)} Results count: {resultado.Count}");
            return resultado;

        }
        */

        /// <summary>
        ///  Descarga el archivo relacionado al Código ERP
        /// </summary>
        /// <param name="documento">Código ERP</param>
        /// <returns>HttpResponseMessage con el archivo como contenido </returns>
        //[HttpGet]
        //[Route("api/MFiles/DownloadFile/")]
        //public HttpResponseMessage GetDocFirstFile(MFilesSearchDocument documento)
        //{
        //    System.Diagnostics.Debug.WriteLine("JSON: " + JsonConvert.SerializeObject(documento));
        //    //MFilesSearchDocument documento = JsonConvert.DeserializeObject<MFilesSearchDocument>(documentoJSON);
        //    //Descarga los archivos usando el Código ERP
        //    var archivosDescargados = objIntegracionMFiles.GetFile(documento);

        //    //Obtiene el archivo en bytes y la extención, si es null el mensaje de error es extraído desde Item2
        //    var file = archivosDescargados.Item1;
        //    var extension = archivosDescargados.Item2;

        //    HttpResponseMessage result;

        //    if (file == null)
        //    {
        //        result = new HttpResponseMessage(HttpStatusCode.NotFound);
        //        result.Content = new StringContent(extension);
        //    }
        //    else
        //    {
        //        //Genera un nombre único para el archivo
        //        string fileName = $@"{Guid.NewGuid()}." + extension;
        //        System.Diagnostics.Debug.WriteLine($"\tArchivo descargado: {fileName}");


        //        //Se crea el archivo y se lo asigna al mensaje de respuesta
        //        var fileMemStream = new MemoryStream(file);

        //        result = new HttpResponseMessage(HttpStatusCode.OK);
        //        result.Content = new StreamContent(fileMemStream);

        //        //se define el header de la respuesta

        //        var headers = result.Content.Headers;
        //        headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //        headers.ContentDisposition.FileName = fileName;
        //        headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        //        headers.ContentLength = fileMemStream.Length;
        //    }
        //    return result;
        //}

        /// <summary>
        /// Actualiza la información de un Documento en M-Files que coincida con el Código ERP
        /// </summary>
        /// <param name="Documento">Objeto con la información a actualizar del documento</param>
        /// <returns>Cadena con el resultado de la indexación</returns>
        /*[HttpPut]
        [Route("api/MFiles/IndexarDocumento")]
        public String Put([FromBody] MFilesSearchDocument Documento)
        {
            String resultado = objIntegracionMFiles.IndexarDocumento(Documento);
            logger.Info("IndexarDocumento: " + resultado + " -- Request BODY: " + JsonConvert.SerializeObject(Documento));
            return resultado;
        }*/


    }
}
