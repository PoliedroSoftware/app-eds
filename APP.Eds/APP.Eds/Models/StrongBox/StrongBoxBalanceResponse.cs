namespace APP.Eds.Models.StrongBox
{
    public class StrongBoxBalanceResponse
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public StrongBoxGetLastBalanceModel Data { get; set; }
    }
}
