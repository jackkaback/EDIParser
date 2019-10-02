using System;
using System.Collections.Generic;

namespace EDIParser {
	public class Envelope : IDisposable {
		private Segment _GS;
		private Segment _GE;
		private string _toString = "";
		private bool _throwError;

		/// <summary>
		/// The document type expressed as the GS type
		/// </summary>
		public readonly string type;

		private List<Document> _Docs = new List<Document>();

		/// <summary>
		/// Generates the envelope at first
		/// </summary>
		/// <param name="gs"></param>
		public Envelope(Segment gs) {
			_GS = gs;
			type = _GS.GetElement(1);
		}

		/// <summary>
		/// Generates the envelope and can throw an error if the counts don't match
		/// </summary>
		/// <param name="gs"></param>
		/// <param name="throwError"></param>
		public Envelope(Segment gs, bool throwError) {
			_GS = gs;
			type = _GS.GetElement(1);

			_throwError = throwError;
		}

		/// <summary>
		/// For making a Null object
		/// </summary>
		public Envelope() {
		}

		public void addDocument(Document d) {
			_Docs.Add(d);
		}

		public Segment Ge {
			get => _GE;
			set => _GE = value;
		}

		public Segment Gs => _GS;

		public List<Document> GetDocuments() {
			return _Docs;
		}

		/// <summary>
		/// Makes the document in the envelope Enumerable
		/// </summary>
		/// <returns>Enumerated Documents</returns>
		public IEnumerator<Document> GetEnumerator() {
			foreach (var doc in _Docs) {
				yield return doc;
			}
		}

		/// <summary>
		/// gets the document at position index
		/// </summary>
		/// <param name="index"></param>
		public Document this[int index] {
			get => _Docs[index];
		}

		/// <summary>
		/// For doing for loops rather than foreach
		/// </summary>
		public int Length() {
			return _Docs.Count;
		}

		#region Special GS/GE information

		public string GSGetEnvelopeID() {
			return _GS.GetElement(6);
		}

		public string GEDocumentCount() {
			return _GE.GetElement(1);
		}

		#endregion

		public override string ToString() {
			return _toString;
		}

		public void GenerateToString() {
			_toString = _GS + "\r\n";

			foreach (var d in _Docs) {
				_toString += d.ToString();
			}

			_toString += _GE + "\r\n";
		}

		/// <summary>
		/// Says if the GE count is the same as the actual number of documents in the envelope
		/// </summary>
		/// <returns></returns>
		public bool DoDocumentCountsMatch() {
			return _Docs.Count == int.Parse(GEDocumentCount());
		}

		/// <summary>
		/// Throws an error if the throw flag is set and document counts don't match
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void CheckError() {
			if (_throwError && !DoDocumentCountsMatch()) {
				throw new Exception("GE count and document count's do not match'");
			}
		}

		void IDisposable.Dispose() {
			Dispose();
		}

		public void Dispose() {
			_GS?.Dispose();
			_GE?.Dispose();
		}
	}
}