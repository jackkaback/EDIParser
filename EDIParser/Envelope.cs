using System.Collections;
using System.Collections.Generic;

namespace EDIParser {
	public class Envelope {
		private Segment _GS;
		private Segment _GE;
		private string _toString = "";

		/// <summary>
		/// The document type expressed as the GS type
		/// </summary>
		public readonly string type;

		private List<Document> _Docs = new List<Document>();

		public Envelope(Segment gs) {
			_GS = gs;
			type = _GS.GetElement(1);
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
		/// <returns></returns>
		public IEnumerator<Document> GetEnumerator() {
			foreach (var doc in _Docs) {
				yield return doc;
			}
		}

		#region Special GS/GE information

		public string GSGetEnvelopeID() {
			return _GS.GetElement(6);
		}

		public string GEDocumentCount() {
			return Ge.GetElement(1);
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
	}
}