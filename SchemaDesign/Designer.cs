using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SchemaDesign
{
	internal class Designer : Control
	{
		private readonly List<Table> tables = new List<Table>();
		private string path;

		public void Load(string file)
		{
			path = file;
			try
			{
				var newTables = Parse(file).ToArray();
				foreach (var table in tables)
				{
					table.Moved -= OnMoved;
					Controls.Remove(table);
				}
				tables.Clear();
				foreach (var table in newTables)
				{
					tables.Add(table);
					Controls.Add(table);
					table.Moved += OnMoved;
				}
				Save();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "SchemaDesign");
			}
		}

		private static IEnumerable<Table> Parse(string file)
		{
			var schemas = new XmlSchemaSet();
			schemas.Add("", "schema.xsd");
			var schema = XDocument.Load(file);
			schema.Validate(schemas, (o, e) => { throw e.Exception; });
			var tables = (schema.Root.Elements().Select(element => new Table(element)) ?? Enumerable.Empty<Table>()).ToList();
			var columns = tables.SelectMany(t => t.Columns).ToList();
			foreach (var column in columns.OfType<ForeignKey>())
			{
				column.Connect(columns);
			}
			return tables;
		}

		private void OnMoved(object sender, EventArgs e)
		{
			Invalidate();
			Save();
		}

		public void Save()
		{
			if (path == null)
			{
				return;
			}
			var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
				new XElement("schema", tables.Select(t => t.Serialise())));
			doc.Save(path);
			var str = new StringBuilder();
			foreach (var table in tables)
			{
				table.Build(str);
			}
			File.WriteAllText(Path.ChangeExtension(path, ".sql"), str.ToString());
			var bmp = new Bitmap(Width, Height);
			DrawToBitmap(bmp, ClientRectangle);
			bmp.Save(Path.ChangeExtension(path, ".png"), ImageFormat.Png);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			foreach (var fk in tables.SelectMany(c => c.Columns).OfType<ForeignKey>())
			{
				var l2r = fk.Table.Left < fk.Link.Table.Left;
				var start = new Point(l2r ? fk.Table.Right : fk.Table.Left, fk.Table.Top + (fk.Table.Columns.IndexOf(fk) + 1) * Table.ColumnHeight + Table.ColumnHeight / 2);
				var end = new Point(l2r ? fk.Link.Table.Left : fk.Link.Table.Right, fk.Link.Table.Top + (fk.Link.Table.Columns.IndexOf(fk.Link) + 1) * Table.ColumnHeight + Table.ColumnHeight / 2);
				var diff = (start.X - end.X) / 2;
				var bend = fk.Style != 1 && Math.Abs(start.Y - end.Y) > 50 && Math.Abs(diff) > 20;
				if (bend)
				{
					var m1 = new Point(start.X + diff * (l2r ? 1 : -1), start.Y);
					var m2 = new Point(m1.X, end.Y);
					e.Graphics.DrawLine(Pens.Black, start, m1);
					e.Graphics.DrawLine(Pens.Black, m1, m2);
					e.Graphics.DrawLine(Pens.Black, m2, end);
				}
				else
				{
					e.Graphics.DrawLine(Pens.Black, start, end);
				}
				e.Graphics.FillEllipse(Brushes.Black, end.X - 4, end.Y - 4, 8, 8);
			}
		}
	}
}