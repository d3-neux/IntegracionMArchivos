using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperacionesMFiles
{
    public class DinersResultList
    {
        [JsonProperty(Order = -5)]
        public int numPagActual { get; }
        [JsonProperty(Order = -4)]
        public int numTotalPag { get; }
        [JsonProperty(Order = -3)]
        public int numTotalRegs { get; }

        [JsonProperty(Order = -2)]
        public List<ResultDocument> doc = new List<ResultDocument>();

        public DinersResultList(int numPagActual, int numTotalPag, int numTotalRegs, List<MFilesDocument> mfilesDocuments)
        {
            this.numPagActual = numPagActual;
            this.numTotalPag = numTotalPag;
            this.numTotalRegs = numTotalRegs;
            doc = GetDocumentsFromList(mfilesDocuments);
        }

        public List<ResultDocument> GetDocumentsFromList(List<MFilesDocument> mFilesDocuments)
        {
            List<ResultDocument> resultList = new List<ResultDocument>();

            foreach(MFilesDocument document in mFilesDocuments)
            {
                resultList.Add(new ResultDocument(document.ObjectID, document.DocProperties));
            }

            return resultList;
        }

        public class ResultDocument
        {
            [JsonProperty(Order = -5)]
            public int id { get; }
            [JsonProperty(Order = -4)]
            public readonly int online = 1;

            [JsonProperty(Order = -2)]
            public object indexes;

            public ResultDocument(int id, List<DocumentProperty> properties)
            {
                this.id = id;
                List<object> propertiesFormated = new List<object>();


                String propertiesObject = "";

                //no devuelve resultados si el valor de la propiedad es vacio
                foreach(DocumentProperty property in properties)
                {
                    //if (property.Name == "ID_MFILES" || property.Name == "Clase" || property.Value == "" )
                    if (property.Name == "ID_MFILES" || property.Name == "Clase")
                        continue;

                    propertiesObject += $"'{property.Name}': '{property.Value}',";

                }

                propertiesObject = "{" + propertiesObject.Substring(0, propertiesObject.Length - 1) + "}";

                this.indexes = JsonConvert.DeserializeObject(propertiesObject);
            }
        }

    }

    public class DinersResultDocument
    {
        public List<String> archivos = new List<String>();

        public DinersResultDocument(List<byte[]> archivosMFiles)
        {
            foreach (byte[] file in archivosMFiles)
            {
                archivos.Add(Convert.ToBase64String(file));
                break;
            }
        }
    }
}
