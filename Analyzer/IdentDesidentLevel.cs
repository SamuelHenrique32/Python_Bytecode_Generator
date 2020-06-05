using System;
using System.Collections.Generic;

namespace Analyzer
{
	public class IdentDesidentLevel
	{
		public int lineInFile { get; set; }

		public Analyzer.TipoTk tokenType;

		public int? bytecodeRegistersLine = null;

		public IdentDesidentLevel(int lineInFile, Analyzer.TipoTk tokenType)
		{
			this.lineInFile = lineInFile;

			this.tokenType = tokenType;
		}
	}
}
