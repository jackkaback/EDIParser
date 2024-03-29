using System;
using System.Collections.Generic;
using System.IO;

namespace EDIParser {
	public class EDIDocument {
		private Segment _ISA;
		private Segment _IEA;
		private string _toString = "";
		private List<Envelope> _envelopes = new List<Envelope>();

		private Envelope _currentEnvlope;

		/// <summary>
		/// Generates the entire EDI document
		/// </summary>
		/// <param name="file">Text of the file</param>
		public EDIDocument(string file) {
			var elementTerm = file[104];
			var subElement = file[105];
			var segTerminator = file[106];
			var lines = file.Split(segTerminator);
			List<Segment> segments = new List<Segment>();
			foreach (var line in lines) {
				Segment t = new Segment(line.Split(elementTerm), segTerminator, elementTerm);

				//Dealing with the special segments here
				if (t.type == "ISA") {
					_ISA = t;
					continue;
				}

				t.SubElementTerm = subElement;

				if (t.type == "GS") {
					_currentEnvlope = new Envelope(t);
					continue;
				}

				if (t.type == "GE") {
					_currentEnvlope.Ge = t;
					_currentEnvlope.GenerateToString();
					_envelopes.Add(_currentEnvlope);
					continue;
				}

				if (t.type == "IEA") {
					_IEA = t;
					_currentEnvlope?.Dispose();
					GenerateToString();
					continue;
				}

				segments.Add(t);

				if (t.type == "SE") {
					_currentEnvlope.addDocument(new Document(segments));
					segments = new List<Segment>();
				}
			}
		}

		/// <summary>
		/// Generates the EDI document from the streamreader, stream reader needs to be at the start
		/// </summary>
		/// <param name="reader">reader for the text</param>
		public EDIDocument(StreamReader reader) {
			string file = reader.ReadToEnd();
			reader.Close();
			var elementTerm = file[104];
			var subElement = file[105];
			var segTerminator = file[106];
			var lines = file.Split(segTerminator);
			List<Segment> segments = new List<Segment>();
			foreach (var line in lines) {
				Segment t = new Segment(line.Split(elementTerm), segTerminator, elementTerm);

				//Dealing with the special segments here
				if (t.type == "ISA") {
					_ISA = t;
					continue;
				}

				t.SubElementTerm = subElement;

				if (t.type == "GS") {
					_currentEnvlope = new Envelope(t);
					continue;
				}

				if (t.type == "GE") {
					_currentEnvlope.Ge = t;
					_currentEnvlope.GenerateToString();
					_envelopes.Add(_currentEnvlope);
					continue;
				}

				if (t.type == "IEA") {
					_IEA = t;
					_currentEnvlope?.Dispose();
					GenerateToString();
					continue;
				}

				segments.Add(t);

				if (t.type == "SE") {
					_currentEnvlope.addDocument(new Document(segments));
					segments = new List<Segment>();
				}
			}
		}

		/// <summary>
		/// Generates the entire EDI document, and can throw an error if any piece of data doesn't match
		/// </summary>
		/// <param name="file">Text of the file</param>
		/// <param name="throwError">Throw an error if the IEA count and envelope counts don't match</param>
		/// <exception cref="Exception">Counts do not match</exception>
		public EDIDocument(string file, bool throwError) {
			var elementTerm = file[104];
			var subElement = file[105];
			var segTerminator = file[106];
			var lines = file.Split(segTerminator);
			List<Segment> segments = new List<Segment>();
			foreach (var line in lines) {
				Segment t = new Segment(line.Split(elementTerm), segTerminator, elementTerm);

				//Dealing with the special segments here
				if (t.type == "ISA") {
					_ISA = t;
					continue;
				}

				t.SubElementTerm = subElement;

				if (t.type == "GS") {
					_currentEnvlope = new Envelope(t, throwError);
					continue;
				}

				if (t.type == "GE") {
					_currentEnvlope.Ge = t;
					_currentEnvlope.GenerateToString();
					_currentEnvlope.CheckError();
					_envelopes.Add(_currentEnvlope);
					continue;
				}

				if (t.type == "IEA") {
					_IEA = t;
					if (throwError && !DoEnvelopeCountsMatch()) {
						throw new Exception("IEA count and count of envelopes do not match");
					}

					_currentEnvlope?.Dispose();

					GenerateToString();
					continue;
				}

				segments.Add(t);

				if (t.type == "SE") {
					_currentEnvlope.addDocument(new Document(segments, throwError));
					segments = new List<Segment>();
				}
			}

			if (throwError && _envelopes.Count != int.Parse(IEAEnvelopeCount())) {
				throw new Exception("Envelope count does not equal the IEA count");
			}
		}

