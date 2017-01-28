#define BOUTON

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Mahjong
{
#if BOUTON
	public class Tuile : Button
	{
		int pos_x, pos_y, pos_z;				// position sur le plateau
		bool m_libre;							// etat tuile libre oui/non
		bool m_check;							// si tuile selection
		public int x { get { return pos_x; } }	// propriete position x de la tuile sur le plateau
		public int y { get { return pos_y; } }	// propriete position x de la tuile sur le plateau
		public int z { get { return pos_z; } }	// propriete position x de la tuile sur le plateau
		public readonly int Valeur;				// valeur de la tuile
		Tuile gauche, droite, bas, haut, couvert;   // tuiles voisines
		public static Region rTuile;			// forme des tuiles
		public static Tuile Sel_Tuile;			// tuile selectionnée
		delegate void FormNotify( object sender, NotifyType e );
		event FormNotify EventForm;				// notification a la fenetre
		public bool libre						// propriete tuile libre
		{
			get { return m_libre; }
			protected set { m_libre = value; }
		}
		public bool Checked						// propriété selection de la tuile
		{
			set
			{
				m_check = value;
				ImageIndex = m_check ? 1 : 0;
			}
			get { return m_check; }
		}

		
		public Tuile( Form1 windows, Tuile_Type type, int id )
		{
			//  initialise le control
			FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			FlatAppearance.BorderSize = 0;
			ImageIndex = 0;
			ImageList = type.ImgMotif;
			Name = "Tuile" + id;
			TabStop = false;
			Size = new Size( Program.TUILE_L, Program.TUILE_H );
			Visible = false;
//			SetStyle( ControlStyles.UserPaint, true );
//			SetStyle( ControlStyles.AllPaintingInWmPaint, true );
			Region = rTuile;

			MouseLeave += new System.EventHandler( Tuile_MouseLeave );
			MouseEnter += new System.EventHandler( Tuile_MouseEnter );
			MouseDown += new MouseEventHandler( Tuile_MouseDown );
			MouseUp += new MouseEventHandler( Tuile_MouseUp );
			EventForm += new FormNotify( windows.TuileNotify );

			// initialise les variables propre aux tuiles
			gauche = droite = bas = haut = couvert = null;
			libre = false;
			m_check = false;
			Tag = type.Type;
			Valeur = type.Valeur;
		}

		// positionne la tuile sur le plateau
		public void Set( int x, int y, int z )
		{
			int decal = Program.TUILE_OFF - (z * (Program.TUILE_Z - 1));

			Location = new Point( x * Program.X_STEP + decal, y * Program.Y_STEP + decal );
			pos_x = x;
			pos_y = y;
			pos_z = z;
			Checked = false;
			Visible = true;
		}

		// vérifie si la tuile est libre
		public void CheckFree()
		{
			gauche = droite = bas = haut = couvert = null;
			libre = false;
			if( pos_z < (Program.MAP_Z - 1) ) couvert = Program.GameMap[pos_z + 1, pos_y, pos_x];
			if( pos_x > 0 ) gauche = Program.GameMap[pos_z, pos_y, pos_x - 1];
			if( pos_x < (Program.MAP_L - 1) ) droite = Program.GameMap[pos_z, pos_y, pos_x + 1];
			if( pos_y > 0 ) haut = Program.GameMap[pos_z, pos_y - 1, pos_x];
			if( pos_y < (Program.MAP_H - 1) ) bas = Program.GameMap[pos_z, pos_y + 1, pos_x];
			if( (gauche == null || droite == null) && couvert == null ) libre = true;
		}

/*
		protected override void OnPaint(PaintEventArgs pevent)
		{
			ImageList.Draw(pevent.Graphics,0,0,ImageIndex);
		}
/*
		protected override void OnPaintBackground( PaintEventArgs pevent )
		{
		}
*/
		void Tuile_MouseUp( object sender, MouseEventArgs e )
		{
			Tuile tuile = (Tuile)sender;
			if( Sel_Tuile != null )      // deja une selection
			{
				if( Sel_Tuile != tuile ) // si selection différente de la tuile courante
				{
					// annule la selection des 2 tuiles
					tuile.Checked = false;
					Sel_Tuile.Checked = false;
					Sel_Tuile = null;
				}
			}
		}

		void Tuile_MouseDown( object sender, MouseEventArgs e )
		{
			Tuile tuile = (Tuile)sender;
			EventForm( this, NotifyType.STOPSOLUCE );		// demande d'arret du clignotement de la solution
			if( tuile.libre )
			{
				if( !tuile.Checked )      // tuile non selectionnée
				{
					if( Sel_Tuile != null )  // deja une selection
					{
						if( Sel_Tuile != tuile ) // si tuiles différentes
						{
							if( (int)Sel_Tuile.Tag == (int)tuile.Tag )   // si même type
							{
								// paire valide selectionnée
								EventForm( this, NotifyType.PAIRE );
							}
							else
							{
								// mauvaise selection
								tuile.ImageIndex = 2;
								Sel_Tuile.ImageIndex = 2;
							}
						}
					}
					else
					{
						// selectionne la tuile
						tuile.Checked = true;
						Sel_Tuile = tuile;
					}
				}
				else // tuile deja selectionnée
				{
					// déselectionne la tuile
					tuile.Checked = false;
					Sel_Tuile = null;
				}
			}
		}

		void Tuile_MouseEnter( object sender, EventArgs e )
		{
			Tuile tuile = (Tuile)sender;
			if( tuile.libre )
				tuile.ImageIndex = 1;
		}

		void Tuile_MouseLeave( object sender, EventArgs e )
		{
			Tuile tuile = (Tuile)sender;
			if( !tuile.Checked )
				tuile.ImageIndex = 0;
		}
	}
#else		
    public class Tuile : Control
    {
        int     pos_x,pos_y,pos_z;					// position sur le plateau
        int     m_valeur;			                    // valeur de la tuile
        bool    m_libre;							// etat tuile libre oui/non
		int		imgindex;
		public	int x	{get { return pos_x; }}		// propriete position x de la tuile sur le plateau
		public	int y	{ get { return pos_y; } }	// propriete position x de la tuile sur le plateau
		public	int z	{ get { return pos_z; } }	// propriete position x de la tuile sur le plateau
		public	int Valeur { get { return m_valeur; } }		// propriete donne la valeur de la tuile
		public bool libre							// propriete tuile libre
		{
			get	{return m_libre;}
			protected	set {m_libre = value;}
		}

        bool	Checked;
        Tuile   gauche,droite,bas,haut,couvert;     // tuiles voisines
		public  static  Region rTuile;
        public  static	Tuile  Sel_Tuile;			// tuile selectionnée

		public int ImageIndex
		{ 
			get {return imgindex;}
			set {imgindex = value; Invalidate();}
		}
		ImageList ImageList;

        public Tuile(Tuile_Type type, int id)
        {
            //  initialise le control
			//FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			//FlatAppearance.BorderSize = 0;
			ImageIndex = 0;
			ImageList = type.ImgMotif;
            Name = "Tuile"+id;
            TabStop = false;
            Size = new Size(Program.TUILE_L, Program.TUILE_H);
            Visible = false;
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
//			Region = rTuile;

			MouseLeave += new System.EventHandler(Tuile_MouseLeave);
            MouseEnter += new System.EventHandler(Tuile_MouseEnter);
            MouseDown += new MouseEventHandler(Tuile_MouseDown);
            MouseUp += new MouseEventHandler(Tuile_MouseUp);

            // initialise les variables propre aux tuiles
            gauche = droite = bas = haut = couvert = null;
            libre = false;
			Checked = false;
			Tag = type.Type;
            m_valeur = type.Valeur;
		}

        // positionne la tuile sur le plateau
        public void Set(int x, int y, int z)
        {
			int decal = Program.TUILE_OFF - ( z * Program.TUILE_Z );
            
            Location = new System.Drawing.Point(x * Program.X_STEP + decal, y * Program.Y_STEP+decal);
            pos_x = x;
            pos_y = y;
            pos_z = z;
			Checked = false;
			Visible = true;
        }
        
        // vérifie si la tuile est libre
        public void CheckFree()
        {
			gauche = droite = bas = haut = couvert = null;
			libre = false;
            if( pos_z < ( Program.MAP_Z - 1 ) ) couvert = Program.GameMap[pos_z+1, pos_y, pos_x];
            if( pos_x > 0 )                     gauche = Program.GameMap[pos_z,pos_y, pos_x - 1];
            if( pos_x < ( Program.MAP_L - 1))   droite = Program.GameMap[pos_z,pos_y,pos_x + 1];
            if( pos_y > 0 )                     haut = Program.GameMap[ pos_z,pos_y-1,pos_x ];
            if( pos_y < ( Program.MAP_H - 1 ))  bas = Program.GameMap[ pos_z,pos_y+1,pos_x];
            if((gauche==null || droite==null) && couvert==null)   libre = true;
        }


		protected override void OnPaint(PaintEventArgs pevent)
		{
			ImageList.Draw(pevent.Graphics,0,0,ImageIndex);
		}
/*
		protected override void OnPaintBackground( PaintEventArgs pevent )
		{
		}
*/
		void Tuile_MouseUp( object sender, MouseEventArgs e )
        {
            Tuile tuile = (Tuile)sender;
            if(Sel_Tuile != null)      // deja une selection
            {
                if(Sel_Tuile != tuile) // si selection différente de la tuile courante
                {
                    // annule la selection des 2 tuiles
                    tuile.ImageIndex = 0;
                    tuile.Checked = false;
                    Sel_Tuile.Checked = false;
                    Sel_Tuile.ImageIndex = 0;
                    Sel_Tuile = null;
                }
            }
        }
        
        void Tuile_MouseDown( object sender, MouseEventArgs e )
        {            
            Tuile tuile = (Tuile)sender;
			((Form1)tuile.FindForm()).Stop_Solution();
            if( tuile.libre )
            {
                if(!tuile.Checked)      // tuile non selectionnée
                {
                    if(Sel_Tuile != null)  // deja une selection
                    {
                        if(Sel_Tuile != tuile) // si tuiles différentes
                        {
                            if((int)Sel_Tuile.Tag == (int)tuile.Tag)   // si même type
                            {
                                // paire valide selectionnée
                                ((Form1)tuile.FindForm()).Tuiles_Found(tuile);
                            }
                            else
                            {
                                // mauvaise selection
                                tuile.ImageIndex = 2;
                                Sel_Tuile.ImageIndex = 2;
                            }
                        }
                    }
                    else
                    {
                         // selectionne la tuile
                         tuile.Checked = true;
                         tuile.ImageIndex = 1;
                         Sel_Tuile = tuile;
                    }
                }
                else // tuile deja selectionnée
                {
                    // déselectionne la tuile
                    tuile.Checked = false;
                    tuile.ImageIndex = 0;
                    Sel_Tuile = null;
                }
            }
        }

        void Tuile_MouseEnter( object sender, EventArgs e )
        {
            Tuile tuile = (Tuile)sender;
			if( tuile.libre )
                tuile.ImageIndex = 1;
        }

        void Tuile_MouseLeave( object sender, EventArgs e )
        {
			Tuile tuile = (Tuile)sender;
            if( !tuile.Checked )
                tuile.ImageIndex = 0;
        }
    }
#endif
}
