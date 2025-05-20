using System.ComponentModel.DataAnnotations.Schema;

namespace polwart_backend.Entities;

public class Map
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public bool IsPublic { get; set; }
	public string BackgroundFormat { get; set; } = "";
	public List<int>? Editors { get; set; } = [];
	[Column(TypeName = "jsonb")] public string Content { get; set; } = "";
}