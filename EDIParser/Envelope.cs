using System.Collections;
using System.Collections.Generic;

namespace EDIParser
{
	public class Envelope
	{
		private Segment GS;
		private Segment GE;

		/// <summary>
		/// The document type expressed as the GS type
		/// </summary>
		public readonly string type;

		private List<Document> Docs = new List<Document>();

		public Envelope(Segment gs)
		{
			GS = gs;
			type = GS.GetElement(1);
		}

		/// <summary>
		/// For making a Null object
		/// </summary>
		public Envelope()
		{
		}

		public void addDocument(Document d)
		{
			Docs.Add(d);
		}

		public Segment Ge
		{
			get => GE;
			set => GE = value;
		}

		public Segment Gs
		{
			get => GS;
			set => GS = value;
		}

		public List<Document> GetDocuments()
		{
			return Docs;
		}

		/// <summary>
		/// Makes the document in the envelope Enumerable
		/// </summary>
		/// <returns></returns>
		public IEnumerator<Document> GetEnumerator()
		{
			foreach (var doc in Docs)
			{
				yield return doc;
			}
		}

		#region Special GS/GE information

		public string GEDocumentCount()
		{
			return Ge.GetElement(1);
		}

		#endregion

		public override string ToString()
		{
			string retval = GS + "\r\n";

			foreach (var d in Docs)
			{
				retval += d.ToString();
			}

			retval += GE + "\r\n";
			return retval;
		}
	}
}