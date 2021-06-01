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
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Parameter
    {
        public string field { get; set; }
        public string @operator { get; set; }

        public string value { get; set; }
        public string value2 { get; set; }
    }

    public class DinersSearchDocument: MFilesSearchDocument
    {
        public string context { get; set; }
        public string operation { get; set; }
        public string numPagActual { get; set; }
        public string cantRegistros { get; set; }
        public string idtrace { get; set; }


        public List<Parameter> parameter = new List<Parameter>();


        public void initialize()
        {
            DocumenPropertyConditions = new List<DocumenPropertyCondition>();
            DocumenPropertyConditions.Add(new DocumenPropertyCondition(100, "", context, "lookup", "Equals"));

            if (operation != "DOC_HIT_LIST")
            {
                DocumenPropertyConditions.Add(new DocumenPropertyCondition(0, "PD.TipoArchivo", operation.ToUpper(), "text", "Equals"));

            }

            


            foreach (Parameter par in parameter)
            {
                var condition1 = "";
                var condition2 = "";

                switch (par.@operator)
                {
                    case "":
                    case "1":
                        condition1 = "Equals";
                        break;
                    //not equal
                    case "2":
                        condition1 = "";
                        break;
                    case "4":
                        condition1 = "LessThan";
                        break;
                    case "8":
                        condition1 = "LessThanOrEqual";
                        break;
                    case "16":
                        condition1 = "GreaterThan";
                        break;
                    case "32":
                        condition1 = "GreaterThanOrEqual";
                        break;
                    //in
                    case "64":
                        condition1 = "";
                        break;
                    //NOT IN
                    case "128":
                        condition1 = "";
                        break;
                    //Like
                    case "256":
                        condition1 = "";
                        break;
                    //not like
                    case "512":
                        condition1 = "";
                        break;
                    //between
                    case "1024":
                        condition1 = "GreaterThanOrEqual";
                        condition2 = "LessThanOrEqual";
                        break;
                    //not between
                    case "2048":
                        condition2 = "GreaterThanOrEqual";
                        condition1 = "LessThanOrEqual";
                        break;
                    default:
                        condition1 = "";
                        break;
                }

                DocumenPropertyConditions.Add(new DocumenPropertyCondition(0, par.field, par.value, "Text", condition1));

                if (condition2 != "")
                    DocumenPropertyConditions.Add(new DocumenPropertyCondition(0, par.field, par.value2, "Text", condition2));
            }


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
