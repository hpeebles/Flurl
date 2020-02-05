using BenchmarkDotNet.Running;

namespace Flurl.PerfTests
{
	class Program
	{
		static void Main(string[] args) {
#if DEBUG
			ToKeyValuePairs.Run();
#else
			BenchmarkRunner.Run<ToKeyValuePairs>();
#endif
		}
	}
}