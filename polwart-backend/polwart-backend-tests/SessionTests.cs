using polwart_backend;
using polwart_backend.Entities;

namespace polwart_backend_tests;

public class SessionTests
{
	private Session _session = null!;
	
	[SetUp]
	public void Setup()
	{
		_session = new(new());
	}

	[Test]
	public void PatchTest()
	{
		Assert.Pass();
	}

	[Test]
	public void GetRevisions()
	{
		
	}
}