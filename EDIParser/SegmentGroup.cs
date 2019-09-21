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
	}
}