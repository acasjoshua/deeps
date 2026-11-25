namespace Deeps.Models;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public List<Category> Categories { get; set; } = new();
}
