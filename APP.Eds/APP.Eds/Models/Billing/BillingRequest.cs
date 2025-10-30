using System.Text.Json.Serialization;

namespace APP.Eds.Models.Billing;

public class BillingRequest
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("sendToEmail")]
    public string SendToEmail { get; set; }

    [JsonPropertyName("softwareManufacturer")]
    public SoftwareManufacturer SoftwareManufacturer { get; set; }

    [JsonPropertyName("payPointInfo")]
    public PayPointInfo PayPointInfo { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; }

    [JsonPropertyName("prefix")]
    public string Prefix { get; set; }

    [JsonPropertyName("transactionDate")]
    public string TransactionDate { get; set; }

    [JsonPropertyName("orderReference")]
    public OrderReference OrderReference { get; set; }

    [JsonPropertyName("sendEmail")]
    public bool SendEmail { get; set; }

    [JsonPropertyName("attachment1")]
    public Attachment Attachment1 { get; set; }

    [JsonPropertyName("attachment2")]
    public Attachment Attachment2 { get; set; }

    [JsonPropertyName("customerEntity")]
    public CustomerEntity CustomerEntity { get; set; }

    [JsonPropertyName("paymentEntity")]
    public PaymentEntity PaymentEntity { get; set; }

    [JsonPropertyName("generalAllowanceEntity")]
    public List<GeneralAllowanceEntity> GeneralAllowanceEntity { get; set; } = new();

    [JsonPropertyName("itemElectronicEntity")]
    public List<ItemElectronicEntity> ItemElectronicEntity { get; set; } = new();

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; }

    [JsonPropertyName("resolutionText")]
    public string ResolutionText { get; set; }

    [JsonPropertyName("headNote")]
    public string HeadNote { get; set; }

    [JsonPropertyName("footNote")]
    public string FootNote { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; }

    [JsonPropertyName("allowanceTotal")]
    public double AllowanceTotal { get; set; }

    [JsonPropertyName("invoiceBaseTotal")]
    public double InvoiceBaseTotal { get; set; }

    [JsonPropertyName("invoiceTaxExclusiveTotal")]
    public double InvoiceTaxExclusiveTotal { get; set; }

    [JsonPropertyName("invoiceTaxInclusiveTotal")]
    public double InvoiceTaxInclusiveTotal { get; set; }

    [JsonPropertyName("totalToPay")]
    public double TotalToPay { get; set; }

    [JsonPropertyName("allTaxTotalEntity")]
    public List<TaxTotalEntity> AllTaxTotalEntity { get; set; } = new();

    [JsonPropertyName("allHoldingsTaxTotalEntity")]
    public List<TaxTotalEntity> AllHoldingsTaxTotalEntity { get; set; } = new();

    [JsonPropertyName("finalTotalToPay")]
    public double FinalTotalToPay { get; set; }
}

public class SoftwareManufacturer
{
    [JsonPropertyName("ownerName")]
    public string OwnerName { get; set; }

    [JsonPropertyName("softwareName")]
    public string SoftwareName { get; set; }

    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
}

public class PayPointInfo
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("cashierName")]
    public string CashierName { get; set; }

    [JsonPropertyName("payPointType")]
    public string PayPointType { get; set; }

    [JsonPropertyName("saleCode")]
    public string SaleCode { get; set; }
}

public class OrderReference
{
    [JsonPropertyName("idOrder")]
    public string IdOrder { get; set; }
}

public class Attachment
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; }

    [JsonPropertyName("b64Data")]
    public string B64Data { get; set; }
}

public class CustomerEntity
{
    [JsonPropertyName("identificationNumber")]
    public string IdentificationNumber { get; set; }

