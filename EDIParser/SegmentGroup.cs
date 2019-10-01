using System;
using System.Collections.Generic;
using System.Linq;

namespace EDIParser {
	public class SegmentGroup : IDisposable {
		private List<Segment> _segments = new List<Segment>();

		public void add(Segment s) {
			_segments.Add(s);
		}

		public IEnumerator<Segment> GetEnumerator() {
			foreach (var segment in _segments) {
				yield return segment;
			}
		}

		public Segment getSegementOfType(string type) {
			foreach (var s in _segments) {
				if (s.type == type) {
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
		public Segment GetSegmentWithPattern(string type, params string[] pattern) {
			foreach (var seg in _segments) {
				if (seg.type == type) {
					for (int ii = 0; ii < pattern.Length; ii++) {
						if (string.IsNullOrWhiteSpace(pattern[ii])) {
							continue;
						}

						if (pattern[ii] != seg.GetElement(ii)) {
							break;
						}

						if (ii == pattern.Length - 1) {
							return seg;
						}
					}
				}
			}

			return new Segment();
		}

		/// <summary>
		/// Returns all Segment with a given pattern
		/// </summary>
		/// <param name="type"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public List<Segment> GetAllSegmentsWithPattern(string type, params string[] pattern) {
			List<Segment> retvals = new List<Segment>();

			foreach (var seg in _segments) {
				if (seg.type == type) {
					for (int ii = 0; ii < pattern.Length; ii++) {
						if (string.IsNullOrWhiteSpace(pattern[ii])) {
							continue;
						}

						if (pattern[ii] != seg.GetElement(ii)) {
							break;
						}

						if (ii == pattern.Length - 1) {
							retvals.Add(seg);
						}
					}
				}
			}

			return retvals;
		}

		/// <summary>
		/// Gets all instances where a segment contains a value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public List<Segment> GetAllSegmentsContaining(string value) {
			List<Segment> retval = new List<Segment>();

			foreach (var segment in _segments) {
				if (segment.SegContains(value)) {
					retval.Add(segment);
				}
			}

			return retval;
		}

		/// <summary>
		/// Gets all instances where a segment contains a value and segment is of type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public List<Segment> GetAllSegmentsContainingOfType(string type, string value) {
			List<Segment> retval = new List<Segment>();

			foreach (var segment in _segments) {
				if (segment.type != type) {
					continue;
				}

				if (segment.SegContains(value)) {
					retval.Add(segment);
				}
			}

			return retval;
		}

		/// <summary>
		/// Gets all instances where a segment contains a value and segment is of type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public List<Segment> GetSegmentsContainingValuesOfType(string type, params string[] values) {
			List<Segment> retval = new List<Segment>();

			foreach (var segment in _segments) {
				if (segment.type != type) {
					continue;
				}

				if (segment.SegContainsValues(values)) {
					retval.Add(segment);
				}
			}

			return retval;
		}

		/// <summary>
		/// Gets all instances where a segment contains a value and segment is of type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public List<Segment> GetSegmentsContainingValuesOfNotType(string type, params string[] values) {
			List<Segment> retval = new List<Segment>();

			foreach (var segment in _segments) {
				if (segment.type == type) {
					continue;
				}

				if (segment.SegContainsValues(values)) {
					retval.Add(segment);
				}
			}

			return retval;
		}

		/// <summary>
		/// Gets all instances where a segment contains a value and the segment is not of a type
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public List<Segment> GetAllSegmentsContainingNotOfType(string type, string value) {
			List<Segment> retval = new List<Segment>();

			foreach (var segment in _segments) {
				if (segment.type == type) {
					continue;
				}

				if (segment.SegContains(value)) {
					retval.Add(segment);
				}
			}

			return retval;
		}

		/// <summary>
		/// Checks is a segment exists with a given pattern
		/// </summary>
		/// <param name="type"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public bool DoesSegExistFromPattern(string type, params string[] pattern) {
			foreach (var seg in _segments) {
				if (seg.type == type) {
					for (int ii = 0; ii < pattern.Length; ii++) {
						if (string.IsNullOrWhiteSpace(pattern[ii])) {
							continue;
						}

						if (pattern[ii] != seg.GetElement(ii)) {
							break;
						}

						if (ii == pattern.Length - 1) {
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the N1, N2, N3, N4 and addition fields of a given loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <param name="additionFields"></param>
		/// <returns></returns>
		public List<Segment> getN1LoopOfType(string qualifier, string[] additionFields) {
			List<Segment> n1Loop = new List<Segment>();

			string[] types = {"N2", "N3", "N4"};

			qualifier = qualifier.ToUpper();

			bool inLoop = false;

			foreach (var s in _segments) {
				if (s.type == "N1" && s.GetElement(1) == qualifier) {
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}

				//You've gone to far
				if (s.type == "N1" && inLoop) {
					break;
				}

				if (inLoop && (types.Contains(s.type) || additionFields.Contains(s.type))) {
					n1Loop.Add(s);
				}
			}

			return n1Loop;
		}

		/// <summary>
		/// Grabs every N1-N4 loop
		/// </summary>
		/// <returns></returns>
		public List<SegmentGroup> GetAllN1Loops() {
			List<SegmentGroup> retVal = new List<SegmentGroup>();

			string[] types = {"N1", "N2", "N3", "N4"};
			foreach (var segment in _segments) {
				if (types.Contains(segment.type)) {
					if (segment.type == "N1") {
						retVal.Add(new SegmentGroup());
					}

					retVal.Last().add(segment);
				}
			}

			return retVal;
		}

		/// <summary>
		/// This is the easier way to get an element
		/// </summary>
		/// <param name="index"></param>
		public Segment this[int index] {
			get => GetElement(index);
		}

		public Segment GetElement(int index) {
			if (_segments.Count >= index) {
				return _segments[index];
			}

			return new Segment();
		}

		/// <summary>
		/// Grabs every message segment in the segment group
		/// </summary>
		/// <returns></returns>
		public List<Segment> GetAllMessageSegments() {
			string[] types = {"N9", "MTX", "MSG"};
			List<Segment> retval = new List<Segment>();
			foreach (var segment in _segments) {
				if (types.Contains(segment.type)) {
					retval.Add(segment);
				}
			}

			return retval;
		}
		

		public void Dispose() {
		}
	}
}