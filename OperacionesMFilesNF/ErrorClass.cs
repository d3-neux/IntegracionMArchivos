using System;


namespace OperacionesMFiles
{
    public class ErrorClass
    {
        public string Valor { get; }
        public string MensajeError { get; }

        public ErrorClass(String valor, String mensajeError)
        {
            Valor = valor;
            MensajeError = mensajeError;
        }

    }
}
