using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDIParser
{
	public class Document
	{
		/// <summary>
		/// The document type express as the number
		/// </summary>
		public readonly string DocumentType;

		private Segment _ST;
		private Segment _SE;
		private string _toString;

		private List<Segment> _segments;
//		private List<List<Segment>> _details;
		private string _detailStart;

		public SegmentGroup header;
		public List<SegmentGroup> deatils;
		public SegmentGroup trailer;
		
		public Document(List<Segment> segments)
		{
			this._segments = segments;
			DocumentType = segments[0].type;
			_ST = segments[0];
			_SE = segments.Last();

			GenerateToString();
		}

		private void GenerateToString()
		{
			_toString = "";

			foreach (var s in _segments)
			{
				_toString += s.ToString() + "\r\n";
			}
		}

		public bool DoesSegExist(string type)
		{
			foreach (var segment in _segments)
			{
				if (segment.type == type)
				{
					return true;
				}
			}

			return false;
		}

//		public bool DoesSegExistFromPattern(string type, params string[] pattern)
//		{
//			foreach (var segment in segments)
//			{
//				throw Exception NotImplementedException;
//			}
//
//			return false;
//		}

		/// <summary>
		/// Returns the first instance of a given segment type, else an empty segment
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Segment GetSegType(string type)
		{
			type = type.ToUpper();
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
		/// Returns the first instance of a given segment type, else throws an exception
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Segment GetRequiredSegType(string type)
		{
			type = type.ToUpper();
			foreach (var s in _segments)
			{
				if (s.type == type)
				{
					return s;
				}
			}

			throw new InvalidOperationException("Segment " + type + " Does not exist");
		}

		/// <summary>
		/// Get's all the segments of a specified type'
		/// </summary>
		/// <param name="type"></param>
		/// <returns>A list of segments</returns>
		public List<Segment> GetAllSegmentsOfType(string type)
		{
			List<Segment> retVal = new List<Segment>();
			type = type.ToUpper();

			foreach (var s in _segments)
			{
				if (s.type == type)
				{
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
		public void DefineSegmentGroups(string itemLevelType, string trailerType)
		{
			//0 header, 1 item, 2 trailer 
			int position = 0;
			_detailStart = itemLevelType;
			
			foreach (var segment in _segments)
			{
				if (segment.type == "ST" || segment.type == "SE")
				{
					continue;
				}

				if (position == 0 && segment.type == itemLevelType)
				{
					position = 1;
				}

				if (position == 1 && segment.type == trailerType)
				{
					position = 2;
				}


				switch (position)
				{
					case 0:
						header.add(segment);
						break;
					case 1:
						if (segment.type == itemLevelType)
						{
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

		public bool DoesN1LoopExist(string type)
		{
			foreach (var segment in _segments)
			{
				if (segment.type == "N1" && segment.GetElement(1) == type)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get's the entirety of the N1 Loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		public List<Segment> getWholeN1Loop(string qualifier)
		{
			List<Segment> n1Loop = new List<Segment>();

			qualifier = qualifier.ToUpper();

			bool inLoop = false;

			foreach (var s in _segments)
			{
				if (s.type == "N1" && s.GetElement(1) == qualifier)
				{
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}

				if (inLoop && (s.type == "N1" || s.type == _detailStart))
				{
					break;
				}

				if (inLoop)
				{
					n1Loop.Add(s);
				}
			}

			return n1Loop;
		}

		/// <summary>
		/// Get's the N1, N2, N3, and N4 of a given loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		public List<Segment> getN1LoopOfType(string qualifier)
		{
			qualifier = qualifier.ToUpper();
			if (header == new SegmentGroup())
			{
				return DoN1FromDocument(qualifier);
			}
			else
			{
				return DoN1FromHeader(qualifier);
			}
		}

		/// <summary>
		/// this is quicker since it just runs through the header if it's defined
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		private List<Segment> DoN1FromHeader(string qualifier)
		{
			List<Segment> n1Loop = new List<Segment>();
             
			string[] types = new[] {"N2", "N3", "N4"};

			bool inLoop = false;
             
			foreach (var s in header)
			{
				if (s.type == "N1" && s.GetElement(1) == qualifier)
				{
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}
             
				if (inLoop && types.Contains(s.type))
				{
					n1Loop.Add(s);
				}
				else if (inLoop)
				{
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
		private List<Segment> DoN1FromDocument(string qualifier)
		{
			List<Segment> n1Loop = new List<Segment>();
            
			string[] types = new[] {"N2", "N3", "N4"};

			bool inLoop = false;
            
			foreach (var s in _segments)
			{
				if (s.type == "N1" && s.GetElement(1) == qualifier)
				{
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}
            
				if (inLoop && types.Contains(s.type))
				{
					n1Loop.Add(s);
				}
				else if (inLoop)
				{
					break;
				}
			}
            
			return n1Loop;
		}
		
		/// <summary>
		/// returns a segment with a specific pattern, null/empty/whitespaces are wildcards&#xA;
		/// It's faster to grab from the specific part of the document if you know where it is
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
		/// Get's the N1, N2, N3, N4 and addition fields of a given loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <param name="additionFields"></param>
		/// <returns></returns>
		public List<Segment> getN1LoopOfType(string qualifier, string[] additionFields)
		{
			List<Segment> n1Loop = new List<Segment>();

			string[] types = new[] {"N2", "N3", "N4"};

			qualifier = qualifier.ToUpper();

			bool inLoop = false;

			foreach (var s in _segments)
			{
				if (s.type == "N1" && s.GetElement(1) == qualifier)
				{
					n1Loop.Add(s);
					inLoop = true;
					continue;
				}

				//You've gone to far
				if ((s.type == "N1" && inLoop) || s.type == _detailStart)
				{
					break;
				}

				if (inLoop && (types.Contains(s.type) || additionFields.Contains(s.type)))
				{
					n1Loop.Add(s);
				}
			}

			return n1Loop;
		}

		public List<Segment> Segments => _segments;

		/// <summary>
		/// Makes the segments in the document Enumerable
		/// </summary>
		/// <returns></returns>
		public IEnumerator<Segment> GetEnumerator()
		{
			foreach (var segment in _segments)
			{
				yield return segment;
			}
		}

		#region Special ST/SE information

		public string STGetID()
		{
			return _ST.GetElement(2);
		}
		
		public string SEGetSECount()
		{
			return _SE.GetElement(1);
		}

		#endregion

		public override string ToString()
		{
			return _toString;
		}

		/// <summary>
		/// Says if the SE count is the same as the actual number of segments in the envelope
		/// </summary>
		/// <returns></returns>
		public bool DoSegemnetCountsMatch()
		{
			return _segments.Count == int.Parse(SEGetSECount());
		}
	}
}