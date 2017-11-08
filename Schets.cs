using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor {
    public class Schets {
        public Bitmap bitmap;
        public List<SchetsElement> schetslijst;

        public List<SchetsElement> Schetslijst { get { return schetslijst; } set { schetslijst = value; } }
        
        public Schets()
        {
            bitmap = new Bitmap(1, 1);
            schetslijst = new List<SchetsElement>();
        }

        public Graphics BitmapGraphics {
            get { return Graphics.FromImage(bitmap); }
        }

        public void VeranderAfmeting(Size sz) {
            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height) {
                Bitmap nieuw = new Bitmap(Math.Max(sz.Width, bitmap.Size.Width)
                                         , Math.Max(sz.Height, bitmap.Size.Height)
                                         );
                Graphics gr = Graphics.FromImage(nieuw);
                gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
                gr.DrawImage(bitmap, 0, 0);
                bitmap = nieuw;
            }
        }

        public void nieuwBitmap() {
            bitmap = new Bitmap(bitmap.Size.Width, bitmap.Size.Height);
        }

        public void Teken(Graphics gr) {
            gr.DrawImage(bitmap, 0, 0);
        }
        
        public void Schoon() {
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void Roteer() {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            foreach(SchetsElement element in schetslijst) {
                element.Roteer(this.bitmap.Height, this.bitmap.Width);
            }
        }

        public void verwijderUitLijst(Point p) {
            schetslijst.Remove(schetslijst.FindLast(x => x.Soort.isBevat(p,x)));   
        }
		
		public void voegtoeAanLijst(ISchetsTool t_huidigetool, Point t_beginpunt, Color t_kleur) {
            schetslijst.Add(new SchetsElement(t_huidigetool, t_beginpunt, t_kleur));
        }

        public String slaLijstOp() {
            return "";
        }
    }
}
