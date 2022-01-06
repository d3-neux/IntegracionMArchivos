using MFaaP.MFWSClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace OperacionesMFiles
{
    public class IntegracionMFiles
    {
        public static MFWSClient client;
        public static MFWSVaultPropertyDefOperations mfPropertyOperator;

        public static string rutaTemp = Path.Combine(Path.GetTempPath(), "MFilesAPIData");
        public Dictionary<int, PropertyDef> valutProperties;

        public static string dbConnection;


        /// <summary>
        /// Inicializa el cliente de MFiles y recibe la lista de propiedades;
        /// </summary>
        /// <param name="server">URL del servidor de M-Files</param>
        /// <param name="boveda">GUID de la bóveda de Motransa</param>
        /// <param name="user">Usuario</param>
        /// <param name="pass">Contraseña</param>
        public IntegracionMFiles(String server, String boveda, String user, String pass, String dbConnection_)
        {

            client = new MFWSClient(server);

            client.AuthenticateUsingCredentials(
                Guid.Parse(boveda),    //id de boveda
                user,                  //usuario
                pass);

            mfPropertyOperator = new MFWSVaultPropertyDefOperations(client);


            valutProperties = client.PropertyDefOperations.GetPropertyDefs().ToDictionary(pd => pd.ID, pd => pd);

            /*
            valutProperties.Select(i => $"{i.Key}: {i.Value.Name}").ToList().ForEach( element => {
                System.Diagnostics.Debug.WriteLine(element);
            });
            */

            dbConnection = dbConnection_;

        }


        public Object GetDinersDocumentsRedo(DinersSearchDocument searchDocument, Boolean includeFiles)
        {
            List<MFilesDocument> mfilesDocuments;

            if (searchDocument.parameter.Count() == 0)
            {
                return new ErrorClass("03", "El array parameters se encuentra vacío");

            }
            else if (searchDocument.idtrace == null)
            {
                return new ErrorClass("04", "El campo idTrace es obligatorio para la consulta");
            }
            else if (searchDocument.operation == "DOC_HIT_LIST")
            {

                //BUSQUEDA DIRECTA A MFILES
                //mfilesDocuments = GetFilesAndMetadata(searchDocument, false).Distinct().ToList();


                mfilesDocuments = DataAccess.GetRecords(searchDocument.GetSQLConditions()).Distinct().ToList();

                mfilesDocuments = mfilesDocuments.OrderByDescending(i => i.DocProperties.Find(x => x.Name.ToUpper() == "FECHA_CORTE").Value).ToList();


                if (mfilesDocuments.Count() == 0)
                {
                    return new ErrorClass("11", "La búsqueda no devolvió resultados");
                }

                var numPagActual = searchDocument.numPagActual;
                var cantRegistros = searchDocument.cantRegistros;
                var numTotalPag = 0;


                if (cantRegistros != 0)
                    numTotalPag = (int) Math.Ceiling(Double.Parse(mfilesDocuments.Count() + "") / Double.Parse(cantRegistros + ""));


                if (numTotalPag == 1 && numPagActual > 1)
                    numPagActual = 1;

                var numTotalRegs = -1;

                var rangoInicial = cantRegistros * (numPagActual - 1) + 1;

                if (rangoInicial > mfilesDocuments.Count())
                    rangoInicial = mfilesDocuments.Count() + 1;

                if (numPagActual == 1)
                    rangoInicial = 1;

                if (numPagActual < 1 || cantRegistros == 0)
                    rangoInicial = 0;

                var rangoFinal = cantRegistros * numPagActual;

                if (rangoFinal > mfilesDocuments.Count())
                    rangoFinal = mfilesDocuments.Count();

                numTotalRegs = rangoFinal - rangoInicial;

                if (numTotalRegs != 0)
                    numTotalRegs++;

                if (numTotalRegs == 0 && mfilesDocuments.Count() != 0)
                    numTotalRegs = mfilesDocuments.Count();

                if (numPagActual == numTotalPag && numPagActual != 0)
                    numTotalRegs = rangoFinal - rangoInicial + 1;

                if (numPagActual == 0 && numTotalPag == 0)
                    numTotalRegs = mfilesDocuments.Count();

                if (rangoInicial != 0 && rangoInicial == rangoFinal && rangoFinal <= mfilesDocuments.Count())
                    numTotalRegs = 1;

                if (rangoInicial > 0 && rangoInicial <= rangoFinal && rangoFinal <= mfilesDocuments.Count())
                {
                    mfilesDocuments = mfilesDocuments.GetRange(rangoInicial - 1, numTotalRegs);
                }

                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:ffffff")} Calculado");

                DinersResultList dinersResultList = new DinersResultList(numPagActual, numTotalPag, numTotalRegs, mfilesDocuments);

                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:ffffff")} JSON");


                return dinersResultList;

            }
            else if (searchDocument.operation == "PDF" || searchDocument.operation == "XML" || searchDocument.operation == "XLS")
            {
                mfilesDocuments = GetFilesAndMetadata(searchDocument, true);
                mfilesDocuments = mfilesDocuments.OrderByDescending(i => i.DocProperties.Find(x => x.Name.ToUpper() == "FECHA_CORTE").Value).ToList();

                if (mfilesDocuments.Count() == 0)
                {
                    return new ErrorClass("11", "La búsqueda no devolvió resultados");
                }

                return new DinersResultDocument(mfilesDocuments.ElementAt(0).Files);

            }
            else
            {
                return new ErrorClass("02", "El operador consultado no es válido");
            }
        }

        
        public List<MFilesDocument> GetFilesAndMetadata(MFilesSearchDocument searchDocument, Boolean includeFiles)
        {
            List<MFilesDocument> mfilesDocuments = new List<MFilesDocument>();

            //Si no hay condiciones
            if (searchDocument.GetMFDocConditions().Count() == 0)
            {
                System.Diagnostics.Debug.WriteLine("Busqueda no devolvió resultados");
                //mfilesDocuments.Add(new MFilesDocument("Busqueda no devolvió resultados"));
                return mfilesDocuments;
            }

            try
            {
                //Se crea la condición de búsqueda
                var results = client.ObjectSearchOperations.SearchForObjectsByConditions(0, searchDocument.GetMFDocConditions().ToArray());



                //System.Diagnostics.Debug.WriteLine("Conditions: " + string.Join("\n",JsonConvert.SerializeObject(searchDocument.GetMFDocConditions()).Split(new[] { "}," }, StringSplitOptions.None)));

                System.Diagnostics.Debug.WriteLine($"Search results {results.Length}");

                if (results.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Busqueda no devolvió resultados");
                    //mfilesDocuments.Add((new MFilesDocument("Busqueda no devolvió resultados")));
                    return mfilesDocuments;
                }
                else if (includeFiles)
                {
                    System.Diagnostics.Debug.WriteLine("Solo buscar primer resultado!!!");

                    results = results.Take(1).ToArray();
                }

                //se obtienen las propiedades de todos los documentos
                mfilesDocuments = GetDocumentsAndProperties(results, includeFiles);


            }
            catch (Exception ex)
            {
                return mfilesDocuments;
            }

            return mfilesDocuments;
        }


        
        private List<MFilesDocument> GetDocumentsAndProperties(ObjectVersion [] objectVersionList, Boolean includeFiles) {

            List<MFilesDocument> mFilesDocuments = new List<MFilesDocument>();

            var start = 0;
            const int blockSize = 300;
            var objectsAndProperties = new List<Tuple<ObjectVersion, PropertyValue[]>>();
            do
            {
                var objects = objectVersionList.Skip(start).Take(blockSize).ToList();
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:ffffff")} CONSULTANDO PROPIEDADES {start} - {objects.Count} / {objectVersionList.Length}");
                if (false == objects.Any())
                    break;

                var properties = client.ObjectPropertyOperations.GetPropertiesOfMultipleObjects(objects.Select(ov => ov.ObjVer).ToArray());

                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:ffffff")} PROPIEDADES CONSULTADAS");
                for (var i = 0; i < objects.Count; i++)
                {
                    var property = properties[i].ToList();
                
                    List<DocumentProperty> DocumentProperties = new List<DocumentProperty>();

                    for (var j = 0; j < property.Count; j++)
                    {
                        var propID = property[j].PropertyDef;
                        var propValue = property[j].TypedValue.Value != null ? property[j].TypedValue.Value.ToString() : "";
                        var propName = valutProperties[propID].Name;
                        var displayValue = property[j].TypedValue.DisplayValue;

                        //excluye tipo archivo
                        if (propID > 1020 || propID == 100)
                        {
                           if (propID == 100)
                            {
                                propValue = displayValue;
                            }

                            var newProperty = new DocumentProperty(propID, propValue, propName);
                            DocumentProperties.Add(newProperty);
                        }
                    }

                    var files = includeFiles ? GetDocumentFiles(objects[i]) : null;
                    mFilesDocuments.Add(new MFilesDocument(DocumentProperties, files, objects[i].ObjVer.ID));

                }
                System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:ffffff")} PROPIEDADES AGREGADAS A RESPONSE");
                start += objects.Count;
            } while (true);

            System.Diagnostics.Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss:ffffff")} FINALIZACIÓN DE BÚSQUEDA");
            return mFilesDocuments;
        }



        
        
        //Devuelve todos los documentos relacionados al objectVersion recibido
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
                    var errorStr = JsonConvert.SerializeObject(new ErrorClass("13", $"Error de descarga de archivo temporal  [{fileName}], ID [{ objectVersion.ObjVer.ID}]"));
                    
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
