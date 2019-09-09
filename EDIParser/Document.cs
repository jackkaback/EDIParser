using System;
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

		private List<Segment> segs;
		private List<List<Segment>> Details;
		private string detailStart;

		public Document(List<Segment> segs)
		{
			this.segs = segs;
			DocumentType = segs[0].type;
		}


		/// <summary>
		/// Returns the first instance of a given segment type, else an empty segment
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Segment GetSegType(string type)
		{
			type = type.ToUpper();
			foreach (var s in segs)
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
			foreach (var s in segs)
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


			foreach (var s in segs)
			{
				if (s.type == type)
				{
					retVal.Add(s);
				}
			}

			return retVal;
		}

		/// <summary>
		/// Get's the N1, N2, N3, and N4 of a given loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <returns></returns>
		public List<Segment> getN1LoopOfType(string qualifier)
		{
			List<Segment> n1Loop = new List<Segment>();

			string[] types = new[] {"N2", "N3", "N3"};

			qualifier = qualifier.ToUpper();

			bool inLoop = false;

			foreach (var s in segs)
			{
				if (s.type == "N1" && s.GetElement(1) == qualifier)
				{
					n1Loop.Add(s);
					inLoop = true;
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
		/// Get's the N1, N2, N3, N4 and addition fields of a given loop
		/// </summary>
		/// <param name="qualifier"></param>
		/// <param name="additionFields"></param>
		/// <returns></returns>
		public List<Segment> getN1LoopOfType(string qualifier, string[] additionFields)
		{
			List<Segment> n1Loop = new List<Segment>();

			string[] types = new[] {"N2", "N3", "N3"};

			qualifier = qualifier.ToUpper();

			bool inLoop = false;

			foreach (var s in segs)
			{
				if (s.type == "N1" && s.GetElement(1) == qualifier)
				{
					n1Loop.Add(s);
					inLoop = true;
				}

				if (inLoop && (types.Contains(s.type) || additionFields.Contains(s.type)))
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

		public List<Segment> Segs => segs;

		public string DetailStart
		{
			set => detailStart = value;
		}
	}
}