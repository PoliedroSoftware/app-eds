namespace APP.Eds.Models.Islander
{
    public class IslanderModel
    {
        public int IdIslander { get; set; } 
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public int IdEds { get; set; }
        public required string Password { get; set; }
    }
}