    [JsonPropertyName("multipleResolution")]
    public string MultipleResolution { get; set; }

    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; }

    [JsonPropertyName("dv")]
    public string Dv { get; set; }

    [JsonPropertyName("profit")]
    public double Profit { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("merchantRegistration")]
    public string MerchantRegistration { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("typeDocumentIdentificationId")]
    public int TypeDocumentIdentificationId { get; set; }

    [JsonPropertyName("typeOrganizationId")]
    public int TypeOrganizationId { get; set; }

    [JsonPropertyName("typeLiabilityId")]
    public int TypeLiabilityId { get; set; }

    [JsonPropertyName("municipalityId")]
    public int MunicipalityId { get; set; }

    [JsonPropertyName("municipalityCode")]
    public string MunicipalityCode { get; set; }

    [JsonPropertyName("typeRegimeId")]
    public int TypeRegimeId { get; set; }
}

public class PaymentEntity
{
    [JsonPropertyName("paymentFormId")]
    public int PaymentFormId { get; set; }

    [JsonPropertyName("paymentMethodId")]
    public int PaymentMethodId { get; set; }

    [JsonPropertyName("paymentDueDate")]
    public string PaymentDueDate { get; set; }

    [JsonPropertyName("durationMeasure")]
    public string DurationMeasure { get; set; }
}

public class GeneralAllowanceEntity
{
    [JsonPropertyName("allowanceChargeReason")]
    public string AllowanceChargeReason { get; set; }

    [JsonPropertyName("allowancePercent")]
    public double AllowancePercent { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("baseAmount")]
    public double BaseAmount { get; set; }
}

public class ItemElectronicEntity
{
    [JsonPropertyName("unitMeasureId")]
    public int UnitMeasureId { get; set; }

    [JsonPropertyName("lineExtensionAmount")]
    public double LineExtensionAmount { get; set; }

    [JsonPropertyName("transaccion")]
    public int Transaccion { get; set; }

    [JsonPropertyName("freeOfChargeIndicator")]
    public bool FreeOfChargeIndicator { get; set; }

    [JsonPropertyName("allowanceCharges")]
    public List<AllowanceCharge> AllowanceCharges { get; set; } = new();

    [JsonPropertyName("taxTotals")]
    public List<TaxTotal> TaxTotals { get; set; } = new();

    [JsonPropertyName("withHoldingTaxTotal")]
    public List<TaxTotal> WithHoldingTaxTotal { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("typeItemIdentificationId")]
    public int TypeItemIdentificationId { get; set; }

    [JsonPropertyName("priceAmount")]
    public double PriceAmount { get; set; }

    [JsonPropertyName("baseQuantity")]
    public double BaseQuantity { get; set; }

    [JsonPropertyName("invoicedQuantity")]
    public double InvoicedQuantity { get; set; }

    [JsonPropertyName("percent")]
    public double Percent { get; set; }

    [JsonPropertyName("taxAmount")]
    public double TaxAmount { get; set; }

    [JsonPropertyName("unitPrice")]
    public double UnitPrice { get; set; }

    [JsonPropertyName("subtotal")]
    public double Subtotal { get; set; }
}

public class AllowanceCharge
{
    [JsonPropertyName("chargeIndicator")]
    public bool ChargeIndicator { get; set; }

    [JsonPropertyName("allowanceChargeReason")]
    public string AllowanceChargeReason { get; set; }

    [JsonPropertyName("multiplierFactorNumeric")]
    public double MultiplierFactorNumeric { get; set; }

    [JsonPropertyName("amount")]
    public double Amount { get; set; }

    [JsonPropertyName("baseAmount")]
    public double BaseAmount { get; set; }
}

public class TaxTotal
{
    [JsonPropertyName("taxId")]
    public int TaxId { get; set; }

    [JsonPropertyName("percent")]
    public double Percent { get; set; }

    [JsonPropertyName("taxAmount")]
    public double TaxAmount { get; set; }

    [JsonPropertyName("taxableAmount")]
    public double TaxableAmount { get; set; }
}

public class TaxTotalEntity
{
    [JsonPropertyName("taxId")]
    public int TaxId { get; set; }

    [JsonPropertyName("taxAmount")]
    public double TaxAmount { get; set; }

    [JsonPropertyName("percent")]
    public double Percent { get; set; }

    [JsonPropertyName("taxableAmount")]
    public double TaxableAmount { get; set; }
}
