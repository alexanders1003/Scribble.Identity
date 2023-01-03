namespace Scribble.Identity.Web.Models.EventItems;

public class EventItemViewModel
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Logger { get; set; }
    public string? Level { get; set; }
    public string? Message { get; set; }
    public string? ThreadId { get; set; }
    public string? ExceptionMessage { get; set; }
}