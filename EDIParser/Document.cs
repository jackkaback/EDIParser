using System;
using System.Collections.Generic;
using System.Linq;

namespace EDIParser {
	public class Document : IDisposable {
		/// <summary>
		/// The document type express as the number
		/// </summary>
		public readonly string DocumentType;

		/// <summary>
		/// Special ST segement
		/// </summary>
		private Segment _ST;

		/// <summary>
		/// special SE segement
		/// </summary>
		private Segment _SE;

		/// <summary>
		/// The string from this document
		/// </summary>
		private string _toString;

		/// <summary>
		/// List of all segements
		/// </summary>
		private List<Segment> _segments;

		private string _detailStart;

		/// <summary>
		/// Every segment in the header
		/// </summary>
		public SegmentGroup header = new SegmentGroup();

		/// <summary>
		/// A list of every item level loop
		/// </summary>
		public List<SegmentGroup> deatils = new List<SegmentGroup>();

		/// <summary>
		/// Everything in the trailer
		/// </summary>
		public SegmentGroup trailer = new SegmentGroup();

		public H1Loop h1Loop;

		/// <summary>
		/// Generates the whole transaction
		/// </summary>
		/// <param name="segments"></param>
		public Document(List<Segment> segments) {
			this._segments = segments;
			DocumentType = segments[0].type;
			_ST = segments[0];
			_SE = segments.Last();

			GenerateToString();
			if (_ST[1] == "856") {
				var temp = segments;

				foreach (var segment in temp) {
					if (segment.type != "H1") {
						header.add(segment);
						temp.Remove(segment);
						continue;
					}

					h1Loop = new H1Loop(temp);
					break;
				}
			}
		}

		/// <summary>
		/// This one will throw an error if the SE count is different
		/// </summary>
		/// <param name="segments"></param>
		/// <param name="ThrowError"></param>
		public Document(List<Segment> segments, bool ThrowError) {
			this._segments = segments;
			DocumentType = segments[0].type;
			_ST = segments[0];
			_SE = segments.Last();

			if (ThrowError && _segments.Count != int.Parse(SEGetSECount())) {
				throw new Exception("SE count and segment count do not match");
			}

			GenerateToString();
		}

		private void GenerateToString() {
			_toString = "";

			foreach (var s in _segments) {
				_toString += s.ToString() + "\r\n";
			}
		}

