using MFaaP.MFWSClient;
using Newtonsoft.Json;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OperacionesMFiles
{
    public class IntegracionMFiles
    {
        private static MFWSClient client;

        public static string rutaTemp = Path.Combine(Path.GetTempPath(), "MFilesAPIData");
        private static Dictionary<string, int> IdPropiedades;

        /// <summary>
        /// Inicializa el cliente de MFiles y recibe la lista de propiedades;
        /// </summary>
        /// <param name="server">URL del servidor de M-Files</param>
        /// <param name="boveda">GUID de la bóveda de Motransa</param>
        /// <param name="user">Usuario</param>
        /// <param name="pass">Contraseña</param>
        /// <param name="IdPropiedades">Diccionario con nombres y id de propiedades</param>
        public IntegracionMFiles(String server, String boveda, String user, String pass, Dictionary<string, int> IdPropiedades)
        {
            client = new MFWSClient(server);
            
            client.AuthenticateUsingCredentials(
                 Guid.Parse(boveda),    //id de boveda
                     user,                  //usuario
                     pass);

            IntegracionMFiles.IdPropiedades = IdPropiedades;
        }

        /// <summary>
        /// Devuelve una tupla del archivo en bytes y su extensión
        /// </summary>
        /// <param name="codigoERP">Código ERP del documento consultado</param>
        /// <param name="propertyID">ID de la propiedad de M-Files</param>
        /// <returns>Tupla con el archivo en byte[] y string con la extension</returns>
        public Tuple<byte[], string> GetFile(int propertyID, String codigoERP)
        {

            var errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_GetFile-1", mensaje = $"Búsqueda de CodigoERP [{codigoERP}] no devolvió resultados" });

            Tuple<byte[], string> archivosDescargados = new Tuple<byte[], string>(null, errorStr);

            try
            {
                //Se crea la condición de búsqueda
                var condition = new TextPropertyValueSearchCondition(propertyID, codigoERP);
                var results = client.ObjectSearchOperations.SearchForObjectsByConditions(condition);

                if (results.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine(errorStr);
                    return new Tuple<byte[], string>(null, errorStr);
                }

                foreach (var objectVersion in results)
                {

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

                        if(!File.Exists(fileName))
                        {
                            errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_GetFile-2", mensaje = $"Error en la descarga del archivo [{fileName}]" });
                            System.Diagnostics.Debug.WriteLine(errorStr);
                            return new Tuple<byte[], string>(null, errorStr);

                        }

                        System.Diagnostics.Debug.WriteLine($"\tArchivo temporal descargado {fileName}");

                        var archivoBytes = File.ReadAllBytes(fileName);

                        archivosDescargados = Tuple.Create(archivoBytes , file.Extension);

                        File.Delete(fileName);
                        System.Diagnostics.Debug.WriteLine($"\tArchivo temporal borrado {fileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_GetFile-3", mensaje = ex.ToString() });
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return new Tuple<byte[], string>(null, errorStr);


            }
            return archivosDescargados;
        }


        /// <summary>
        /// Indexa el documento con la información recibida por el web service
        /// </summary>
        /// <param name="documento">Objeto de la clase MFIlesDocument</param>
        /// <returns>Respusta de indexación del documento</returns>
        public String IndexarDocumento(MFilesDocument documento)
        {
            var errorStr = JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-1", mensaje = $"Error desconocido al indexar documentos { DateTime.Now:dd/MM/yyyy HH:mm:ss}" });
            System.Diagnostics.Debug.WriteLine($"\tDocumento Actual: {documento}");

            try
            {
                var DocumentoMfiles = GetDocumentObjVersion(documento.CodigoERP);

                if (DocumentoMfiles == null)
                    return JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-2", mensaje = $"Error al obtener ObjVersion { DateTime.Now:dd/MM/yyyy HH:mm:ss}" });

                var PropiedadesIndexadas = CrearPropiedades(documento);


                var obj = client.ObjectOperations.CheckOut(DocumentoMfiles.ObjVer);

                //Se actualizan las propiedades
                ExtendedObjectVersion resultado;

                try
                {
                    resultado = client.ObjectPropertyOperations.SetProperties(obj.ObjVer, PropiedadesIndexadas, false, CancellationToken.None);
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    client.ObjectOperations.UndoCheckout(obj.ObjVer);
                    return JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-3", mensaje = $"Error al actualizar propiedades { DateTime.Now:dd/MM/yyyy HH:mm:ss}" });
                }


                var msgStr = JsonConvert.SerializeObject(new { codigo = "IMF_IndexarDocumento-OK", mensaje = $"Documento {resultado.ObjVer.ID} indexado exitosamente - { DateTime.Now:dd/MM/yyyy HH:mm:ss}" }); 
                
                client.ObjectOperations.CheckIn(obj.ObjVer);
                System.Diagnostics.Debug.WriteLine(msgStr);
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

        /// <summary>
        /// Crea array de propiedades del documento a actualizar
        /// </summary>
        /// <param name="documento">Recibe objeto de la clase MFilesDocument</param>
        /// <returns>PropertyValue[] </returns>
        private PropertyValue[] CrearPropiedades(Object documento)
        {
            System.Diagnostics.Debug.WriteLine ( documento.GetType() );

            Documento newDocumento;
            Retencion newRetencion;
            Factura newFactura;

            List<(string, string, string)> listaPropiedadesClase;
            
            List<PropertyValue> listaPropiedadesMFiles = new List<PropertyValue>();


            if (documento.GetType().ToString() == "OperacionesMFiles.Documento")
            {
                newDocumento = (Documento)documento;
                listaPropiedadesClase = newDocumento.GetMFilesProperties();
            }
            else if (documento.GetType().ToString() == "OperacionesMFiles.Retencion")
            {
                newRetencion = (Retencion)documento;
                listaPropiedadesClase = newRetencion.GetMFilesProperties();
            }
            else if (documento.GetType().ToString() == "OperacionesMFiles.Factura")
            {
                newFactura = (Factura)documento;
                listaPropiedadesClase = newFactura.GetMFilesProperties();
            }
            else
                return null;

            foreach (var item in listaPropiedadesClase)
            {
                var mFilesPropID = IdPropiedades[item.Item1];
                var valor = item.Item2;
                var tipo = item.Item3;
                

                if (valor == null)
                    continue;

                MFDataType mFDataType = MFDataType.Text;

                if (tipo == "Text")
                    mFDataType = MFDataType.Text;
                else if (tipo == "Date")
                    mFDataType = MFDataType.Date;
                else if (tipo == "Floating")
                {
                    mFDataType = MFDataType.Floating;
                    valor = valor.Replace(',', '.');
                }


                PropertyValue nuevaPropiedad = new PropertyValue
                {
                    PropertyDef = mFilesPropID,
                    TypedValue = new TypedValue { DataType = mFDataType, Value = valor }
                };

                listaPropiedadesMFiles.Add(nuevaPropiedad);
            }

            return listaPropiedadesMFiles.ToArray();
        }

        /// <summary>
        /// Obtiene un objVersion que coindica con el parámetro de búsqueda
        /// </summary>
        /// <param name="codigoERP">Código ERP del documento</param>
        /// <returns>ObjectVersion del documento encontrado</returns>
        private ObjectVersion GetDocumentObjVersion(string codigoERP)
        {
            var condition = new TextPropertyValueSearchCondition(IdPropiedades["CodigoERP"], codigoERP);
            //var condition2 = new TextPropertyValueSearchCondition(IdPropiedades["CodigoERP"], codigoERP);

            var results = client.ObjectSearchOperations.SearchForObjectsByConditions(condition);

            // Iterate over the results and output them. results 
            System.Diagnostics.Debug.WriteLine($"There were {results.Length} results returned.");

            foreach (var objectVersion in results)
            {
                return objectVersion;
            }

            return null;
        }
    }
}
