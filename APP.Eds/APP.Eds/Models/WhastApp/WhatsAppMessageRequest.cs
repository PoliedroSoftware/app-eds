using APP.Eds.Models.Court;

namespace APP.Eds.Models.WhastApp;

public class WhatsAppMessageRequest
{
    public CourtModel Court { get; set; }
    public string PhoneNumber { get; set; }
}