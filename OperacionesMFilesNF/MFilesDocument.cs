using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using MFaaP.MFWSClient;
using Newtonsoft.Json;

namespace OperacionesMFiles
{
    

    //Root myDeserializedClass = JsonConvert.DeserializeObject<MFilesSearchDocument>(myJsonResponse); 
    public class DocumentProperty
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Name{ get; set; }

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
        public List<DocumentProperty> DocProperties{ get; set; }

        [JsonProperty("Files", NullValueHandling = NullValueHandling.Ignore)]
        public List<byte[]> Files { get; set; }

        public MFilesDocument(List<DocumentProperty> DocProperties, List<byte[]> Files)
        {
            this.DocProperties = DocProperties;
            this.Files = Files;
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
    }
}


/*
    id: "1147", // numero de documento
    id: "1002", // fecha de documento
    id: "1148", // ruc
    id: "1149", // total
    id: "39", 	// estado 

     
    {
	class: "2",
	properties: [
		{
			id: "1147",
			value: "",
			type: "text",
			condition: "equal"
		},
		{
			id: "1002", 
			value: "",
			type: "date",
			condition: "equal"
		},
		{
			id: "1148",
			value: "",
			type: "text",
			condition: "equal"
		},
		{
			id: "1149",
			value: "12.25",
			type: "floating",
			condition: "equal"
		}
	]
}



     */
