using System.Text;
using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace OpenApi.Models.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var array = new[] { 0, 1, 2, 3, 4, 5, 6 };

		var span = array.AsSpan();
		var subSpan = span[7..];

		Console.WriteLine(JsonSerializer.Serialize(subSpan.ToArray()));
	}
}