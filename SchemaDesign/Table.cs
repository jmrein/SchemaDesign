using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SchemaDesign
{
	internal class Table : Control
	{
		public List<Column> Columns { get; private set; }
		private Point? dragStart;
		public event EventHandler Moved;
		public const int ColumnHeight = 14;

		public Table(XElement element)
		{
			Name = element.Attribute("name").Value;
			Columns = element.Elements().Select(child =>
			{
				if (string.Equals(child.Name.LocalName, "column"))
				{
					return new Column(this, child);
				}
				if (string.Equals(child.Name.LocalName, "fk"))
				{
					return new ForeignKey(this, child);
				}
				return null;
			}).ToList();
			var left = element.Attribute("x");
			var top = element.Attribute("y");
			if (left != null)
			{
				Left = int.Parse(left.Value);
			}
			if (top != null)
			{
				Top = int.Parse(top.Value);
			}

			Width = 150;
			Height = (Columns.Count + 1) * ColumnHeight;
			MouseDown += (o, e) =>
			{
				if (e.Button == MouseButtons.Left)
				{
					dragStart = e.Location;
				}
			};
			MouseMove += (o, e) =>
			{
				if (e.Button == MouseButtons.Left && dragStart != null)
				{
					Left = e.X + Left - dragStart.Value.X;
					Top = e.Y + Top - dragStart.Value.Y;
					if (Moved != null)
					{
						Moved(this, new EventArgs());
					}
				}
			};
			MouseUp += (o, e) =>
			{
				if (dragStart != null && Moved != null)
				{
					Moved(this, new EventArgs());
				}
				dragStart = null;
			};
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.Clear(Color.White);
			Draw(e.Graphics, 0, 0);
		}

		public void Draw(Graphics g, int x, int y)
		{
			g.DrawRectangle(Pens.Black, new Rectangle(x, y, Width - 1, Height - 1));
			g.DrawString(Name, Font, Brushes.Black, x, y);
			g.DrawLine(Pens.Black, x, y + ColumnHeight, x + Width, y + ColumnHeight);
			y += ColumnHeight;
			foreach (var column in Columns)
			{
				if (column.PrimaryKey || column.DataType == DataType.AutoInt)
				{
					g.FillEllipse(Brushes.Black, x + 3, y + 3, ColumnHeight - 6, ColumnHeight - 6);
				}
				g.DrawString(column.Name, Font, column.Nullable ? Brushes.Gray : Brushes.Black, x + ColumnHeight, y);
				y += ColumnHeight;
			}
		}

		public virtual XElement Serialise()
		{
			var element = new XElement("table", new XAttribute("name", Name), new XAttribute("x", Left), new XAttribute("y", Top));
			foreach (var column in Columns)
			{
				element.Add(column.Serialise());
			}
			return element;
		}

		public void Build(StringBuilder str)
		{
			str.AppendFormat("CREATE TABLE `{0}` (", Name).AppendLine();
			var lines = (from column in Columns
						 let nullableText = column.Nullable ? "" : " NOT NULL"
						 let uniquenessText = column.Unique ? " UNIQUE" : ""
						 select string.Format(CultureInfo.InvariantCulture, "\t`{0}` {1}{2}{3}", column.Name, column.TranslatedDataType(), nullableText, uniquenessText))
						 .ToList();
			if (Columns.Any(c => c.PrimaryKey))
			{
				lines.Add(string.Format("\tPRIMARY KEY({0})", string.Join(", ", Columns.Where(c => c.PrimaryKey).Select(c => "`" + c.Name + "`"))));
			}
			str.AppendLine(string.Join("," + Environment.NewLine, lines));
			str.AppendLine(");");
			str.AppendLine();
		}
	}
}
