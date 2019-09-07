using System.Collections.Generic;

namespace EDIParser
{
	public class EDIDocument
	{
		private Segment ISA;
		private Segment GS;
		private List<Segment> segs;
		private List<Document> Docs = new List<Document>();

		public EDIDocument(string file)
		{
			var elementTerm = file[105];
			var lines = file.Split(file[107]);

			foreach (var line in lines)
			{
				Segment t = new Segment(line.Split(elementTerm));

				if (t.type == "ISA")
				{
					ISA = t;
					continue;
				}
				if (t.type == "GS")
				{
					GS = t;
					continue;
				}

				segs.Add(t);

				if (t.type == "SE")
				{
					Docs.Add(new Document(segs, GS, ISA));
					segs = new List<Segment>();
				}
			}
		}
	}
}