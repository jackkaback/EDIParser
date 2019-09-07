using System;
using System.Collections.Generic;

namespace EDIParser
{
	public class Document
	{
		
		/// <summary>
		/// ISA header
		/// </summary>
		private Segment ISA;
		/// <summary>
		/// GS header
		/// </summary>
		private Segment GS;
		/// <summary>
		/// The document type express as the number
		/// </summary>
		public readonly string DocumentType;

		private List<Segment> segs;
		private List<List<Segment>> Details;
		private string detailStart;

		public Document(List<Segment> segs, Segment gs, Segment ISA)
		{
			this.segs = segs;
			DocumentType = segs[0].type;
			
			GS = gs;
			this.ISA = ISA;
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

		public string DetailStart
		{
			set => detailStart = value;
		}

		public string ISAAuthorizationQlf()
		{
			return ISA.GetElement(1).Trim();
		}
		public string ISAAuthorizationInfo()
		{
			return ISA.GetElement(2).Trim();
		}
		public string ISASecurityQlf()
		{
			return ISA.GetElement(3).Trim();
		}
		public string ISASecurityInfo()
		{
			return ISA.GetElement(4).Trim();
		}
		public string ISASenderQlf()
		{
			return ISA.GetElement(5).Trim();
		}
		public string ISASenderID()
		{
			return ISA.GetElement(6).Trim();
		}
		public string ISARecieverQlf()
		{
			return ISA.GetElement(7).Trim();
		}
		public string ISARecieverID()
		{
			return ISA.GetElement(8).Trim();
		}
		public string ISADate()
		{
			return ISA.GetElement(9).Trim();
		}
		public string ISATime()
		{
			return ISA.GetElement(10).Trim();
		}
		public string ISAControlStanders()
		{
			return ISA.GetElement(11).Trim();
		}
		public string ISAControlVersion()
		{
			return ISA.GetElement(12).Trim();
		}
		public string ISAControlNumber()
		{
			return ISA.GetElement(13).Trim();
		}
		public string ISAAcknowledgementRequested()
		{
			return ISA.GetElement(14).Trim();
		}
		public string ISATestProductionIndictor()
		{
			return ISA.GetElement(15).Trim();
		}
		public string subElementSeperator()
		{
			return ISA.GetElement(16).Trim();
		}
	}
}