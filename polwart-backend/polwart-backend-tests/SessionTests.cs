using polwart_backend;

namespace polwart_backend_tests;

public class SessionTests
{
	private Session _session = null!;
	
	[SetUp]
	public void Setup()
	{
		_session = new(0, "");
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