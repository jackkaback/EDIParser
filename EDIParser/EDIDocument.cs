using System;
using System.Collections;
using System.Collections.Generic;

namespace EDIParser
{
	public class EDIDocument
	{
		private Segment _ISA;
		private Segment _IEA;
		private char _elementTerm;
		private char _segTerminator;
		private string _toString = "";
		private List<Envelope> _envelopes = new List<Envelope>();
		private List<Segment> _segments = new List<Segment>();

		private Envelope _currentEnvlope;

		public EDIDocument(string file)
		{
			_elementTerm = file[104];
			_segTerminator = file[106];
			var lines = file.Split(_segTerminator);

			foreach (var line in lines)
			{
				Segment t = new Segment(line.Split(_elementTerm), _segTerminator, _elementTerm);

				//Dealing with the special segments here
				if (t.type == "ISA")
				{
					_ISA = t;
					continue;
				}

				if (t.type == "GS")
				{
					_currentEnvlope = new Envelope(t);
					continue;
				}

				if (t.type == "GE")
				{
					_currentEnvlope.Ge = t;
					_currentEnvlope.GenerateToString();
					_envelopes.Add(_currentEnvlope);
					continue;
				}

				if (t.type == "IEA")
				{
					_IEA = t;
					GenerateToString();
					continue;
				}

				_segments.Add(t);

				if (t.type == "SE")
				{
					_currentEnvlope.addDocument(new Document(_segments));
					_segments = new List<Segment>();
				}
			}
		}

		public List<Envelope> GetEnvelopes()
		{
			return _envelopes;
		}

		/// <summary>
		/// Get's the first instance of an envelope or returns an empty envelope
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Envelope GetEnvelopeOfType(string type)
		{
			type = type.ToUpper();
			foreach (var e in _envelopes)
			{
				if (e.type == type)
				{
					return e;
				}
			}

			return new Envelope();
		}

		/// <summary>
		/// Says if an Envelope exists
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool DoesEnvelopeExist(string type)
		{
			type = type.ToUpper();
			foreach (var e in _envelopes)
			{
				if (e.type == type)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Get's the first instance of an envelope, or throws an error
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Envelope GetRequiredEnvelope(string type)
		{
			type = type.ToUpper();
			foreach (var e in _envelopes)
			{
				if (e.type == type)
				{
					return e;
				}
			}

			throw new Exception("Envelope does not exist");
		}

		#region ISA Information

		public string ISAAuthorizationQlf()
		{
			return _ISA.GetElement(1).Trim();
		}

		public string ISAAuthorizationInfo()
		{
			return _ISA.GetElement(2).Trim();
		}

		public string ISASecurityQlf()
		{
			return _ISA.GetElement(3).Trim();
		}

		public string ISASecurityInfo()
		{
			return _ISA.GetElement(4).Trim();
		}

		public string ISASenderQlf()
		{
			return _ISA.GetElement(5).Trim();
		}

		public string ISASenderID()
		{
			return _ISA.GetElement(6).Trim();
		}

		public string ISARecieverQlf()
		{
			return _ISA.GetElement(7).Trim();
		}

		public string ISARecieverID()
		{
			return _ISA.GetElement(8).Trim();
		}

		public string ISADate()
		{
			return _ISA.GetElement(9).Trim();
		}

		public string ISATime()
		{
			return _ISA.GetElement(10).Trim();
		}

		public string ISAControlStanders()
		{
			return _ISA.GetElement(11).Trim();
		}

		public string ISAControlVersion()
		{
			return _ISA.GetElement(12).Trim();
		}

		public string ISAControlNumber()
		{
			return _ISA.GetElement(13).Trim();
		}

		public string ISAAcknowledgementRequested()
		{
			return _ISA.GetElement(14).Trim();
		}

		public string ISATestProductionIndictor()
		{
			return _ISA.GetElement(15).Trim();
		}

		public string subElementSeperator()
		{
			return _ISA.GetElement(16).Trim();
		}

		public string IEAEnvelopeCount()
		{
			return _IEA.GetElement(1);
		}

		#endregion

		/// <summary>
		/// Makes the envelopes in the ansi document Enumerable
		/// </summary>
		/// <returns></returns>
		public IEnumerator<Envelope> GetEnumerator()
		{
			foreach (var envelope in _envelopes)
			{
				yield return envelope;
			}
		}

		public int EnvelopeCount()
		{
			return _envelopes.Count;
		}

		/// <summary>
		/// Says if the IEA count is the same as the actual number of envelops in the whole documents
		/// </summary>
		/// <returns></returns>
		public bool DoEnvelopeCountsMatch()
		{
			return _envelopes.Count == int.Parse(IEAEnvelopeCount());
		}

		public Segment Isa => _ISA;
		public Segment Iea => _IEA;

		private void GenerateToString()
		{
			_toString = _ISA + "\r\n";

			foreach (var e in _envelopes)
			{
				_toString += e.ToString();
			}

			_toString += _IEA + "\r\n";
		}

		public override string ToString()
		{
			return _toString;
		}
	}
}