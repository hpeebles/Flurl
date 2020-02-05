using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Flurl.Util;

namespace Flurl.PerfTests
{
	public class ToKeyValuePairs
	{
		private readonly string _inputString = "abc=12345&abc=6789&xyz=0";
		private readonly IEnumerable _inputCollection = new[] {
			new KeyValuePair<string, object>("abc", 12345),
			new KeyValuePair<string, object>("abc", 6789),
			new KeyValuePair<string, object>("xyz", 0)
		};
		private readonly object _inputObject = new {
			abc = new[] {12345, 6789},
			xyz = 0
		};

		public static void Run() {
			var tests = new ToKeyValuePairs();
			tests.String();
			tests.Collection();
			tests.Object();
		}

		[Benchmark]
		public List<KeyValuePair<string, object>> String() {
			return _inputString.ToKeyValuePairs().ToList();
		}

		[Benchmark]
		public List<KeyValuePair<string, object>> Collection() {
			return _inputCollection.ToKeyValuePairs().ToList();
		}

		[Benchmark]
		public List<KeyValuePair<string, object>> Object() {
			return _inputObject.ToKeyValuePairs().ToList();
		}
	}
}