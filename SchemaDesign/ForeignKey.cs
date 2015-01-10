using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SchemaDesign
{
	internal class ForeignKey : Column
	{
		private readonly string tableName;
		private readonly string columnName;
		public Column Link { get; private set; }
		public int Style { get; private set; }

		public ForeignKey(Table table, XElement element)
			: base(table, element)
		{
			tableName = element.Attribute("table").Value;
			columnName = element.Attribute("column").Value;
			Name = GetDefaultName();// string.IsNullOrEmpty(Name) ? ToString() : Name;
			Style = element.ParseInt("style");
		}

		public string GetDefaultName()
		{
			return string.Join("_", tableName, columnName);
		}


		public void Connect(IEnumerable<Column> columns)
		{
			Link = columns.Single(c => string.Equals(c.Table.Name, tableName, StringComparison.InvariantCultureIgnoreCase)
									   && string.Equals(c.Name, columnName, StringComparison.InvariantCultureIgnoreCase));
			if (DataType == DataType.None)
			{
				DataType = Link.DataType;
				if (DataType == DataType.AutoInt)
				{
					DataType = DataType.Int;
				}
			}

		}

		public override XElement Serialise()
		{
			var element = base.Serialise();
			if (string.Equals(element.Value, GetDefaultName()))
			{
				element.Nodes().OfType<XText>().Remove();
			}
			if (DataType == Link.DataType || (DataType == DataType.Int && Link.DataType == DataType.AutoInt))
			{
				var attr = element.Attribute("type");
				if (attr != null)
				{
					attr.Remove();
				}
			}
			element.Name = "fk";
			element.Add(new XAttribute("table", Link.Table.Name));
			element.Add(new XAttribute("column", Link.Name));
			return element;
		}
	}
}
