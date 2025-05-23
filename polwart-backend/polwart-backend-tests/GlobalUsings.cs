using Microsoft.AspNetCore.SignalR;

namespace polwart_backend_tests;

public class TestClient: ISingleClientProxy
{
	public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = new CancellationToken())
	{
		throw new NotImplementedException();
	}

	public Task<T> InvokeCoreAsync<T>(string method, object?[] args, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}