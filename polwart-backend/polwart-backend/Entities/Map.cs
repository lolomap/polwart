using System.ComponentModel.DataAnnotations.Schema;

namespace polwart_backend.Entities;

public class Map
{
	public int Id { get; set; }
	public bool IsPublic { get; set; }
	[Column(TypeName = "jsonb")] public string Content { get; set; } = "";
}