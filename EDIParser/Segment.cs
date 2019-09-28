using System.Collections.Generic;
using System.Globalization;

namespace EDIParser {
	public class Segment {
		public readonly string type;

		private char _elementTerm = '*';
		private char _segterminator = '~';

		private List<string> _elements = new List<string>();

		public Segment(string[] values) {
			type = values[0];

			foreach (var i in values) {
				_elements.Add(i);
			}
		}

		public Segment(string[] values, char segterm, char eleterm) {
			type = values[0];
			_elementTerm = eleterm;
			_segterminator = segterm;

			foreach (var i in values) {
				_elements.Add(i);
			}
		}

		/// <summary>
		/// For making a null object
		/// </summary>
		public Segment() {
		}

		/// <summary>
		/// Gets the value of the element at I if it exists
		/// </summary>
		/// <param name="i"></param>
		/// <returns>The I'th element or an empty string</returns>
		public string GetElement(int i) {
			if (_elements.Count > i) {
				return _elements[i];
			}

			return string.Empty;
		}

		/// <summary>
		/// This shouldn't be used to write the data out, but rather for debugging or to write to console
		/// </summary>
		public override string ToString() {
			return string.Join(_elementTerm, _elements) + _segterminator;
		}

		/// <summary>
		/// Makes the segment enumerable
		/// </summary>
		/// <returns>An Enumerated string</returns>
		public IEnumerator<string> GetEnumerator() {
			foreach (var e in _elements) {
				yield return e;
			}
		}

		/// <summary>
		/// This is the easier way to get an element
		/// </summary>
		/// <param name="index"></param>
		public string this[int index] {
			get => GetElement(index);
		}

		/// <summary>
		/// Used to tell if a segment contains a specific string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool SegContains(string value) {
			foreach (var e in _elements) {
				if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(e, value, CompareOptions.IgnoreCase) >= 0) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Finds the first instance where the value exists
		/// </summary>
		/// <param name="value"></param>
		/// <returns>index of found value, -1 if it doesn't exist'</returns>
		public int SegContainsAtAddress(string value) {
			foreach (var e in _elements) {
				if (_elements.IndexOf(e) == 0) {
					continue;
				}
				
				if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(e, value, CompareOptions.IgnoreCase) >= 0) {
					return _elements.IndexOf(e);
				}
			}

			return -1;
		}

		/// <summary>
		/// Finds all instances of where the data exists and returns a list of their indexes
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int[] SegContainsAtAddresses(string value) {
			List<int> retVals = new List<int>();

			foreach (var e in _elements) {
				if (_elements.IndexOf(e) == 0) {
					continue;
				}
				
				if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(e, value, CompareOptions.IgnoreCase) >= 0) {
					retVals.Add(_elements.IndexOf(e));
				}
			}

			return retVals.ToArray();
		}
	}
}