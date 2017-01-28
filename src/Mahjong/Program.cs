#define REGULAR

using System;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Text.RegularExpressions;
//using System.Data.OleDb;
//using System.Data;

namespace Mahjong
{

	static class Program
	{
		public const int TUILE_Z = 7;		// décalage des tuiles en fonction de la profondeur
		public const int TUILE_L = 44;      // largeur tuile
		public const int TUILE_H = 54;      // hauteur tuile
		public const int TUILE_OFF = 4 * TUILE_Z;	// décalage des tuiles par rapport au plateau
		public const int X_STEP = TUILE_L - TUILE_Z;
		public const int Y_STEP = TUILE_H - TUILE_Z;
		public const int NB_TYPE = 42;      // nombre de motifs différents
		public static int MAP_L;			// largeur du plateau (en tuile)
		public static int MAP_H;			// hauteur du plateau (en tuile)
		public static int MAP_Z;			// nombre d'étages
		public static string[] BuildVer;	// version du programme
		public static string MapName;		// nom du plateau

		// définition des tuiles (type,nombres,valeur)
		public static int[,] Def_Tuiles = {
			// Bambous
			{0,4,6},{1,4,6},{2,4,6},{3,4,6},{4,4,6},{5,4,6},{6,4,6},{7,4,6},{8,4,6},
			// caracteres
			{9,4,6},{10,4,6},{11,4,6},{12,4,6},{13,4,6},{14,4,6},{15,4,6},{16,4,6},{17,4,6},
			// cercles
			{18,4,6},{19,4,6},{20,4,6},{21,4,6},{22,4,6},{23,4,6},{24,4,6},{25,4,6},{26,4,6},
			// dragons
			{27,4,10},{28,4,10},{29,4,10},
			// fleurs
			{30,1,12},{30,1,12},{30,1,12},{30,1,12},
			// saisons
			{31,1,12},{31,1,12},{31,1,12},{31,1,12},
			// vents
			{32,4,8},{33,4,8},{34,4,8},{35,4,8}
		};

		// définition du plateau
		public static bool[, ,] LevelMap;	 // map du plateau
		#region
		/*
        {   // 1er etage
        {true,true,true,true,true,true,true,true,true,true},
        {true,true,true,true,true,true,true,true,true,true},
        {true,true,true,true,true,true,true,true,true,true},
        {true,true,true,true,true,true,true,true,true,true},
        {true,true,true,true,true,true,true,true,true,true},
        {true,true,true,true,true,true,true,true,true,true},
        {true,true,true,true,true,true,true,true,true,true},
        {true,true,true,true,true,true,true,true,true,true}
        },
        {   // 2eme etage
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,true,true,true,true,true,true,false,false},
        {false,true,true,true,true,true,true,true,true,false},
        {false,true,true,true,true,true,true,true,true,false},
        {false,true,true,true,true,true,true,true,true,false},
        {false,true,true,true,true,true,true,true,true,false},
        {false,false,true,true,true,true,true,true,false,false},
        {false,false,false,false,false,false,false,false,false,false}
        },
        {   // 3eme etage
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,true,true,true,true,false,false,false},
        {false,false,false,true,true,true,true,false,false,false},
        {false,false,false,true,true,true,true,false,false,false},
        {false,false,false,true,true,true,true,false,false,false},
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false}
        },
        {   // 4eme etage
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,true,true,false,false,false,false},
        {false,false,false,false,true,true,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false},
        {false,false,false,false,false,false,false,false,false,false}
        }
        };
 */
		#endregion

		public static Tuile[, ,] GameMap;    // position des tuiles sur le plateau

		// messages d'erreur du fichier de plateau
		public static string[] ErrMessage = {
			"Fichier Map Manquant !",
			"Erreur de syntaxe sur taille",
			"Taille trop grande",
			"Trop de couche",
			"Largeur d'une couche invalide",
			"Hauteur d'une couche invalide",
			"Nombre de Tuiles incorrecte"
		};


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// extraction de la version
			string v = Assembly.GetExecutingAssembly().FullName;
			v = v.Substring( v.IndexOf( "=" ) );
			BuildVer = (v.Substring( 1, v.IndexOf( "," ) - 1 )).Split( new char[] { '.' } );

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new Form1() );
		}

