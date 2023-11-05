using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkr_DL.Entities
{
	public class File
	{
		public string CDNUrl { get; set; } = string.Empty;
		public string FileName { get; set; } = string.Empty;
		public long Size { get; set; } = 0;
	}
}
