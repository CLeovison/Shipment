namespace Shipment.Entities;

public class RefreshTokens
{
    public Guid RefreshTokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;
    public Users? User { get; set; }
}