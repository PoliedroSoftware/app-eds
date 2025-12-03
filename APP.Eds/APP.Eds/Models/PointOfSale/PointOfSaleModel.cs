namespace APP.Eds.Models.PointOfSale;

public class PointOfSaleModel
{
    public string InvoiceNumber { get; set; }
    public string? ExternalUuid { get; set; }
    public string? Cufe { get; set; }
    public string? Status { get; set; }
    public DateTime? IssueDatetime { get; set; }
    public DateTime? DueDate { get; set; }
    public string? IssuerName { get; set; }
    public string? IssuerNit { get; set; }
    public string? IssuerEmail { get; set; }
    public string? IssuerPhone { get; set; }
    public string? IssuerAddress { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerId { get; set; }
    public string? BuyerEmail { get; set; }
    public string? BuyerPhone { get; set; }
    public string? BuyerAddress { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? SubtotalAmount { get; set; }
    public decimal? TaxBaseAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PurchaseOrderRef { get; set; }
    public string? Notes { get; set; }
    public string? PdfUrl { get; set; }
    public string? XmlUrl { get; set; }
    public string? WhatsappPhone { get; set; }
    public int? EdsId { get; set; }
    public int? IsleroId { get; set; }
    public string? ProviderTag { get; set; }
}
