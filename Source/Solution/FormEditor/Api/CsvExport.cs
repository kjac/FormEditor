// nicked this one from http://stackoverflow.com/questions/2422212/simple-c-sharp-csv-excel-export-class
// - cleaned it up a bit for code standard

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace FormEditor.Api
{
	/// <summary>
	/// Simple CSV export
	/// Example:
	///   CsvExport myExport = new CsvExport();
	///
	///   myExport.AddRow();
	///   myExport["Region"] = "New York, USA";
	///   myExport["Sales"] = 100000;
	///   myExport["Date Opened"] = new DateTime(2003, 12, 31);
	///
	///   myExport.AddRow();
	///   myExport["Region"] = "Sydney \"in\" Australia";
	///   myExport["Sales"] = 50000;
	///   myExport["Date Opened"] = new DateTime(2005, 1, 1, 9, 30, 0);
	///
	/// Then you can do any of the following three output options:
	///   string myCsv = myExport.Export();
	///   myExport.ExportToFile("Somefile.csv");
	///   byte[] myCsvData = myExport.ExportToBytes();
	/// </summary>
	public class CsvExport
	{
		/// <summary>
		/// To keep the ordered list of column names
		/// </summary>
		private List<string> _fields = new List<string>();

		/// <summary>
		/// The list of rows
		/// </summary>
		private List<Dictionary<string, object>> _rows = new List<Dictionary<string, object>>();

		/// <summary>
		/// The current row
		/// </summary>
		private Dictionary<string, object> CurrentRow { get { return _rows[_rows.Count - 1]; } }

		/// <summary>
		/// Set a value on this column
		/// </summary>
		public object this[string field]
		{
			set
			{
				// Keep track of the field names, because the dictionary loses the ordering
				if (!_fields.Contains(field))
				{
					_fields.Add(field);
				}
				CurrentRow[field] = value;
			}
		}

		/// <summary>
		/// Call this before setting any fields on a row
		/// </summary>
		public void AddRow()
		{
			_rows.Add(new Dictionary<string, object>());
		}

		/// <summary>
		/// Converts a value to how it should output in a csv file
		/// If it has a comma, it needs surrounding with double quotes
		/// Eg Sydney, Australia -> "Sydney, Australia"
		/// Also if it contains any double quotes ("), then they need to be replaced with quad quotes[sic] ("")
		/// Eg "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
		/// </summary>
		string MakeValueCsvFriendly(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			if (value is INullable && ((INullable)value).IsNull)
			{
				return string.Empty;
			}
			if (value is DateTime)
			{
				if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
				{
					return ((DateTime)value).ToString("yyyy-MM-dd");
				}
				return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
			}
			string output = value.ToString();
			if (output.IndexOfAny(new char[] { '"', ',', ';', '\n', '\r' }) != -1)
			{
				output = '"' + output.Replace("\"", "\"\"") + '"';
			}
			return output;
		}

		/// <summary>
		/// Output all rows as a CSV returning a string
		/// </summary>
		public string Export()
		{
			StringBuilder sb = new StringBuilder();

			// The header
			foreach (string field in _fields)
			{
				sb.Append(field).Append(";");
			}
			sb.AppendLine();

			// The rows
			foreach (Dictionary<string, object> row in _rows)
			{
			    var delimiter = Configuration.Instance.Delimiter;
				foreach (var field in _fields)
				{
					if (row.ContainsKey(field))
					{
						sb.Append(MakeValueCsvFriendly(row[field])).Append(delimiter);
					}
					else
					{
						sb.Append(string.Empty).Append(delimiter);
					}
				}
				sb.AppendLine();
			}

			return sb.ToString();
		}

		/// <summary>
		/// Exports as raw UTF8 bytes
		/// </summary>
		public byte[] ExportToBytes()
		{
			return Encoding.UTF8.GetBytes(Export());
		}
	}
}
