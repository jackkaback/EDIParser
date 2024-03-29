using System;
using System.Collections.Generic;
using System.Linq;

namespace EDIParser {
	public class Document {
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
		public List<SegmentGroup> details = new List<SegmentGroup>();

		/// <summary>
		/// Everything in the trailer
		/// </summary>
		public SegmentGroup trailer = new SegmentGroup();

		public H1Loop h1Loop;

		/// <summary>
		/// Generates the whole transaction
		/// </summary>
		/// <param name="segments">All segments in the document</param>
		public Document(List<Segment> segments) {
			this._segments = segments;
			DocumentType = segments[0][1];

			if (segments[0].type != "ST" || segments.Last().type != "SE") {
				throw new Exception("First or last segment is not ST/SE");
			}

			if (segments.Count == 2) {
				throw new Exception("Document is empty");
			}

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
		/// <param name="segments">All segments in the document</param>
		/// <param name="ThrowError">Throw an error if the counts don't match'</param>
		public Document(List<Segment> segments, bool ThrowError) {
			this._segments = segments;
			DocumentType = segments[0][1];

			if (segments[0].type != "ST" || segments.Last().type != "SE") {
				throw new Exception("First or last segment is not ST/SE");
			}

			if (segments.Count == 2) {
				throw new Exception("Document is empty");
			}

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

		/// <summary>
		/// Checks is a given segment exists in the document
		/// </summary>
		/// <param name="type">Type of the segment</param>
		/// <returns></returns>
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
		/// <param name="type">Type of desired segment</param>
		/// <param name="pattern">additional fields in the segment</param>
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
		/// Finds the first instance of a segment
		/// </summary>
		/// <param name="type">Type of segment</param>
		/// <returns>Returns the first instance of a given segment type, else an empty segment</returns>
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
		/// <param name="type">Type of segment</param>
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
		/// <param name="type">Desired segment type</param>
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
		/// <param name="itemLevelType">The segment which is used for the line items</param>
		/// <param name="trailerType">The segment that defines being at the start of the trailer</param>s
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
							details.Add(new SegmentGroup());
						}

						details.Last().add(segment);
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
		/// <param name="itemLevelType">The segment which is used for the line items</param>
		/// <param name="trailerTypes">The segments that define being at the start of the trailer</param>s
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
							details.Add(new SegmentGroup());
						}

						details.Last().add(segment);
						break;
					case 2:
						trailer.add(segment);
						break;
				}
			}
		}

		/// <summary>
		/// Determines if a given N1 exists
		/// </summary>
		/// <param name="type">Type of N1 loop</param>
		/// <returns>If the N1 loop exists</returns>
		public bool DoesN1LoopExist(string type) {
			foreach (var segment in _segments) {
				if (segment.type == "N1" && segment.GetElement(1) == type) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the entirety of an N1 Loop
		/// </summary>
		/// <param name="qualifier">Type of N1 loop</param>
		/// <returns>List of segments containing an N1 loop</returns>
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
		/// <param name="qualifier">Type of N1 loop</param>
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
		/// <param name="qualifier">Type of N1 qualifier</param>
		/// <returns>N1 loop</returns>
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
		/// <param name="qualifier">Type of N1 qualifier</param>
		/// <returns>List of segments</returns>
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
		/// <returns>A list of segment groups with all N1 loops</returns>
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
		/// <param name="type">type of segment</param>
		/// <param name="pattern">Additional specific elements desired</param>
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
		/// <param name="qualifier">Type of N1 loop</param>
		/// <param name="additionFields">Additional segments desired</param>
		/// <returns>All desired segments from a given N1 loop</returns>
		public List<Segment> getN1LoopOfType(string qualifier, params string[] additionFields) {
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
		/// <param name="qualifier">Type of N1 loop</param>
		/// <param name="additionFields">Additional segments desired</param>
		/// <returns>All desired segments from a given N1 loop</returns>
		public SegmentGroup getN1LoopOfTypeSegmentGroup(string qualifier, params string[] additionFields) {
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
		/// <returns>All segments of types N9/MSG/MTX</returns>
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
		/// Checks the CTT count based on the count of the count of the Details
		/// </summary>
		/// <returns>-1 if different, 0 if no CTT found, 1 if the counts are the same/returns>
		public int CheckCTTCount() {
			Segment ctt = GetSegType("CTT");

			if (ctt == new Segment()) {
				return 0;
			}

			int count = int.Parse(ctt[1]);

			return count == details.Count ? 1 : -1;
		}

		/// <summary>
		/// Checks the CTT count based on the count of the countType
		/// </summary>
		/// <param name="counterType">Segment to count</param>
		/// <returns>-1 if different, 0 if no CTT found, 1 if the counts are the same/returns>
		public int CheckCTTCountDefineCounter(string counterType) {
			Segment ctt = GetSegType("CTT");

			if (ctt == new Segment()) {
				return 0;
			}

			int count = 0;
			foreach (var seg in _segments) {
				if (seg.type == counterType) {
					count++;
				}
			}
			
			int CTTCount = int.Parse(ctt[1]);
			
			return CTTCount == count ? 1 : -1;
		}

		/// <summary>
		/// Returns a segment group containing all N1 loop
		/// </summary>
		/// <param name="qualifier">Type of N1 loop</param>
		/// <returns></returns>
		public SegmentGroup getN1LoopOfTypeSegmentGroup(string qualifier) {
			SegmentGroup n1Loop = new SegmentGroup();

			var n1s = getN1LoopOfType(qualifier);

			foreach (var n1 in n1s) {
				n1Loop.add(n1);
			}

			return n1Loop;
		}

		/// <summary>
		/// Makes the segments in the document Enumerable
		/// </summary>
		/// <returns>Enumerated Segements</returns>
		public IEnumerator<Segment> GetEnumerator() {
			foreach (var segment in _segments) {
				yield return segment;
			}
		}

		public Segment GetSegment(int index) {
			if (_segments.Count > index) {
				return _segments[index];
			}

			return new Segment();
		}

		public Segment this[int index] {
			get => GetSegment(index);
		}

		public int Count() {
			return _segments.Count;
		}

		public int Length() {
			return _segments.Count;
		}

		#region Special ST/SE information

		public string STGetID() {
			return _ST.GetElement(2);
		}

		public int STGetIDInt() {
			return int.Parse(_ST.GetElement(2));
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

		public void Dispose() {
			_ST?.Dispose();
			_SE?.Dispose();
			header?.Dispose();
			trailer?.Dispose();
			h1Loop?.Dispose();
		}
	}
}