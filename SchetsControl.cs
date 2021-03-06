﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SchetsEditor {
    public class SchetsControl : UserControl {
        private Schets schets;
        private Color penkleur;
        private int pendikte;

        public Color PenKleur {
            get { return penkleur; }
        }

        public int PenDikte {
            get { return pendikte; }
        }

        public Schets Schets {
            get { return schets; }
        }
        public SchetsControl() {
            this.BorderStyle = BorderStyle.Fixed3D;
            this.schets = new Schets();
            this.Paint += this.teken;
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }
        protected override void OnPaintBackground(PaintEventArgs e) {
        }
        private void teken(object o, PaintEventArgs pea) {
            schets.Teken(pea.Graphics);
        }
        private void veranderAfmeting(object o, EventArgs ea) {
            schets.VeranderAfmeting(this.ClientSize);
            this.Invalidate();
        }
        public Graphics MaakBitmapGraphics() {
            Graphics g = schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
        public void Schoon(object o, EventArgs ea) {
            schets.Schoon();
            this.Invalidate();
        }
        public void Roteer(object o, EventArgs ea) {
            schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
            schets.Roteer();
            this.Invalidate();
        }
        public void VeranderKleur(object obj, EventArgs ea) {
            string kleurNaam = ((ComboBox)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
        public void VeranderLijndikte(object obj, EventArgs ea) {
            string kleurNaam = ((ComboBox)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
        public void VeranderKleurViaMenu(object obj, EventArgs ea) {
            int lijndikte = int.Parse(((ToolStripMenuItem)obj).Text);
            pendikte = lijndikte;
        }

        public void TekenUitLijst() {
            schets.Schoon();
            foreach (SchetsElement element in this.schets.schetslijst) {
                if (element.Zichtbaar) {
                    penkleur = element.Kleur;
                    element.Soort.MuisVast(this, element.Beginpunt);
                    if (element.Soort.GetType() == new TekstTool().GetType()) {
                        foreach (char c in element.Tekst) {
                            element.Soort.Letter(this, c, element.Gedraaid);
                        }
                    }
                    if (element.Soort.GetType() == new PenTool().GetType()) {
                        foreach (Point p in element.penPunten) {
                            element.Soort.MuisDrag(this, p);
                        }
                    }
                    element.Soort.MuisLos(this, element.Eindpunt);
                }
            }
            this.Invalidate();
        }
    }
}
