using System.Collections.Generic;

namespace EDIParser
{
	public class Segment
	{
		public readonly string type;
		private List<string> elements;

		public Segment(string[] values)
		{
			type = values[0];
			
			foreach (var i in values)
			{
				elements.Add(i);
			}
		}

		public Segment()
		{
		}
		
		/// <summary>
		/// Get's the value of the element at I if it exists
		/// </summary>
		/// <param name="i"></param>
		/// <returns>The I'th element or an empty string'</returns>
		public string GetElement(int i)
		{
			if (elements.Count > i)
			{
				return elements[i];
			}
			return string.Empty;
		}
	}
}