
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entities;

public class RefreshToken : BaseEntity
{
    public int IdUser { get; set; }
    [ForeignKey("IdUser")]  
    public User User { get; set; }
    public string ? Token { get; set; }
    public string ? JwtId { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
}
