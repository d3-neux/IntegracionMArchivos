using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperacionesMFiles
{
    public class Historico
    {
        public string Id { get; set; }
        public string Nombre_Titulo { get; set; }
        public string Tipo_Archivo { get; set; }
        public string Fecha_Corte { get; set; }
        public string ID_Cliente { get; set; }
        public string ID_Num_Cuenta { get; set; }
        public string Nom_Persona { get; set; }
        public string Direccion { get; set; }
        public string Num_Credito { get; set; }
        public string Ciudad_Persona { get; set; }
        public string Telefonos_Persona { get; set; }
        public string Email { get; set; }
        public string Fecha_Carga { get; set; }
        public string ID_Mfiles { get; set; }
        public string Num_Doc_Tribu { get; set; }
        public string Fecha_Registro { get; set; }

        public override string ToString()
        {
            return $"{Nombre_Titulo} {Tipo_Archivo} {Fecha_Corte} {ID_Cliente} " ;
        }
    }


}
