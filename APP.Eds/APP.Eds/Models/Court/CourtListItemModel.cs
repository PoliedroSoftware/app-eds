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
    public List<CollectionItem> Collections { get; set; }
    public List<DispenserItem> Dispensers { get; set; }
    public List<DocumentItem> Documents { get; set; }
    public List<ExpenditureItem> Expenditures { get; set; }
}

public class CollectionItem
{
    public int Id { get; set; }
    public int Court { get; set; }
    public string Date { get; set; }
    public string Collection { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
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
}

public class DocumentItem
{
    public int Id { get; set; }
    public int Court { get; set; }
    public string Descripcion { get; set; }
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
}
