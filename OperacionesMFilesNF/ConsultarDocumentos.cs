using MFaaP.MFWSClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OperacionesMFiles
{
    public class ConsultarDocumentos
    {
        private static MFWSClient client;

        public static string rutaTemp = Path.Combine(Path.GetTempPath(), "mfilesData");
        private static Dictionary<string, int> IdPropiedades;

        //private static List<String> documentos = new List<string>();


        /*public static void Main(string[] args)
        {
            ConsultarDocumentos obj = new ConsultarDocumentos();

            var archivosDescargados = obj.GetFiles();


            if (archivosDescargados == null)
                return;

            int i = 0;

            foreach (Tuple<byte[], string> item in archivosDescargados)
            {
                i++;
                var file = item.Item1;
                var extension = item.Item2;

                string nuevaRuta = Path.Combine(rutaTemp, $"newFile{i}." + extension);

                System.Diagnostics.Debug.WriteLine($"\tFile: {nuevaRuta}");
                File.WriteAllBytes( nuevaRuta, file);
            }

        }*/


        public ConsultarDocumentos(String server, String boveda, String user, String pass, Dictionary<string, int> IdPropiedades)
        {
            client = new MFWSClient(server);
            //Conectar a bóveda

            client.AuthenticateUsingCredentials(
                 Guid.Parse(boveda),    //id de boveda
                     user,                  //usuario
                     pass);

            ConsultarDocumentos.IdPropiedades = IdPropiedades;



        }

        /// <summary>
        /// Devuelve una lista de tuplas con el archivo en bytes y su extensión
        /// </summary>
        /// <param name="erpID">Código ERP del documento consultado</param>
        /// <param name="propertyID">ID de la propiedad de M-Files</param>
        /// <returns></returns>
        public List<Tuple<byte[], string>> GetFiles(int propertyID, String erpID)
        {
            List< Tuple<byte[], string> > archivosDescargados = new List<Tuple<byte[], string>>();

            try
            {
                var condition = new TextPropertyValueSearchCondition(propertyID, erpID);

                var results = client.ObjectSearchOperations.SearchForObjectsByConditions(condition);

                if (results.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No hay resultados");
                    return null;
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
                            System.Diagnostics.Debug.WriteLine("No encontrado");

                        }

                        System.Diagnostics.Debug.WriteLine($"\t\tFile: {file.Name} output to {fileName}");

                        var archivoBytes = File.ReadAllBytes(fileName);

                        archivosDescargados.Add(Tuple.Create(archivoBytes , file.Extension));

                        File.Delete(fileName);

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return archivosDescargados;
        }


        /// <summary>
        /// Actualiza los metadatos del documento que coincida con el Código ERP
        /// </summary>
        /// <param name="erpID">Código ERP del documento a actualizar</param>
        /// <param name="empresa">Nombre de Empresa</param>
        /// <param name="numDocumento">Número de documento</param>
        /// <param name="numFacturaRetenida">Número de factura retenida</param>
        /// <param name="rucEmisor">Ruc Emisor del documento</param>
        /// <param name="fechaEmision">Fecha de emisión</param>
        /// <param name="valor">Valor</param>
        /// <returns></returns>
        
        public String IndexarDocumento(string codigoERP, string empresa, string numDocumento, string numFacturaRetenida, string rucEmisor, string fechaEmision, string valor)
        {
            System.Diagnostics.Debug.WriteLine($"\tFile: {empresa}");


            var DocumentoMfiles = GetDocumentObjVersion(codigoERP);

            if (DocumentoMfiles == null)
                return null;
            
            var PropiedadesIndexadas = CrearPropiedades(empresa, numDocumento, numFacturaRetenida, rucEmisor, fechaEmision, valor);

            var resultado = client.ObjectPropertyOperations.SetProperties(DocumentoMfiles.ObjVer, PropiedadesIndexadas, false, CancellationToken.None);

            if (resultado == null)
                return null;
            
            System.Diagnostics.Debug.WriteLine("RESULTADO: " + resultado.ObjVer.ID);

            return resultado.ObjVer.ID.ToString();
        }

        private PropertyValue[] CrearPropiedades(string empresa, string numDocumento, string numFacturaRetenida, string rucEmisor, string fechaEmision, string valor)
        {

            //Se crean las propiedades (Metadatos)
            return new[]
                {
                    new PropertyValue //empresa
                    {
                        PropertyDef = IdPropiedades["empresa"],
                        TypedValue = new TypedValue { DataType = MFDataType.Text, Value = empresa}
                    },

                    new PropertyValue //numDocumento
                    {
                        PropertyDef = IdPropiedades["numDocumento"],
                        TypedValue = new TypedValue { DataType = MFDataType.Text, Value = numDocumento}
                    },

                    new PropertyValue //numFacturaRetenida
                    {
                        PropertyDef = IdPropiedades["numFacturaRetenida"],
                        TypedValue = new TypedValue { DataType = MFDataType.Text, Value = numFacturaRetenida}
                    },

                    new PropertyValue //rucEmisor
                    {
                        PropertyDef = IdPropiedades["rucEmisor"],
                        TypedValue = new TypedValue { DataType = MFDataType.Text, Value = rucEmisor}
                    },

                    new PropertyValue //fechaEmision
                    {
                        PropertyDef = IdPropiedades["fechaEmision"],
                        TypedValue = new TypedValue { DataType = MFDataType.Date, Value = fechaEmision}
                    },

                    new PropertyValue //valor
                    {
                        PropertyDef = IdPropiedades["valor"],
                        TypedValue = new TypedValue { DataType = MFDataType.Floating, Value = valor}
                    },


                };
        }

        private ObjectVersion GetDocumentObjVersion(string codigoERP)
        {
            var condition = new TextPropertyValueSearchCondition(IdPropiedades["codigoERP"], codigoERP);
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
