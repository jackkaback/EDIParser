using System.Collections.Generic;

namespace EDIParser
{
	public class SegmentGroup
	{
		private List<Segment> _segments = new List<Segment>();

		public void add(Segment s)
		{
			_segments.Add(s);
		}

		public IEnumerator<Segment> GetEnumerator()
		{
			foreach (var segment in _segments)
			{
				yield return segment;
			}
		}

		public Segment getSegementOfType(string type)
		{
			foreach (var s in _segments)
			{
				if (s.type == type)
				{
					return s;
				}
			}
			
			return new Segment();
		}

		/// <summary>
		/// returns a segment with a specific pattern, null/empty/whitespaces are wildcards
		/// </summary>
		/// <param name="type"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public Segment GetSegmentWithPattern(string type, params string[] pattern)
		{

			foreach (var seg in _segments)
			{
				if (seg.type == type)
				{
					for (int ii = 0; ii < pattern.Length; ii++)
					{
						if (string.IsNullOrWhiteSpace(pattern[ii]))
						{
							continue;
						}

						if (pattern[ii] != seg.GetElement(ii))
						{
							break;
						}

						if (ii == pattern.Length - 1)
						{
							return seg;
						}
					}
				}
			}
			
			return new Segment();
		}
		
		/// <summary>
		/// Checks is a segment exists with a given pattern
		/// </summary>
		/// <param name="type"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public bool DoesSegExistFromPattern(string type, params string[] pattern)
		{
			foreach (var seg in _segments)
			{
				if (seg.type == type)
				{
					for (int ii = 0; ii < pattern.Length; ii++)
					{
						if (string.IsNullOrWhiteSpace(pattern[ii]))
						{
							continue;
						}

						if (pattern[ii] != seg.GetElement(ii))
						{
							break;
						}

						if (ii == pattern.Length - 1)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}