using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockMarketStorage.Infrastructure.Persistence;

[Table("ticks")]
public sealed class TickEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("ticker")]
    [MaxLength(32)]
    public string Ticker { get; set; } = string.Empty;

    [Required]
    [Column("price")]
    public decimal Price { get; set; }

    [Required]
    [Column("volume")]
    public decimal Volume { get; set; }

    [Required]
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Required]
    [Column("source")]
    [MaxLength(64)]
    public string Source { get; set; } = string.Empty;
}
