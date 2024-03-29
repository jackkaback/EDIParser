using System;
using System.Collections.Generic;
using System.Globalization;

namespace EDIParser {
	public class Segment {
		public readonly string type;

		private char _elementTerm;
		private char _segterminator;
		private char _subElementTerm;

		/// <summary>
		/// Only set after using the constructor with the sub element feature or calling SegHasSubElelments
		/// </summary>
		public bool hasSubelements;

		private List<string> _elements = new List<string>();

		/// <summary>
		/// Constructor with only the elements
		/// </summary>
		/// <param name="values">List of elements</param>
		public Segment(string[] values) {
			type = values[0];

			foreach (var i in values) {
				_elements.Add(i);
			}
		}

		/// <summary>
		/// Constructor with elements, segterminator, and element terminator
		/// </summary>
		/// <param name="values">List of elements</param>
		/// <param name="segterm">Segment terminator</param>
		/// <param name="eleterm">Element terminator</param>
		public Segment(string[] values, char segterm, char eleterm) {
			type = values[0];
			_elementTerm = eleterm;
			_segterminator = segterm;

			foreach (var i in values) {
				_elements.Add(i);
			}
		}

		/// <summary>
		/// Constructor with elements, segterminator, element terminator, and subElement terminator
		/// </summary>
		/// <param name="values">List of elements</param>
		/// <param name="segterm">Segment terminator</param>
		/// <param name="eleterm">Element terminator</param>
		/// <param name="subElement">Sub element terminator</param>
		public Segment(string[] values, char segterm, char eleterm, char subElement) {
			type = values[0];
			_elementTerm = eleterm;
			_segterminator = segterm;
			_subElementTerm = subElement;

			SegHasSubElelments();

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
		/// <param name="i">Element desired</param>
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
		/// <param name="index">Element number desired</param>
		public string this[int index] {
			get => GetElement(index);
		}

		public char SubElementTerm {
			set => _subElementTerm = value;
		}

		public int Length() {
			return _elements.Count;
		}

		public int Count() {
			return _elements.Count;
		}

		/// <summary>
		/// Used to tell if a segment contains a specific string
		/// </summary>
		/// <param name="value">Desired substring</param>
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
		/// Used to tell if a segment contains a specific string
		/// </summary>
		/// <param name="values">Desired substring</param>
		/// <returns></returns>
		public bool SegContainsValues(params string[] values) {
			foreach (var e in _elements) {
				foreach (var value in values) {
					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(e, value, CompareOptions.IgnoreCase) >= 0) {
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Finds the first instance where the value exists
		/// </summary>
		/// <param name="value">Desired substring</param>
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
		/// <param name="value">Desired substring</param>
		/// <returns>Indexes of all found values</returns>
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

		/// <summary>
		/// Returns the positions of all elements where any value is a match
		/// </summary>
		/// <param name="values">Desired substring</param>
		/// <returns>Indexes of all found values</returns>
		public int[] SegContainsValuesAtAddresses(params string[] values) {
			List<int> retVals = new List<int>();

			foreach (var e in _elements) {
				if (_elements.IndexOf(e) == 0) {
					continue;
				}

				foreach (var value in values) {
					if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(e, value, CompareOptions.IgnoreCase) >= 0) {
						retVals.Add(_elements.IndexOf(e));
						break;
					}
				}
			}

			return retVals.ToArray();
		}

		/// <summary>
		/// checks if this segment has subelements
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public bool SegHasSubElelments() {
			if (_subElementTerm == '\0') {
				throw new Exception("No sub element terminator exists");
			}

			foreach (var element in _elements) {
				if (element.Contains(_subElementTerm.ToString())) {
					hasSubelements = true;
					return true;
				}
			}

			hasSubelements = false;
			return false;
		}

		/// <summary>
		/// checks if this segment has subelements
		/// </summary>
		/// <param name="sub">Subelement separator</param>
		/// <returns></returns>
		public bool SegHasSubElelments(char sub) {
			_subElementTerm = sub;
			return SegHasSubElelments();
		}

		/// <summary>
		/// Finds all elements with sub elements
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public List<int> PositionOfAllSubElements() {
			if (_subElementTerm == '\0') {
				throw new Exception("No sub element terminator exists");
			}

			List<int> retval = new List<int>();
			foreach (var element in _elements) {
				if (SegContains(_subElementTerm.ToString())) {
					retval.Add(_elements.IndexOf(element));
				}
			}

			return retval;
		}

		/// <summary>
		/// Finds all elements with sub elements
		/// </summary>
		/// <param name="sub">Subelement separator</param>
		/// <returns></returns>
		public List<int> PositionOfAllSubElements(char sub) {
			_subElementTerm = sub;
			return PositionOfAllSubElements();
		}

		/// <summary>
		/// Returns all the elements in a segment that has sub elements
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>	
		public List<string> GetAllElementWithSubElements() {
			if (_subElementTerm == '\0') {
				throw new Exception("No sub element terminator exists");
			}

			List<string> retval = new List<string>();
			foreach (var element in _elements) {
				if (SegContains(_subElementTerm.ToString())) {
					retval.Add(element);
				}
			}

			return retval;
		}

		/// <summary>
		/// Splits an element by the segment terminator
		/// </summary>
		/// <param name="element">Specific element with subelement separator</param>
		/// <returns></returns>
		public String[] SplitWithSubelement(int element) {
			var retVal = _elements[element].Split(_subElementTerm);

			return retVal;
		}

		public List<string> GetAllElementWithSubElements(char sub) {
			_subElementTerm = sub;
			return GetAllElementWithSubElements();
		}

		public void Dispose() {
		}
	}
}