#if REGULAR
		// lecture et init du fichier map
		public static int Load_Plateau( string map )
		{
			int couche = -1;
			int Largeur = 0, Hauteur = 0;
			int err = -1;
			int y = 0, x;
			int nb = 0;
			Match m;
			bool[, ,] Map={{{false}}};
			//string connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Classeur1.xls;" + "extended Properties=Excel 8.0";

			try
			{
				//OleDbConnection oConn = new OleDbConnection( connStr );
				//OleDbDataAdapter oDA = new OleDbDataAdapter( "SELECT * FROM [Feuil1$]",oConn );
				//DataSet oDS = new DataSet();
				//oDA.Fill( oDS,"Feuille" );

				StreamReader sr = new StreamReader( map );
				string s = sr.ReadLine();
				if( s == null ) return 0;	// vide!!!
				MapName = "Plateau sans nom";		// init nom du plateau
				do
				{
					s = s.Trim();
					if( s.StartsWith( ";" ) ) continue;	// commentaire
					else if( s.StartsWith( "nom" ) )		// nom du plateau
					{
						Regex r = new Regex( "\"([^\"]+)\"" );
						m = r.Match( s );				// recherche: texte entre guillemets
						if( m.Success )
						{
							MapName = "Plateau " + m.Groups[1].Value;
						}
					}
					else if( s.StartsWith( "taille" ) )		// taille du plateau
					{
						Regex r = new Regex( @"=\s*(?<larg>\d{1,2})\s*,\s*(?<haut>\d{1,2})" );
						m = r.Match( s );			// recherche: nombre,nombre
						if( m.Success )
						{
							Largeur = Convert.ToInt32( m.Groups["larg"].Value );
							Hauteur = Convert.ToInt32( m.Groups["haut"].Value );
							if( Largeur > 16 || Hauteur > 16 ) err = 2;
							else Map = new bool[4, Hauteur, Largeur];	// creation de la map du plateau
						}
						else err = 1;					// erreur de syntaxe sur la taille
					}
					else if( s.StartsWith( "[couche]" ) )	// entete couche (étage)
					{
						if( couche < 4 )				// ok pour nouvelle couche
						{
							y = 0;
							couche++;					// nouvelle couche
						}
						else err = 3;					// erreur
					}
					else if( s.StartsWith( "0" ) || s.StartsWith( "." ) )	// ligne de couche
					{
						x = 0;
						if( Largeur == 0 || Largeur != s.Length )		// taille du plateau invalide
						{
							err = 4;
							continue;
						}
						foreach( char c in s )	// scan la ligne
						{
							if( c == '0' )
							{
								Map[couche, y, x++] = true;
								if( nb++ > 144 ) err = 6;	// trop de tuile
							}
							else Map[couche, y, x++] = false;
						}
						if( y++ >= Hauteur ) err = 5;		// hauteur de la couche invalide; 
					}
				} while( (s = sr.ReadLine()) != null && err < 0 );	// ligne suivante

				if( err<0 && nb != 144 ) err = 6;
				if( err < 0 && Largeur>0)		// init du plateau
				{
					MAP_L = Largeur;
					MAP_H = Hauteur;
					MAP_Z = couche + 1;
					LevelMap = (bool[, ,])Map.Clone();
				}
				sr.Close();
				return err;
			} catch( FileNotFoundException )
			{
				return 0;
			}
		}
#else
		// lecture et init du fichier map
		public static int Load_Plateau(string map)
		{
			int couche = -1;
			int Largeur = 0, Hauteur=0;
			int err = -1;
			int y = 0,x,x1;
			int nb = 0;

			try
			{
				StreamReader sr = new StreamReader(map);
				string s = sr.ReadLine();
				if( s == null ) return 0;	// vide!!!
				MapName = "Plateau sans nom";		// init nom du plateau
				do{
					s = s.Trim();
					if(s.StartsWith(";"))	continue;	// commentaire
					else if(s.StartsWith("nom"))		// nom du plateau
					{
						x = s.IndexOf('"');
						x1 = s.LastIndexOf('"');
						if( x > 0 && x1>0 && x!=x1)
						{
							s = s.Substring(x+1, x1-x-1);
							MapName = "Plateau " + s;
						}
					}
					else if( s.StartsWith("taille") )		// taille du plateau
					{
						x = s.IndexOf("=");				// recherche le égale
						if( x < 0 ) err = 1;			// pas de =
						else
						{
							s = s.Substring(x + 1);		// chaine apres le =
							x = s.IndexOf(",");			// recherche le separateur
							if( x < 0 ) err = 1;		// pas de séparateur
							else
							{
								if( !Int32.TryParse(s.Substring(0, x), out Largeur) )
								{
									err = 1;			// largeur pas lisible
									continue;
								}
								else if( !Int32.TryParse(s.Substring(x + 1), out Hauteur) )
								{
									err = 1;
									continue;			// hauteur pas lisible
								}
							}
							if( Largeur >16 || Hauteur >16 )	err = 2;
							else LevelMap = new bool[4,Hauteur, Largeur];	// creation de la map du plateau
						}
					}
					else if( s.StartsWith("[couche]") )	// entete couche (étage)
					{
						if( couche < 4  )		// ok pour nouvelle couche
						{
							y = 0;
							couche++;					// nouvelle couche
						}
						else err = 3;				// erreur
					}
					else if( s.StartsWith("0") || s.StartsWith(".") )	// ligne de couche
					{
						x = 0;
						if( Largeur == 0 || Largeur != s.Length )		// taille du plateau invalide
						{
							err = 4;
							continue;
						}
						foreach( char c in s )	// scan la ligne
						{
							if( c == '0' )
							{
								LevelMap[couche, y, x++] = true;
								if(nb++ > 144)	err = 6;		// trop de tuile
							}
							else LevelMap[couche, y, x++] = false;
						}
						if( y++ >= Hauteur ) err = 5;		// hauteur de la couche invalide; 
					}
				} while( ( s = sr.ReadLine() ) != null && err<0 );	// ligne suivante

				if( err <0)		// init dimension du plateau
				{
					if( nb != 144 ) err = 6;
					MAP_L = Largeur;
					MAP_H = Hauteur;
					MAP_Z = couche+1;
				}
				sr.Close();
				return err;
			}
			catch( FileNotFoundException )
			{
				return 0;
			}
		}
#endif
	}
}