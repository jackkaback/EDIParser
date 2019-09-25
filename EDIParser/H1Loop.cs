using System;
using System.Collections.Generic;
using System.Linq;

namespace EDIParser {
	public class H1Loop{
		private SegmentGroup _segements = new SegmentGroup();
		private int _level;
		private int _parent;
		public string type;
		private string _tostring;
		private Segment _thisH1;
		private List<H1Loop> _innerH1Loop =new List<H1Loop>();

		public H1Loop(List<Segment> segs) {
			if (segs[0].type != "H1") {
				throw new Exception("First Item should be an H1");
			}

			_thisH1 = segs[0];

			_level = int.Parse(segs[0][1]);
			if (!string.IsNullOrWhiteSpace(segs[0][2])) {
				_parent = int.Parse(segs[0][2]);
			}
			else {
				_parent = 0;
			}
			
			type = segs[0][3];

			segs.Remove(segs[0]);
			foreach (var seg in segs) {
				if (seg.type == "H1") {
					if (seg[2] == _parent.ToString()){
						_innerH1Loop.Add(new H1Loop(segs));
						foreach (var usedSeg in _innerH1Loop.Last().Segements) {
							segs.Remove(usedSeg);
						}
					}
					else {
						return;
					}
				}

				else {
					_segements.add(seg);
					segs.Remove(seg);
				}
			}
		}
		
		public IEnumerator<H1Loop> GetEnumerator() {
			foreach (var H1 in _innerH1Loop) {
				yield return H1;
			}
		}
		public SegmentGroup Segements => _segements;

		public override string ToString() {
			_tostring = _thisH1.ToString() + "\r\n";
			foreach (var segement in _segements) {
				_tostring += segement.ToString() + "\r\n";
			}

			foreach (var h1Loop in _innerH1Loop) {
				_tostring += h1Loop.ToString() + "\r\n";
			}
			
			return _tostring;
		}
	}
}