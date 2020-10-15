using MFaaP.MFWSClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace OperacionesMFiles
{
    public class ConsultarDocumentos
    {
        private static MFWSClient client;

        private static string server = "http://127.0.0.1:80/M-Files/";
        private static string boveda = "{799F0633-EBFD-4597-9007-D8EFCE35E200}";
        private static string user = "administrador";
        private static string pass = "mf";

        private static int propertyID = 1077;
        private static string propertyvalue = "TEST";

        private static string rutaTemp = Path.Combine(Path.GetTempPath(), "mfilesData");

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



        

        public List<Tuple<byte[], string>> GetFiles()
        {
            List< Tuple<byte[], string> > archivosDescargados = new List<Tuple<byte[], string>>();

            try
            {
                var client = new MFWSClient(server);
                //Conectar a bóveda
                client.AuthenticateUsingCredentials(
                     Guid.Parse(boveda),    //id de boveda
                         user,                  //usuario
                         pass);

                var condition = new TextPropertyValueSearchCondition(propertyID, propertyvalue);

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
                        //documentos.Add(fileName);
                        
                        //File.Delete(fileName);

                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return archivosDescargados;
        }
    }
}
