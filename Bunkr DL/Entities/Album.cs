using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bunkr_DL.Entities
{
	public class Album
	{
		public string Title { get; set; } = string.Empty;
		public List<File> Files { get; set; } = new List<File>();
	}
}
