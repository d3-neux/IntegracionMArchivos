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
    //esta clase representa los criterios de búsqueda que se enviarán a m-files para que devuelva n documentos

    //Root myDeserializedClass = JsonConvert.DeserializeObject<MFilesSearchDocument>(myJsonResponse); 

    public class DocumenPropertyCondition
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Value { get; set; }
        public string Type { get; set; }
        public string Condition { get; set; }

        public DocumenPropertyCondition(int id, string name, string value, string type, string condition)
        {
            if (name == null)
                name = "XXX";

            if (value == null)
                value = "";

            if (Type == null)
                Type = "";

            if (Condition == null)
                Condition = "";

            Id = id;
            Name = name;
            Value = value;
            Type = type;
            Condition = condition;

            //Si el valor es igual al nombre de una clase, se reemplaza el nombre por el id en la variabla value

            if (id == 100) { 
                foreach (ValueListItem item in IntegracionMFiles.client.ValueListItemOperations.GetValueListItems(1).Items.Where(p => (p.Name == Value)))
                {
                    Value = String.Concat(item.ID);
                }
            }
        }

        /*
            M-Files Conditions operator                  
            Unknown = 0,
            Equals = 1 = 1,
            LessThan = 2 = 4,
            LessThanOrEqual = 3 = 8,
            GreaterThan = 4 = 16
            GreaterThanOrEqual = 5 = 32,
            MatchesWildcard = 6 //////////,
            Contains = 7 = 64,
            StartsWith = 8

            Diners condition operators
            NotEqual = 2; NOOO
            NotIn = 128;
            Like = 256;
            NotLike = 512;
            
            Between = 1024;
            NotBetween = 2048;

            Equal = 1 ; //
            LessThan = 4;
            LessThanEqual = 8;
            GreaterThan = 16;
            GreaterThanEqual = 32;   
            In = 64;
        */

        public override string ToString()
        {
            return $"ID {Id}, NAME {Name}, Value {Value}, Type {Type}, Type {Condition} ";
        }
    }

    public class MFilesSearchDocument
    {
        public List<DocumenPropertyCondition> DocumenPropertyConditions = new List<DocumenPropertyCondition>();

        public override string ToString()
        {
            return $"Properties: {string.Join(",", DocumenPropertyConditions)}";
        }

        

        public List<ISearchCondition> GetMFDocConditions()
        {
            List<ISearchCondition> documentConditions = new List<ISearchCondition>();

            foreach (var item in DocumenPropertyConditions)
            {
                if (item.Id == 0 && item.Name != "Nombre")
                    item.Id = IntegracionMFiles.mfPropertyOperator.GetPropertyDefIDByAlias(item.Name);

                //IntegracionMFiles.mfPropertyOperator.get
                //Se obtiene el tipo de condicion de acuerdo al tipo especificado
                SearchConditionOperators condition;

                try
                {
                    condition = (SearchConditionOperators)Enum.Parse(typeof(SearchConditionOperators), item.Condition);
                }
                catch(Exception e)
                {
                    continue;
                }

                var type = item.Type.ToLower();

                ISearchCondition mfCondition = null;

                if (type == "text")
                {
                    mfCondition = new TextPropertyValueSearchCondition(item.Id, item.Value, condition);
                }
                else if (type == "date")
                {
                    var day = Int32.Parse(item.Value.Split('/')[0]);
                    var month = Int32.Parse(item.Value.Split('/')[1]);
                    var year = Int32.Parse(item.Value.Split('/')[2]);
                    mfCondition = new DatePropertyValueSearchCondition(item.Id, new DateTime(year, month, day), condition);
                }
                else if (type == "floating")
                {
                    mfCondition = new NumericPropertyValueSearchCondition(item.Id, Double.Parse(item.Value), condition);

                }
                else if (type == "integer")
                {
                    mfCondition = new NumericPropertyValueSearchCondition(item.Id, Int32.Parse(item.Value), condition);
                }
                else if (type == "lookup")
                {
                    mfCondition = new LookupPropertyValueSearchCondition(item.Id, condition, false, Int32.Parse(item.Value));
                }
                else
                {
                    continue;
                    //throw new Exception("Tipo de dato es incorrecto");
                }

                documentConditions.Add(mfCondition);
            }
            
            return documentConditions;
        }

        public List<PropertyValue> GetMFProperties()
        {
            List<PropertyValue> documentProperties = new List<PropertyValue>();

            foreach (var item in DocumenPropertyConditions)
            {
                 documentProperties.Add(CreateMFProperty(item.Id, item.Value, item.Type));
            }
            return documentProperties;
        }

        private PropertyValue CreateMFProperty(int id, string value, string type)
        {
            //valor default si no se especifican tipos conocidos
            MFDataType mFDataType = MFDataType.Text;

            if (type == "Text")
                mFDataType = MFDataType.Text;
            else if (type == "Date")
                mFDataType = MFDataType.Date;
            else if (type == "Floating")
            {
                mFDataType = MFDataType.Floating;
                value = value.Replace(',', '.');
            }
            else if (type == "Integer")
            {
                mFDataType = MFDataType.Integer;
                value = value.Replace(',', '.');
            }

            return new PropertyValue
            {
                PropertyDef = id,
                TypedValue = new TypedValue { DataType = mFDataType, Value = value }
            };
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
