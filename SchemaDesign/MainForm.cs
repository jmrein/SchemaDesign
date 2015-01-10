using System;
using System.Windows.Forms;

namespace SchemaDesign
{
	internal class MainForm : Form
	{
		private readonly Designer designer;

		public MainForm()
		{
			Controls.Add(designer = new Designer { Dock = DockStyle.Fill });
			var file = new MenuItem("&File", new[]{
				new MenuItem("&Open", Open_Click),
				new MenuItem("&Save", Save_Click),
				new MenuItem("E&xit", Exit_Click)});
			Text = "SchemaDesign";
			this.Menu = new MainMenu();
			this.Menu.MenuItems.Add(file);
		}

		private void Open_Click(object sender, EventArgs e)
		{
			var ofd = new OpenFileDialog { Filter = "Xml Schemas|*.xml" };
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				designer.Load(ofd.FileName);
			}
		}

		private void Save_Click(object sender, EventArgs e)
		{
			designer.Save();
		}

		private void Exit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}
}