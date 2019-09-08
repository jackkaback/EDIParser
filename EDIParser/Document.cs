using System;
using System.Collections.Generic;

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

		public string DetailStart
		{
			set => detailStart = value;
		}
	}
}