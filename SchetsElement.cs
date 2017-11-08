using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor {
    public class SchetsElement {

        //Elementen in deze lijst bevatten de volgende gegevens: 
        //soort, beginpunt, eindpunt, kleur, eventuele tekst 
        //(misschien ook een layer indicator zodat we kunnen 
        //kijken welk element in een hogere layer zit en 
        //dan slechts een verwijderen als meerdere ‘raak’ zijn)
        ISchetsTool soort;
        Point beginpunt;
        Point eindpunt;
        Color kleur;
        string tekst;
        List<Point> penpunten;
        bool zichtbaar = true;

        public ISchetsTool Soort { get { return soort; } set { soort = value; } }
        public Point Beginpunt { get { return beginpunt; } set { beginpunt = value; } }
        public Point Eindpunt { get { return eindpunt; } set { eindpunt = value; } }
        public Color Kleur { get { return kleur; } set { kleur = value; } }
        public string Tekst { get { return tekst; } set { tekst = value; } }
        public List<Point> penPunten { get { return penpunten; } set { penpunten = value; } }
        public bool Zichtbaar { get { return zichtbaar; } set { zichtbaar = value; } }
        //int SchetsLayer { get; set; }
		
		public SchetsElement(ISchetsTool t_soort, Point t_beginpunt, Point t_eindpunt, Color t_kleur, string t_tekst) {
            soort = t_soort;
            beginpunt = t_beginpunt;
            eindpunt = t_eindpunt;
            kleur = t_kleur;
            tekst = t_tekst;
        }
        public SchetsElement(ISchetsTool t_soort, Point t_beginpunt, Point t_eindpunt, Color t_kleur) {
            soort = t_soort;
            beginpunt = t_beginpunt;
            eindpunt = t_eindpunt;
            kleur = t_kleur;
        }

        public SchetsElement(ISchetsTool t_soort, Point t_beginpunt, Color t_kleur, string t_tekst) {
            soort = t_soort;
            beginpunt = t_beginpunt;
            kleur = t_kleur;
            tekst = t_tekst;
        }
        public SchetsElement(ISchetsTool t_soort, Point t_beginpunt, Color t_kleur) {
            soort = t_soort;
            beginpunt = t_beginpunt;
            kleur = t_kleur;
            //if (soort.GetType() == new PenTool().GetType()) {
            penpunten = new List<Point>();
            //}
        }

        public SchetsElement() {
        }
    }
}