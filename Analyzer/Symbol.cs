using System;
using System.Collections.Generic;

namespace Analyzer
{
	public class Symbol
	{
		public string identifier { get; set; }

		public int? value { get; set; }

		public Boolean isLoaded = false;

		public List<int> lines = new List<int>();

		public List<int> columns = new List<int>();

	}
}

