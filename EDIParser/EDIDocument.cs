using System;
using System.Collections;
using System.Collections.Generic;

namespace EDIParser
{
	public class EDIDocument
	{
		private Segment ISA;
		private Segment IEA;
		private Segment GS;
		private List<Envelope> envelopes = new List<Envelope>();
		private List<Segment> segments = new List<Segment>();

		private Envelope currentEnvlope;

		public EDIDocument(string file)
		{
			var elementTerm = file[104];
			var segterminator = file[106];
			var lines = file.Split(segterminator);

			foreach (var line in lines)
			{
				Segment t = new Segment(line.Split(elementTerm));

				//Dealing with the special segments here
				if (t.type == "ISA")
				{
					ISA = t;
					continue;
				}
				if (t.type == "GS")
				{
					currentEnvlope = new Envelope(t);
					continue;
				}

				if (t.type == "GE")
				{
					currentEnvlope.Ge = t;
					envelopes.Add(currentEnvlope);
				}

				if (t.type == "IEA")
				{
					IEA = t;
					continue;
				}

				segments.Add(t);

				if (t.type == "SE")
				{
					currentEnvlope.addDocument(new Document(segments));
					segments = new List<Segment>();
				}
			}
		}
		
		public List<Envelope> GetEnvelopes()
		{
			return envelopes;
		}

		/// <summary>
		/// Get's the first instance of an envelope or returns an empty envelope
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Envelope GetEnvelopeOfType(string type)
		{
			type = type.ToUpper();
			foreach (var e in envelopes)
			{
				if (e.type == type)
				{
					return e;
				}
			}
			
			return new Envelope();
		}
		
		/// <summary>
		/// Get's the first instance of an envelope, or throws an error
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Envelope GetRequiredEnvelope(string type)
		{
			type = type.ToUpper();
			foreach (var e in envelopes)
			{
				if (e.type == type)
				{
					return e;
				}
			}
			throw new InvalidOperationException("Envelope " + type + " Does not exist");
		}

		#region ISA Information

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

		#endregion

		/// <summary>
		/// Makes the envelopes in the ansi document Enumerable
		/// </summary>
		/// <returns></returns>
		public IEnumerator<Envelope> GetEnumerator()
		{
			foreach (var envelope in envelopes)
			{
				yield return envelope;
			}
		}

	}
}