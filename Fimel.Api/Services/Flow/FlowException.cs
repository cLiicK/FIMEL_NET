namespace Fimel.Api.Services.Flow
{
    public class FlowException : Exception
    {
        public int StatusHttp { get; }
        public string? Codigo { get; }

        public FlowException(int statusHttp, string? codigo, string mensaje) : base(mensaje)
        {
            StatusHttp = statusHttp;
            Codigo = codigo;
        }
    }
}
