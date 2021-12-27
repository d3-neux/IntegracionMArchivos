using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperacionesMFiles
{
    class DataAccess
    {
        static List<Historico> historicos = new List<Historico>();


        public static List<MFilesDocument> GetRecords(string conditions)
        {
            List<MFilesDocument> resultList = new List<MFilesDocument>();

            var sqlQuery = $"select * from historico where 1 = 1 {conditions}";

            System.Diagnostics.Debug.WriteLine($"SQL QUERY {sqlQuery}");

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(IntegracionMFiles.dbConnection) )
            {
                historicos = connection.Query<Historico>(sqlQuery).ToList();
            }

            foreach (var historico in historicos)
            {
                //docProperties.Add(new DocumentProperty(1020, historico.Tipo_Archivo, "TIPO_ARCHIVO"));
                List<DocumentProperty> docProperties = new List<DocumentProperty>();

                docProperties.Add(new DocumentProperty(1021, historico.Fecha_Corte, "FECHA_CORTE"));
                docProperties.Add(new DocumentProperty(1022, historico.ID_Cliente, "ID_CLIENTE"));
                docProperties.Add(new DocumentProperty(1023, historico.ID_Num_Cuenta, "ID_NUM_CUENTA"));
                docProperties.Add(new DocumentProperty(1024, historico.Nom_Persona, "NOM_PERSONA"));
                docProperties.Add(new DocumentProperty(1025, historico.Direccion, "DIRECCION"));
                docProperties.Add(new DocumentProperty(1026, historico.Num_Credito, "NUM_CREDITO"));
                docProperties.Add(new DocumentProperty(1027, historico.Ciudad_Persona, "CIUDAD_PERSONA"));
                docProperties.Add(new DocumentProperty(1028, historico.Telefonos_Persona, "TELEFONOS_PERSONA"));
                docProperties.Add(new DocumentProperty(1029, historico.Email, "EMAIL"));
                docProperties.Add(new DocumentProperty(1030, historico.Fecha_Carga, "FECHA_CARGA"));
                //docProperties.Add(new DocumentProperty(1031, historico.ID_Mfiles, "ID_MFILES"));
                docProperties.Add(new DocumentProperty(1032, historico.Num_Doc_Tribu, "NUM_DOC_TRIBU"));
                docProperties.Add(new DocumentProperty(1033, historico.Fecha_Registro, "FECHA_REGISTRO"));

                resultList.Add(new MFilesDocument(docProperties, null, int.Parse(historico.ID_Mfiles)));

            }

            return resultList;
        }



    }
}
