using MFaaP.MFWSClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace OperacionesMFiles
{
    public class IntegracionMFiles
    {
        public static MFWSClient client;
        public static MFWSVaultPropertyDefOperations mfPropertyOperator;



        public static string rutaTemp = Path.Combine(Path.GetTempPath(), "MFilesAPIData");


        /// <summary>
        /// Inicializa el cliente de MFiles y recibe la lista de propiedades;
        /// </summary>
        /// <param name="server">URL del servidor de M-Files</param>
        /// <param name="boveda">GUID de la bóveda de Motransa</param>
        /// <param name="user">Usuario</param>
        /// <param name="pass">Contraseña</param>
        public IntegracionMFiles(String server, String boveda, String user, String pass)
        {
            client = new MFWSClient(server);

            client.AuthenticateUsingCredentials(
                Guid.Parse(boveda),    //id de boveda
                user,                  //usuario
                pass);

            mfPropertyOperator = new MFWSVaultPropertyDefOperations(client);


        }


        public List<MFilesDocument> GetFilesAndMetadata(MFilesSearchDocument searchDocument, Boolean includeFiles)
        {
            List<MFilesDocument> mfilesDocuments = new List<MFilesDocument>();

            //try
            {
                //Se crea la condición de búsqueda
                var results = client.ObjectSearchOperations.SearchForObjectsByConditions(searchDocument.GetMFDocConditions().ToArray());

                if (results.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Busqueda no devolvió resultados");

                    mfilesDocuments.Add( (new MFilesDocument("Busqueda no devolvió resultados") ) ) ;

                    return mfilesDocuments;
                }

                
                //Si hay resultados
                foreach (var objectVersion in results)
                {
                    var files = includeFiles ? GetDocumentFiles(objectVersion) : null;
                    MFilesDocument mfilesDocument = new MFilesDocument(GetDocumentProperties(objectVersion), files);
                    mfilesDocuments.Add(mfilesDocument);
                }
            }
            /*catch (Exception ex)
            {
                errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_GetFile-3", mensaje = ex.ToString() });
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return new Tuple<byte[], string>(null, errorStr);


            }*/
            return mfilesDocuments;
        }

        


        /// <summary>
        /// Devuelve una tupla del archivo en bytes y su extensión
        /// </summary>
        /// <param name="codigoERP">Código ERP del documento consultado</param>
        /// <param name="propertyID">ID de la propiedad de M-Files</param>
        /// <returns>Tupla con el archivo en byte[] y string con la extension</returns>
        public Tuple<byte[], string> GetFile(MFilesSearchDocument document)
        {

            var errorStr = JsonConvert.SerializeObject(new { origen = this.GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + " - 1 "
                , mensaje = $"Búsqueda [{document.ToString()}] no devolvió resultados" });

            Tuple<byte[], string> archivosDescargados = new Tuple<byte[], string>(null, errorStr);
            //try
            {
                //Se crea la condición de búsqueda
                var results = client.ObjectSearchOperations.SearchForObjectsByConditions(document.GetMFDocConditions().ToArray());

                if (results.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine(errorStr);
                    return new Tuple<byte[], string>(null, errorStr);
                }

                //Si hay resultados
                foreach (var objectVersion in results)
                {
                    var folderPath = new System.IO.DirectoryInfo(Path.Combine(rutaTemp));

                    if (false == folderPath.Exists)
                        folderPath.Create();

                    GetDocumentProperties(objectVersion);

                    foreach (var file in objectVersion.Files)
                    {
                        // Generate a unique file name.
                        var fileName = System.IO.Path.Combine(folderPath.FullName, file.ID + "." + file.Extension);

                        // Download the file data.
                        client.ObjectFileOperations.DownloadFile(objectVersion.ObjVer.Type,
                            objectVersion.ObjVer.ID,
                            objectVersion.Files[0].ID,
                            fileName,
                            objectVersion.ObjVer.Version);

                        if (!File.Exists(fileName))
                        {
                            errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_GetFile-2", mensaje = $"Error en la descarga del archivo [{fileName}]" });
                            System.Diagnostics.Debug.WriteLine(errorStr);
                            return new Tuple<byte[], string>(null, errorStr);

                        }

                        System.Diagnostics.Debug.WriteLine($"\tArchivo temporal descargado {fileName}");

                        var archivoBytes = File.ReadAllBytes(fileName);

                        archivosDescargados = Tuple.Create(archivoBytes, file.Extension);

                        File.Delete(fileName);
                        System.Diagnostics.Debug.WriteLine($"\tArchivo temporal borrado {fileName}");
                    }
                }
            }
            /*catch (Exception ex)
            {
                errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_GetFile-3", mensaje = ex.ToString() });
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return new Tuple<byte[], string>(null, errorStr);


            }*/
            return archivosDescargados;
        }


        /// <summary>
        /// Indexa el documento con la información recibida por el web service
        /// </summary>
        /// <param name="documento">Objeto de la clase MFIlesDocument</param>
        /// <returns>Respusta de indexación del documento</returns>
        /*public String IndexarDocumento(MFilesSearchDocument documento)
        {
            var errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-1", mensaje = $"Error desconocido al indexar documentos { DateTime.Now:dd/MM/yyyy HH:mm:ss}" });
            System.Diagnostics.Debug.WriteLine($"\tDocumento Actual: {documento}");

            if (documento == null)
            {
                errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-5", mensaje = "Error al recibir JSON con metadatos" });
                System.Diagnostics.Debug.WriteLine(errorStr);
                return errorStr;
            }

            try
            {
                var DocumentoMfiles = GetDocumentObjVersion(documento.CodigoERP, documento.GetType().ToString());

                if (DocumentoMfiles == null)
                    return JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-2", mensaje = $"Documento no ha sido encontrado en M-Files { DateTime.Now:dd/MM/yyyy HH:mm:ss}" });
                else
                    System.Diagnostics.Debug.WriteLine("DOCUMENTO ENCONTRADO: " + DocumentoMfiles.ObjVer.ID);

                var PropiedadesIndexadas = CrearPropiedades(documento);
                var documentoNuevoMFiles = client.ObjectOperations.CheckOut(DocumentoMfiles.ObjVer);

                //Se actualizan las propiedades
                ExtendedObjectVersion resultado;

                try
                {
                    resultado = client.ObjectPropertyOperations.SetProperties(documentoNuevoMFiles.ObjVer, PropiedadesIndexadas, false, CancellationToken.None);
                    System.Diagnostics.Debug.WriteLine("PROPIEDADES ACTUALIZADAS");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    client.ObjectOperations.UndoCheckout(documentoNuevoMFiles.ObjVer);
                    return JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-3", mensaje = $"Error al actualizar propiedades { DateTime.Now:dd/MM/yyyy HH:mm:ss}" });
                }


                var msgStr = JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-OK", mensaje = $"Documento {documentoNuevoMFiles.ObjVer.ID} indexado exitosamente - { DateTime.Now:dd/MM/yyyy HH:mm:ss}" });
                System.Diagnostics.Debug.WriteLine(msgStr);
                client.ObjectOperations.CheckIn(resultado.ObjVer);
               
                return msgStr;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-4", mensaje = ex.ToString() });
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            return errorStr;
        }
        */
        /// <summary>
        /// Crea array de propiedades del documento a actualizar
        /// </summary>
        /// <param name="documento">Recibe objeto de la clase MFilesDocument</param>
        /// <returns>PropertyValue[] </returns>
        

        /// <summary>
        /// Obtiene un objVersion que coindica con el parámetro de búsqueda
        /// </summary>
        /// <param name="codigoERP">Código ERP del documento</param>
        /// <returns>ObjectVersion del documento encontrado</returns>
        private ObjectVersion GetDocumentObjVersion(MFilesSearchDocument documento)
        {
            //System.Diagnostics.Debug.WriteLine("Condicion!!!" + condition2.ToString());
            var results = client.ObjectSearchOperations.SearchForObjectsByConditions(documento.GetMFDocConditions().ToArray());

            // Iterate over the results and output them. results 
            System.Diagnostics.Debug.WriteLine($"There were {results.Length} results returned.");

            foreach (var objectVersion in results)
            {
                return objectVersion;
            }
            return null;
        }

        //Get object properties from object version
        private List<DocumentProperty> GetDocumentProperties(ObjectVersion objectVersion)
        {
            var properties = client.ObjectPropertyOperations.GetProperties(objectVersion.ObjVer);

            List<DocumentProperty> DocumentProperties = new List<DocumentProperty>();

            foreach (var property in properties)
            {

                if (property.PropertyDef > 999)
                {
                    var propertyName = client.PropertyDefOperations.GetPropertyDef(property.PropertyDef).Name;
                    var propertyID = property.PropertyDef;
                    var propertyValue = property.TypedValue.Value != null ? property.TypedValue.Value.ToString() : "";

                    var newProperty = new DocumentProperty(propertyID, propertyValue, propertyName);

                    DocumentProperties.Add(newProperty);
                    System.Diagnostics.Debug.WriteLine($"{ newProperty }");
                }
            }

            return DocumentProperties;
        }

        //Obtiene object version y devuelve lista de byte[] por cada archivo relacionado
        public List<byte[]> GetDocumentFiles(ObjectVersion objectVersion)
        {
            List<byte[]> base64Files = new List<byte[]>();


            var folderPath = new System.IO.DirectoryInfo(Path.Combine(rutaTemp));

            if (false == folderPath.Exists)
                folderPath.Create();

            foreach (var file in objectVersion.Files)
            {
                // Generate a unique file name.
                var fileName = System.IO.Path.Combine(folderPath.FullName, file.ID + "." + file.Extension);

                // Download the file data.
                client.ObjectFileOperations.DownloadFile(objectVersion.ObjVer.Type,
                    objectVersion.ObjVer.ID,
                    objectVersion.Files[0].ID,
                    fileName,
                    objectVersion.ObjVer.Version);

                if (!File.Exists(fileName))
                {
                    var errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_GetFile-2", mensaje = $"Error en la descarga del archivo [{fileName}]", ID = $"{objectVersion.ObjVer.ID}" });
                    base64Files.Add(Encoding.Unicode.GetBytes(errorStr));
                    continue;

                }

                System.Diagnostics.Debug.WriteLine($"\tArchivo temporal descargado {fileName}");

                var archivoBytes = File.ReadAllBytes(fileName);

                base64Files.Add(archivoBytes);

                File.Delete(fileName);
                System.Diagnostics.Debug.WriteLine($"\tArchivo temporal borrado {fileName}");

            }

            return base64Files;
        }
    }
}
