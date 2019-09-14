using System.Collections.Generic;

namespace EDIParser
{
	public class Segment
	{
		public readonly string type;
		
		private char elementTerm = '*';
		private char segterminator = '~';
		
		private List<string> elements = new List<string>();

		public Segment(string[] values)
		{
			type = values[0];
			
			foreach (var i in values)
			{
				elements.Add(i);
			}
		}
		
		public Segment(string[] values, char segterm, char eleterm)
		{
			type = values[0];
			elementTerm = eleterm;
			segterminator = segterm;
			
			foreach (var i in values)
			{
				elements.Add(i);
			}
		}

		/// <summary>
		/// For making a null object
		/// </summary>
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

		/// <summary>
		/// This shouldn't be used to write the data out, bt rather for debugging or to write to console
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Join(elementTerm, elements) + segterminator;
		}
		
		public IEnumerable<string> GetEnumerable() {
			foreach (var e in elements)
			{
				yield return e;
			}
		}
	}
}