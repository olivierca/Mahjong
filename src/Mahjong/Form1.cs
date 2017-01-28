using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Mahjong
{
	public enum NotifyType { STOPSOLUCE, PAIRE }

	public struct Tuile_Type
	{
		public ImageList ImgMotif; // motif de la tuile
		public int Valeur;         // valeur de la tuile
		public int Nb;             // nombre de tuiles de ce type
		public int Type;           // type de la tuile
	}

	public partial class Form1 : Form
	{
		List<ArrayList> Tuiles;			// liste des tuiles indéxé par type pairable
		public Tuile_Type[] Col_tuile;	// liste des types de tuiles (Motif)
		Random Rnd;
		int Nb_Tuiles;					// nombre de tuiles sur le plateau
		int Nb_Libre = 0;				// emplacements libre sur le plateau;
		int GameScore;					// score de la partie
		Tuile[] Solution;				// paire de tuiles en solution
		bool[, ,] Cur_Map;				// map courante
		public Region rtuile;

		public Form1()
		{
			InitializeComponent();
		}

		// initialisation du Jeux
		#region
		void NewGame()
		{
			int id = 0;
			Tuile tuile;

			Plateau.Visible = false;
			// ATTENTION les anciens controls des tuiles doivent être enlevé
			if( Nb_Tuiles > 0 && Program.GameMap != null )		// si il reste des tuiles
			{
				// vide l'ancien plateau
				foreach( Tuile t in Program.GameMap )           // recherche les tuiles sur le plateau
				{
					if( t != null )
					{
						Plateau.Controls.Remove( t );			// retire le control windows
						if( --Nb_Tuiles == 0 ) break;			// plus de tuiles
					}
				}
			}

			// init la Map (type de plateau)
			Cur_Map = (bool[, ,])Program.LevelMap.Clone();
			Program.GameMap = new Tuile[Program.MAP_Z, Program.MAP_H, Program.MAP_L];   // creation du plateau

			Tuiles = new List<ArrayList>();
			GameScore = 0;
			Tuile.Sel_Tuile = null;
			Nb_Tuiles = 0;

			// creation des tuiles
			foreach( Tuile_Type t in Col_tuile )
			{
				if( Tuiles.Count <= t.Type ) Tuiles.Add( new ArrayList() );
				for( int i = 0; i < t.Nb; i++ )
				{
					tuile = new Tuile( this, t, id++ );
					Tuiles[t.Type].Add( tuile );		// ajoute a la liste
					Plateau.Controls.Add( tuile );		// ajoute aux controls windows
					Nb_Tuiles++;
				}
			}

			//  place les tuiles sur le plateau de jeu
			Init_Plateau();
		}

		// place les tuiles sur le plateau
		void Init_Plateau()
		{
			Tuile tuile;
			int rep = 0;

			bMelange.Enabled = false;
			bNew.Enabled = false;
			lMessage.Text = "";
			Plateau.Visible = false;
			SuspendLayout();

			do
			{
				// vide le plateau
				for( int z = 0; z < Program.MAP_Z; z++ )
					for( int y = 0; y < Program.MAP_H; y++ )
						for( int x = 0; x < Program.MAP_L; x++ ) Program.GameMap[z, y, x] = null;

				Nb_Libre = Nb_Tuiles;

				// place la liste des tuiles sur le plateau
				foreach( ArrayList t in Tuiles )
				{
					rep = Place_Type( t );
					if( rep <= 0 ) break;
				}
			} while( (rep < 0 || !Check_Solution()) && Nb_Libre > 0 );	// si pas de solution ou echec alors on recommence

			// Arrange l'ordre d'affichage des tuiles
			for( int z = 0; z < Program.MAP_Z; z++ )
				for( int y = 0; y < Program.MAP_H; y++ )
					for( int x = 0; x < Program.MAP_L; x++ )
					{
						tuile = Program.GameMap[z, y, x];
						if( tuile != null ) tuile.BringToFront();
					}

			ResumeLayout( false );
			PerformLayout();
			Plateau.Visible = true;
			UpdateGame();
		}

		// place les tuiles d'un type sur le plateau
		// retourne -1 si placement impossible, 0 si terminé, 1 si tuiles placées
		int Place_Type( ArrayList list )
		{
			bool ok;

			foreach( Tuile tuile in list )   // liste les tuiles
			{
				if( tuile != null )
				{
					do
					{
						ok = Place_Tuile( tuile );
					} while( !ok && Nb_Libre > 2 );
					if( !ok ) return -1;				// placement pas possible
					if( --Nb_Libre == 0 ) return 0;		// placement terminé
				}
			}
			return 1;
		}

		// place la tuile sur le plateau
		bool Place_Tuile( Tuile tuile )
		{
			int x, y, z;
			Tuile t;

			// recherche emplacement libre
			do
			{
				x = Rnd.Next( Program.MAP_L );
				y = Rnd.Next( Program.MAP_H );
				z = Rnd.Next( Program.MAP_Z );
			} while( Program.GameMap[z, y, x] != null || !Cur_Map[z, y, x] );

			for( int i = 0; i < Program.MAP_Z; i++ )
			{
				// pas de superpositions de paires
				t = Program.GameMap[i, y, x];
				if( t != null && (int)t.Tag == (int)tuile.Tag ) return false;
			}
			Program.GameMap[z, y, x] = tuile;              // ok place la tuile
			tuile.Set( x, y, z );
			return true;
		}
		#endregion

		// gestion du jeux        
		#region
		void UpdateGame()
		{
			// mise a jour du score
			Score.Text = "Score    " + GameScore.ToString();

			if( Nb_Tuiles == 0 )	// si gagné
			{
				melangerToolStripMenuItem.Enabled = false;
				bSolution.Enabled = false;
				bNew.Enabled = true;
				lMessage.Text = "C'est Gagné !";
			}
			// vérifie solutions
			else if( !Check_Solution() )
			{
				// plus de solution
				melangerToolStripMenuItem.Enabled = true;
				bSolution.Enabled = false;
				bNew.Enabled = true;
				bMelange.Enabled = true;
				lMessage.Text = "Plus de Solution !";
			}
		}

		// notification d'une tuile
		public void TuileNotify( object sender, NotifyType n )
		{
			if( n == NotifyType.STOPSOLUCE ) Stop_Solution();	// arret du clignotement
			else if( n == NotifyType.PAIRE )						// paire validé
			{
				Tuiles_Found( (Tuile)sender );
			}
		}

		// paire de tuilles trouvé
		void Tuiles_Found( Tuile tuile )
		{
			// enléve les tuiles trouvé
			Kill_Tuile( tuile );
			Kill_Tuile( Tuile.Sel_Tuile );
			Tuile.Sel_Tuile = null;
			tuile = null;

			UpdateGame();
		}

		// enleve la tuile du jeu
		void Kill_Tuile( Tuile tuile )
		{
			GameScore += tuile.Valeur;      // mise a jour du score
			// elimine de la liste des tuiles
			Tuiles[(int)tuile.Tag].Remove( tuile );
			// retire du plateau de jeux
			Program.GameMap[tuile.z, tuile.y, tuile.x] = null;
			Cur_Map[tuile.z, tuile.y, tuile.x] = false;
			// retire des controls windows
			Plateau.Controls.Remove( tuile );
			Nb_Tuiles--;
		}

		bool Check_Solution()
		{

			timer1.Stop();
			// vérifie la liberté des tuiles
			foreach( Tuile tuile in Program.GameMap )
			{
				if( tuile != null ) tuile.CheckFree();
			}

			foreach( ArrayList list in Tuiles )			// liste les tuiles par type
			{
				Solution[0] = Solution[1] = null;
				foreach( Tuile tuile in list )			// liste les tuiles du même type
				{
					if( tuile!= null && tuile.libre )
					{
						if( Solution[0] == null ) Solution[0] = tuile;  // une solution possible
						else
						{
							Solution[1] = tuile;		// solution trouvée
							bSolution.Enabled = true;
							return true;
						}
					}
				}
			}
			return false;
		}

		// stop le clignotement de la solution
		void Stop_Solution()
		{
			timer1.Stop();    // stop la solution courante
			if( Solution[1] != null )
			{
				if( Tuile.Sel_Tuile != Solution[0] ) Solution[0].ImageIndex = 0;
				if( Tuile.Sel_Tuile != Solution[1] ) Solution[1].ImageIndex = 0;
			}
			bSolution.Enabled = true;
		}
		#endregion

		// initialisations diverses
		private void Form1_Load( object sender, EventArgs e )
		{
			// lecture et initialisation de la map du plateau
			int err = Program.Load_Plateau( "jonglevel.txt" );
			if( err >= 0 )
			{
				// erreur d'initialisation de la map -> on quitte
				MessageBox.Show( Program.ErrMessage[err], "Erreur de fichier MahJong" );
				Close();
				return;
			}

			Ajuste_Fenetres();		// ajuste la taille de la fenetre en fonction un plateau

			BuildTuileSet();

			Rnd = new Random();
			Solution = new Tuile[2];
			NewGame();

		}

		// construction des types de tuile
		private void BuildTuileSet()
		{
			// construction de la liste des tuiles
			Col_tuile = new Tuile_Type[Program.NB_TYPE];

			// creation de la liste des types de tuile
			for( int i = 0; i < Program.NB_TYPE; i++ )
			{
				Col_tuile[i].ImgMotif = new ImageList( this.components );
				Col_tuile[i].ImgMotif.ImageSize = new Size( Program.TUILE_L, Program.TUILE_H );
				Col_tuile[i].ImgMotif.ColorDepth = ColorDepth.Depth16Bit;
				Col_tuile[i].ImgMotif.Images.Add( (Image)Properties.Resources.ResourceManager.GetObject( "tuile" + (i + 1) + "_0" ) );
				Col_tuile[i].ImgMotif.Images.Add( (Image)Properties.Resources.ResourceManager.GetObject( "tuile" + (i + 1) + "_1" ) );
				Col_tuile[i].ImgMotif.Images.Add( (Image)Properties.Resources.ResourceManager.GetObject( "tuile" + (i + 1) + "_2" ) );
				Col_tuile[i].ImgMotif.TransparentColor = Color.Black;
				Col_tuile[i].Type = Program.Def_Tuiles[i, 0];
				Col_tuile[i].Nb = Program.Def_Tuiles[i, 1];
				Col_tuile[i].Valeur = Program.Def_Tuiles[i, 2];
			}

			// creation de la région de tracage des tuiles (Boutons en polygone)
			GraphicsPath gp = new GraphicsPath();
			int x = Program.TUILE_L;
			int y = Program.TUILE_H;
			int p = Program.TUILE_Z; int o = 1;
			int x1 = x - Program.TUILE_Z;
			int y1 = y - Program.TUILE_Z;
			Point[] points = { new Point( o, o ), new Point( x1, o ), new Point( x, p ), new Point( x, y ), new Point( p, y ), new Point( o, y1 ) };
			gp.AddPolygon( points );
			Tuile.rTuile = new Region( gp );
		}

		// ajuste la taille de la fenetre en fonction du plateau
		void Ajuste_Fenetres()
		{
			// init taille du plateau
			int l = Program.MAP_L * Program.TUILE_L + Program.TUILE_OFF;
			int h = Program.MAP_H * Program.TUILE_H + Program.TUILE_OFF;

			// ajuste taille de la fenetre
			this.Size = new Size( l + panel1.Width + 40, h + lMessage.Height + 60 );
			int zl = Size.Width - panel1.Width;		// taille zone plateau
			int zh = Size.Height - lMessage.Height;
			lMessage.Size = new Size( l, lMessage.Height );
			lMessage.Location = new Point( (zl - l) / 2, lMessage.Top );

			Plateau.Size = new Size( l, h );
			Plateau.Location = new Point( (zl - l) / 2, ((zh - h) / 2) + lMessage.Height );

			Text = "MahJong de l'Olive, " + Program.MapName;
		}

		// gestion des evenements
		#region
		private void nouvellePartieToolStripMenuItem_Click( object sender, EventArgs e )
		{
			NewGame();
		}

		private void melangerToolStripMenuItem_Click( object sender, EventArgs e )
		{
			// redispose les pieces sur le plateau
			Init_Plateau();
		}

		private void quitterToolStripMenuItem_Click( object sender, EventArgs e )
		{
			Close();
		}

		// gestion du clignotement de la solution
		private void timer1_Tick( object sender, EventArgs e )
		{
			switch( (string)timer1.Tag )
			{
				case "1":
				Solution[0].ImageIndex = 0;
				Solution[1].ImageIndex = 0;
				timer1.Tag = "0";
				break;
				case "0":
				Solution[0].ImageIndex = 1;
				Solution[1].ImageIndex = 1;
				timer1.Tag = "1";
				break;
			}
		}

		private void bSolution_Click( object sender, EventArgs e )
		{
			timer1.Start();     // demande de solution
			GameScore -= 30;
			bSolution.Enabled = false;
		}

		private void bMelange_Click( object sender, EventArgs e )
		{
			// redispose les pieces sur le plateau
			Init_Plateau();
		}

		private void bNew_Click( object sender, EventArgs e )
		{
			NewGame();
		}

		private void nouveauPlateauToolStripMenuItem_Click( object sender, EventArgs e )
		{
			// la selection du fichier
			OpenFileDialog sel_file = new OpenFileDialog();
			sel_file.Title = "Charger un fichier plateau";
			sel_file.Filter = "Fichier plateau|*.txt";
			sel_file.InitialDirectory = Environment.CurrentDirectory;
			sel_file.CheckFileExists = true;
			sel_file.AddExtension = true;
			DialogResult res = sel_file.ShowDialog();
			if( res == DialogResult.Cancel ) return;

			// charge nouveau plateau
			int err = Program.Load_Plateau( sel_file.FileName );
			if( err >= 0 )
			{
				// erreur d'initialisation de la map -> on quitte
				MessageBox.Show( Program.ErrMessage[err], "Erreur de fichier MahJong" );
				nouvellePartieToolStripMenuItem.Enabled = false;
				//Close();
				return;
			}
			nouvellePartieToolStripMenuItem.Enabled = true;
			Ajuste_Fenetres();
			NewGame();
		}

		private void aProposToolStripMenuItem_Click( object sender, EventArgs e )
		{
			About Adial = new About();
			Adial.ShowDialog();
		}

		#endregion

	}
}