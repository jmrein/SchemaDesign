using System;
using System.Globalization;
using System.Xml.Linq;

namespace SchemaDesign
{
	internal class Column
	{
		public Table Table { get; private set; }
		public string Name { get; set; }
		public bool Nullable { get; private set; }
		public bool PrimaryKey { get; private set; }
		public bool Unique { get; private set; }
		public DataType DataType { get; set; }

		public Column(Table table, XElement element)
		{
			Table = table;
			Name = element.Value;
			Nullable = element.ParseBool("null");
			PrimaryKey = element.ParseBool("pk");
			Unique = element.ParseBool("unique");
			DataType = ParseDataType(element.Attribute("type"));
		}

		private static DataType ParseDataType(XAttribute attribute)
		{
			if (attribute == null)
			{
				return DataType.None;
			}
			return (DataType)Enum.Parse(typeof(DataType), attribute.Value, true);
		}

		public override string ToString()
		{
			return string.Join("_", Table.Name, Name);
		}

		public virtual XElement Serialise()
		{
			var element = new XElement("column", new XAttribute("type", DataType.ToString().ToLower()), new XText(Name));
			if (PrimaryKey)
			{
				element.Add(new XAttribute("pk", "true"));
			}
			if (Nullable)
			{
				element.Add(new XAttribute("null", "true"));
			}
			if (Unique)
			{
				element.Add(new XAttribute("unique", "true"));
			}
			return element;
		}

		public string TranslatedDataType()
		{
			switch (DataType)
			{
				case DataType.AutoInt:
					return "INTEGER PRIMARY KEY AUTOINCREMENT";
				case DataType.Int:
					return "INTEGER";
				case DataType.Blob:
					return "BLOB";
				case DataType.Numeric:
					return "NUMERIC";
				case DataType.Text:
					return "TEXT";
				default:
					throw new ArgumentException("No data type specified in " + ToString(), ToString());
			}
		}
	}
}
