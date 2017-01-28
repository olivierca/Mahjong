using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mahjong
{
	public partial class About : Form
	{
		public About()
		{
			InitializeComponent();
		}

		private void About_Load( object sender, EventArgs e )
		{
			aVersion.Text = "Version : " + Program.BuildVer[0] + "." + Program.BuildVer[1];
			aRevision.Text = "Revision : " + Program.BuildVer[2] + "." + Program.BuildVer[3];
		}
	}
}