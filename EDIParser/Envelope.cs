using System.Collections.Generic;

namespace EDIParser
{
	public class Envelope
	{
		private Segment GS;
		private Segment GE;
		public readonly string type;
		private List<Document> Docs = new List<Document>();

		public Envelope(Segment gs)
		{
			GS = gs;
			type = GS.GetElement(1);
		}
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
	}
}