using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OperacionesMFiles
{

    //Root myDeserializedClass = JsonConvert.DeserializeObject<MFilesSearchDocument>(myJsonResponse); 
    public class DocumentProperty
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }

        public DocumentProperty(int id, string value, string name)
        {
            Id = id;
            Value = value;
            Name = name;
        }

        public override string ToString()
        {
            return $"ID: {Id}, Value: {Value}, Name: {Name}";
        }
    }

    //esta clase representa un documento de m-files con sus propiedades > 1000 y todos los archivos del objecto
    public class MFilesDocument
    {
        public int ObjectID { get; set; }
        public List<DocumentProperty> DocProperties { get; set; }

        [JsonProperty("Files", NullValueHandling = NullValueHandling.Ignore)]
        public List<byte[]> Files { get; set; }



        //SOLO DINERS ---- quitar comentario para proceso de DINERS
        public override bool Equals(object obj)
        {
            MFilesDocument newObj = (obj as MFilesDocument);

            if (newObj.DocProperties.Exists(x => x.Name == "ID_MFILES") && (this.DocProperties.Exists(x => x.Name == "ID_MFILES")))
            {
                var newObjID = newObj.DocProperties.Find(x => x.Name == "ID_MFILES").Value;
                var documentID = DocProperties.Find(x => x.Name == "ID_MFILES").Value;
                System.Diagnostics.Debug.WriteLine($"Documentos tienen ID_MFILES {newObjID} - {documentID}");
                return newObjID == documentID;
            }
            System.Diagnostics.Debug.WriteLine($"Documentos no tienen ID_MFILES {newObj.GetHashCode()} - {this.GetHashCode()}");
            return newObj.GetHashCode() == this.GetHashCode();
        }

        public override int GetHashCode()
        {
            if (DocProperties.Exists(x => x.Name == "ID_MFILES"))
            {
                return DocProperties.Find(x => x.Name == "ID_MFILES").Value.GetHashCode();
            }
            return ObjectID;
        }
        
        //END SOLO DINERS



        /// <param name="DocProperties"></param>
        /// <param name="Files"></param>
        /// <param name="objectID"></param>

        public MFilesDocument(List<DocumentProperty> DocProperties, List<byte[]> Files, int objectID)
        {
            this.DocProperties = DocProperties;
            this.Files = Files;
            this.ObjectID = objectID;
        }

        public MFilesDocument(string errMsg)
        {
            List<DocumentProperty> DocProperties = new List<DocumentProperty>();
            DocProperties.Add(new DocumentProperty(0, errMsg, ""));
            this.DocProperties = DocProperties;
        }

        public override string ToString()
        {
            return $"Properties: {string.Join(",", DocProperties)}";
        }


        public string GetDinersPropertiesString()
        {
            string str = "";
            str = $"id:'{ObjectID}',";
            str += $"online:'1',";
            str += "indexes:{";

            foreach (DocumentProperty property in this.DocProperties)
            {
                str += $"{property.Name}:'{property.Value}',";
            }
            str = str.Substring(0, str.Length - 1) + "}";

            return str;
        }

        public string GetDinersFilesString()
        {
            string str = "archivo:[";
            foreach (byte[] file in Files)
            {
                str += $"\"{Convert.ToBase64String(file)}\",";
                break;
            }
            str = str.Substring(0, str.Length - 1) + "]";
            return str;
        }
    }
}

