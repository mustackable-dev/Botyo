using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Botyo.Entities;

[Table("Notifications")]
public record Notification
{
    [Key]
    public int Id {get; set;}
    public required string Content { get; set; }
    public required string Cron {get; set;}
    public bool Active {get; set;}
}