using System.Text.Json.Serialization;

namespace APP.Eds.Models.Court;

public class CourtListItemModel
{
    public int Id { get; set; }
    public string DateStarttime { get; set; }
    public string DateEndtime { get; set; }
    public int Consecutive { get; set; }
    public int IdEds { get; set; }
    public string Eds { get; set; }
    public string Bussiness { get; set; }
    public string Islander { get; set; }
    public string Starttime { get; set; }
    public string Endtime { get; set; }
    public double Distinc { get; set; }
    public double TotalAccumulatedAmount { get; set; }
    public double TotalAccumulatedGallons { get; set; }
    
    [JsonPropertyName("description")]
    public string Descripcion { get; set; }
    
    // Propiedades de listas con inicializaci�n segura
    private List<CollectionItem> _collections;
    public List<CollectionItem> Collections 
    { 
        get => _collections ??= new List<CollectionItem>(); 
        set => _collections = value ?? new List<CollectionItem>(); 
    }
    
    private List<DispenserItem> _dispensers;
    public List<DispenserItem> Dispensers 
    { 
        get => _dispensers ??= new List<DispenserItem>(); 
        set => _dispensers = value ?? new List<DispenserItem>(); 
    }
    
    private List<DocumentItem> _documents;
    public List<DocumentItem> Documents 
    { 
        get => _documents ??= new List<DocumentItem>(); 
        set => _documents = value ?? new List<DocumentItem>(); 
    }
    
    private List<ExpenditureItem> _expenditures;
    public List<ExpenditureItem> Expenditures 
    { 
        get => _expenditures ??= new List<ExpenditureItem>(); 
        set => _expenditures = value ?? new List<ExpenditureItem>(); 
    }

    // Constructor to ensure proper initialization
    public CourtListItemModel()
    {
        // Las propiedades ya se inicializan autom�ticamente en sus getters
        // pero podemos asegurar la inicializaci�n aqu� tambi�n
        Collections = new List<CollectionItem>();
        Dispensers = new List<DispenserItem>();
        Documents = new List<DocumentItem>();
        Expenditures = new List<ExpenditureItem>();
    }

    //Traducciones
    public string DateTranslation { get; set; }
    public string ConsecutiveTranslation { get; set; }
    public string IslanderTranslation { get; set; }
    public string CourtDetailTranslation { get; set; }
    public string ShiftTranslation { get; set; }
    public string TotalsTranslation { get; set; }
    public string AccumulatedAmountTranslation { get; set; }
    public string AccumulatedGallonsTranslations { get; set; }
    public string DistincTranslation { get; set; }
    public string DispensersTranslation { get; set; }
    public string AccumulatedGallons { get; set; }
    public string LastAccumulatedAmountTranslation { get; set; }
    public string LastAccumulatedGallonsTranslation { get; set; }
    public string DocumentsTranslation { get; set; }
    public string ExpendituresTranslation { get; set; }
    public string CourtTranslation { get; set; }
    public string ThereIsNoImageTranslation { get; set; }
    public string ExpenditureTranslation { get; set; }
}

public class Translation
{
    public string DateTranslation { get; set; }
}

public class CollectionItem
{
    public int Id { get; set; }
    public int Court { get; set; }
    public string Date { get; set; }
    public string Collection { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
    public string DateTranslation { get; set; }
    public string CollectionTranslation { get; set; }
    public string AmountTranslation { get; set; }
    public string DescriptionTranslation { get; set; }
}

public class DispenserItem
{
    public int Id { get; set; }
    public string Business { get; set; }
    public int IdEds { get; set; }
    public string Eds { get; set; }
    public int Dispenser { get; set; }
    public int NumberHose { get; set; }
    public double LastAccumulatedAmount { get; set; }
    public double LastAccumulatedGallons { get; set; }
    public int CodeCourt { get; set; }
    public string Islander { get; set; }
    public string Starttime { get; set; }
    public string Endtime { get; set; }
    public string Date { get; set; }
    public double Distinc { get; set; }
    public string Product { get; set; }
    public double Price { get; set; }
    public string ProductType { get; set; }
    public double AccumulatedAmount { get; set; }
    public double AccumulatedGallons { get; set; }
    public string DispenserTranslation { get; set; }
    public string NumberHoseTranslation { get; set; }
    public string ProductTranslation { get; set; }
    public string PriceTranslation { get; set; }
    public string StarttimeTranslation { get; set; }
    public string EndtimeTranslation { get; set; }
    public string LastAccumulatedAmountTranslation { get; set; }
    public string LastAccumulatedGallonsTranslation { get; set; }
    public string AccumulatedAmountTranslation { get; set; }
    public string AccumulatedGallonsTranslations { get; set; }
}

public class DocumentItem
{
    public int Id { get; set; }
    public int Court { get; set; }
    public string Descripcion { get; set; }
    public string CourtTranslation { get; set; }
    public string ThereIsNoImageTranslation { get; set; }
    public ImageSource ImageSource
    {
        get
        {
            if (string.IsNullOrEmpty(Descripcion))
                return null;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(Descripcion);
                return ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            catch
            {
                return null;
            }
        }
    }
    public bool HasImage
    {
        get
        {
            if (string.IsNullOrEmpty(Descripcion))
                return false;

            try
            {
                byte[] imageBytes = Convert.FromBase64String(Descripcion);
                using var ms = new MemoryStream(imageBytes);
                return ms.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
    public bool NoImage => !HasImage;
}

public class ExpenditureItem
{
    public int Id { get; set; }
    public int Court { get; set; }
    public string Date { get; set; }
    public string Expenditure { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
    public string DateTranslation { get; set; }
    public string ExpenditureTranslation { get; set; }
    public string AmountTranslation { get; set; }
    public string DescriptionTranslation { get; set; }
}
