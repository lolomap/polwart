namespace polwart_backend.Requests;

public class ChangeEditorsRequest
{
	public int MapId { get; set; }
	public List<int> Editors { get; set; } = [];
}