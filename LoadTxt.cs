using System.IO;
using System.Reflection;
using System.Text;

namespace VWA
{
	
		class LoadTxt { 
		public string ReadEmbeddedTextFile(string resourceName)
		{
			string content = string.Empty;

			
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				if (stream != null)
				{
					using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
					{
						content = reader.ReadToEnd(); 
					}
				}
				else
				{
					content = "Ressourcen nicht gefunden."; 
				}
			}

			return content;
		}
		
	}
}
