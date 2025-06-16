namespace APP.Eds.Models.Court
{
    public class CourtModel
    {
        public int IdBusiness { get; set; }
        public int IdEds { get; set; }
        public int IdIslander { get; set; }
        public string DateStarttime { get; set; }
        public string Starttime { get; set; }
        public string DateEndtime { get; set; }
        public string Endtime { get; set; }
        public string? Descripcion { get; set; }
        public double? Distintic { get; set; }
        public IEnumerable<CourtDispenser> CourtDispensers { get; set; }
        public IEnumerable<CourtDocument> CourtDocuments { get; set; }
        public IEnumerable<CourtExpenditure> CourtExpenditures { get; set; }
        public IEnumerable<CourtTypeOfCollection> CourtTypeOfCollections { get; set; }
    }
}