		/// <summary>
		/// Generates the entire EDI document, and can throw an error if any piece of data doesn't match
		/// </summary>
		/// <param name="reader">reader of the document</param>
		/// <param name="throwError">Throw an error if the IEA count and envelope counts don't match</param>
		/// <exception cref="Exception">Counts do not match</exception>
		public EDIDocument(StreamReader reader, bool throwError) {
			string file = reader.ReadToEnd();
			reader.Close();
			var elementTerm = file[104];
			var subElement = file[105];
			var segTerminator = file[106];
			var lines = file.Split(segTerminator);
			List<Segment> segments = new List<Segment>();
			foreach (var line in lines) {
				Segment t = new Segment(line.Split(elementTerm), segTerminator, elementTerm);

				//Dealing with the special segments here
				if (t.type == "ISA") {
					_ISA = t;
					continue;
				}

				t.SubElementTerm = subElement;

				if (t.type == "GS") {
					_currentEnvlope = new Envelope(t, throwError);
					continue;
				}

				if (t.type == "GE") {
					_currentEnvlope.Ge = t;
					_currentEnvlope.GenerateToString();
					_envelopes.Add(_currentEnvlope);
					continue;
				}

				if (t.type == "IEA") {
					_IEA = t;
					GenerateToString();
					if (throwError && !DoEnvelopeCountsMatch()) {
						throw new Exception("IEA count and count of envelopes do not match");
					}

					_currentEnvlope?.Dispose();
					continue;
				}

				segments.Add(t);

				if (t.type == "SE") {
					_currentEnvlope.addDocument(new Document(segments));
					segments = new List<Segment>();
				}
			}
		}

		public List<Envelope> GetEnvelopes() {
			return _envelopes;
		}

		/// <summary>
		/// Gets the first instance of an envelope or returns an empty envelope
		/// </summary>
		/// <param name="type">Type of GS envelope</param>
		/// <returns>Envelope of given type if it exists</returns>
		public Envelope GetEnvelopeOfType(string type) {
			type = type.ToUpper();
			foreach (var e in _envelopes) {
				if (e.type == type) {
					return e;
				}
			}

			return new Envelope();
		}

		/// <summary>
		/// Says if an Envelope exists
		/// </summary>
		/// <param name="type">Type of GS envelope</param>
		/// <returns>If the desired element exists</returns>
		public bool DoesEnvelopeExist(string type) {
			type = type.ToUpper();
			foreach (var e in _envelopes) {
				if (e.type == type) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the first instance of an envelope, or throws an error
		/// </summary>
		/// <param name="type">Type of desired envelope</param>
		/// <returns>Returns the first instance of the given envelope type</returns>
		public Envelope GetRequiredEnvelope(string type) {
			type = type.ToUpper();
			foreach (var e in _envelopes) {
				if (e.type == type) {
					return e;
				}
			}

			throw new Exception("Envelope does not exist");
		}

		#region ISA Information

		public string ISAAuthorizationQlf() {
			return _ISA.GetElement(1).Trim();
		}

		public string ISAAuthorizationInfo() {
			return _ISA.GetElement(2).Trim();
		}

		public string ISASecurityQlf() {
			return _ISA.GetElement(3).Trim();
		}

		public string ISASecurityInfo() {
			return _ISA.GetElement(4).Trim();
		}

		public string ISASenderQlf() {
			return _ISA.GetElement(5).Trim();
		}

		public string ISASenderID() {
			return _ISA.GetElement(6).Trim();
		}

		public string ISARecieverQlf() {
			return _ISA.GetElement(7).Trim();
		}

		public string ISARecieverID() {
			return _ISA.GetElement(8).Trim();
		}

		public string ISADate() {
			return _ISA.GetElement(9).Trim();
		}

		public string ISATime() {
			return _ISA.GetElement(10).Trim();
		}

		public string ISAControlStanders() {
			return _ISA.GetElement(11).Trim();
		}

		public string ISAControlVersion() {
			return _ISA.GetElement(12).Trim();
		}

		public string ISAControlNumber() {
			return _ISA.GetElement(13).Trim();
		}

		public string ISAAcknowledgementRequested() {
			return _ISA.GetElement(14).Trim();
		}

		public string ISATestProductionIndictor() {
			return _ISA.GetElement(15).Trim();
		}

		public string subElementSeperator() {
			return _ISA.GetElement(16).Trim();
		}

		public string IEAEnvelopeCount() {
			return _IEA.GetElement(1);
		}

		#endregion

		/// <summary>
		/// Makes the envelopes in the ansi document Enumerable
		/// </summary>
		/// <returns></returns>
		public IEnumerator<Envelope> GetEnumerator() {
			foreach (var envelope in _envelopes) {
				yield return envelope;
			}
		}

		/// <summary>
		/// gets the Envelope at the indexed position
		/// </summary>
		/// <param name="index"></param>
		public Envelope this[int index] {
			get => _envelopes[index];
		}

		/// <summary>
		/// For doing for loops rather than foreach, same as envelope count, but more convention name
		/// </summary>
		/// <returns></returns>
		public int Length() {
			return _envelopes.Count;
		}

		public int EnvelopeCount() {
			return _envelopes.Count;
		}

		/// <summary>
		/// Says if the IEA count is the same as the actual number of envelops in the whole documents
		/// </summary>
		/// <returns></returns>
		public bool DoEnvelopeCountsMatch() {
			return _envelopes.Count == int.Parse(IEAEnvelopeCount());
		}

		public Segment Isa => _ISA;
		public Segment Iea => _IEA;

		private void GenerateToString() {
			_toString = _ISA + "\r\n";

			foreach (var e in _envelopes) {
				_toString += e.ToString();
			}

			_toString += _IEA + "\r\n";
		}

		public override string ToString() {
			return _toString;
		}

		public void Dispose() {
			_ISA?.Dispose();
			_IEA?.Dispose();
			_currentEnvlope?.Dispose();
		}
	}
}