		public bool DoesSegExist(string type) {
			foreach (var segment in _segments) {
				if (segment.type == type) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks is a segment exists with a given pattern. This is slower than looking for it in specific chunk
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
		/// Returns the first instance of a given segment type, else an empty segment
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Segment GetSegType(string type) {
			type = type.ToUpper();
			foreach (var s in _segments) {
				if (s.type == type) {
					return s;
				}
			}

			return new Segment();
		}

		/// <summary>
		/// Returns the first instance of a given segment type, else throws an exception
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Segment GetRequiredSegType(string type) {
			type = type.ToUpper();
			foreach (var s in _segments) {
				if (s.type == type) {
					return s;
				}
			}

			throw new InvalidOperationException("Segment " + type + " Does not exist");
		}

		/// <summary>
		/// Gets all the segments of a specified type'
		/// </summary>
		/// <param name="type"></param>
		/// <returns>A list of segments</returns>
		public List<Segment> GetAllSegmentsOfType(string type) {
			List<Segment> retVal = new List<Segment>();
			type = type.ToUpper();

			foreach (var s in _segments) {
				if (s.type == type) {
					retVal.Add(s);
				}
			}

			return retVal;
		}

		/// <summary>
		/// Defines and sets the segments by type so the header, details, and trailers can be worked with
		/// </summary>
		/// <param name="itemLevelType"></param>
		/// <param name="trailerType"></param>s
		public void DefineSegmentGroups(string itemLevelType, string trailerType) {
			//0 header, 1 item, 2 trailer 
			int position = 0;
			_detailStart = itemLevelType;

			foreach (var segment in _segments) {
				if (segment.type == "ST" || segment.type == "SE") {
					continue;
				}

				if (position == 0 && segment.type == itemLevelType) {
					position = 1;
				}

				if (position == 1 && segment.type == trailerType) {
					position = 2;
				}

				switch (position) {
					//Header is everything between the ST and the first line item
					case 0:
						header.add(segment);
						break;
					case 1:
						//Starts a new line item every time the line item segment is hit
						if (segment.type == itemLevelType) {
							deatils.Add(new SegmentGroup());
						}

						deatils.Last().add(segment);
						break;
					case 2:
						trailer.add(segment);
						break;
				}
			}
		}

		/// <summary>
		/// Same as the other but if the start of the trailer isn't consistent
		/// </summary>
		/// <param name="itemLevelType"></param>
		/// <param name="trailerTypes"></param>s
		public void DefineSegmentGroups(string itemLevelType, string[] trailerTypes) {
			//0 header, 1 item, 2 trailer 
			int position = 0;
			_detailStart = itemLevelType;

			foreach (var segment in _segments) {
				if (segment.type == "ST" || segment.type == "SE") {
					continue;
				}

				if (position == 0 && segment.type == itemLevelType) {
					position = 1;
				}

				if (position == 1 && trailerTypes.Contains(segment.type)) {
					position = 2;
				}

				switch (position) {
					//Header is everything between the ST and the first line item
					case 0:
						header.add(segment);
						break;
					case 1:
						//Starts a new line item every time the line item segment is hit
						if (segment.type == itemLevelType) {
							deatils.Add(new SegmentGroup());
						}

						deatils.Last().add(segment);
						break;
					case 2:
						trailer.add(segment);
						break;
				}
			}
		}

		public bool DoesN1LoopExist(string type) {
			foreach (var segment in _segments) {
				if (segment.type == "N1" && segment.GetElement(1) == type) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the entirety of the N1 Loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		public List<Segment> getWholeN1Loop(string qualifier) {
			List<Segment> n1Loop = new List<Segment>();

			qualifier = qualifier.ToUpper();

			bool inLoop = false;

			foreach (var s in _segments) {
				if (s.type == "N1" && s.GetElement(1) == qualifier) {
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}

				if (inLoop && (s.type == "N1" || s.type == _detailStart)) {
					break;
				}

				if (inLoop) {
					n1Loop.Add(s);
				}
			}

			return n1Loop;
		}

		/// <summary>
		/// Gets the N1, N2, N3, and N4 of a given loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		public List<Segment> getN1LoopOfType(string qualifier) {
			qualifier = qualifier.ToUpper();
			if (header == new SegmentGroup()) {
				return DoN1FromDocument(qualifier);
			}
			else {
				return DoN1FromHeader(qualifier);
			}
		}

		/// <summary>
		/// this is quicker since it just runs through the header if it's defined
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		private List<Segment> DoN1FromHeader(string qualifier) {
			List<Segment> n1Loop = new List<Segment>();

			string[] types = {"N2", "N3", "N4"};

			bool inLoop = false;

			foreach (var s in header) {
				if (s.type == "N1" && s.GetElement(1) == qualifier) {
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}

				if (inLoop && types.Contains(s.type)) {
					n1Loop.Add(s);
				}
				else if (inLoop) {
					break;
				}
			}

			return n1Loop;
		}

		/// <summary>
		/// This s the old way of doing it, iterating through the whole document
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		private List<Segment> DoN1FromDocument(string qualifier) {
			List<Segment> n1Loop = new List<Segment>();

			string[] types = {"N2", "N3", "N4"};

			bool inLoop = false;

			foreach (var s in _segments) {
				if (s.type == "N1" && s.GetElement(1) == qualifier) {
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}

				if (inLoop && types.Contains(s.type)) {
					n1Loop.Add(s);
				}
				else if (inLoop) {
					break;
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

			if (header != new SegmentGroup()) {
				retVal = header.GetAllN1Loops();
			}
			else {
				string[] types = {"N1", "N2", "N3", "N4"};
				foreach (var segment in _segments) {
					if (types.Contains(segment.type)) {
						if (segment.type == "N1") {
							retVal.Add(new SegmentGroup());
						}

						retVal.Last().add(segment);
					}
				}
			}

			return retVal;
		}

		/// <summary>
		/// returns a segment with a specific pattern, null/empty/whitespaces are wildcards&#xA;
		/// It's faster to grab from the specific part of the document if you know where it is
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
				if ((s.type == "N1" && inLoop) || s.type == _detailStart) {
					break;
				}

				if (inLoop && (types.Contains(s.type) || additionFields.Contains(s.type))) {
					n1Loop.Add(s);
				}
			}

			return n1Loop;
		}

		/// <summary>
		/// Same as get n1 loop, but as a segment group
		/// </summary>
		/// <param name="qualifier"></param>
		/// <param name="additionFields"></param>
		/// <returns></returns>
		public SegmentGroup getN1LoopOfTypeSegmentGroup(string qualifier, string[] additionFields) {
			SegmentGroup n1Loop = new SegmentGroup();

			var n1s = getN1LoopOfType(qualifier, additionFields);

			foreach (var n1 in n1s) {
				n1Loop.add(n1);
			}

			return n1Loop;
		}
		
		/// <summary>
		/// Grabs every message segment in the document
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

		/// <summary>
		/// Returns a segment group
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		public SegmentGroup getN1LoopOfTypeSegmentGroup(string qualifier) {
			SegmentGroup n1Loop = new SegmentGroup();

			var n1s = getN1LoopOfType(qualifier);

			foreach (var n1 in n1s) {
				n1Loop.add(n1);
			}

			return n1Loop;
		}

		public List<Segment> Segments => _segments;

		/// <summary>
		/// Makes the segments in the document Enumerable
		/// </summary>
		/// <returns>Enumerated Segements</returns>
		public IEnumerator<Segment> GetEnumerator() {
			foreach (var segment in _segments) {
				yield return segment;
			}
		}

		#region Special ST/SE information

		public string STGetID() {
			return _ST.GetElement(2);
		}

		public string SEGetSECount() {
			return _SE.GetElement(1);
		}

		#endregion

		public override string ToString() {
			return _toString;
		}

		/// <summary>
		/// Says if the SE count is the same as the actual number of segments in the envelope
		/// </summary>
		/// <returns></returns>
		public bool DoSegemnetCountsMatch() {
			return _segments.Count == int.Parse(SEGetSECount());
		}

		void IDisposable.Dispose() {
			Dispose();
		}

		public void Dispose() {
			_ST?.Dispose();
			_SE?.Dispose();
			header?.Dispose();
			trailer?.Dispose();
			h1Loop?.Dispose();
		}
	}
}