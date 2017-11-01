using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor {
    public interface ISchetsTool {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
        bool isBevat(Point p, Point beginpunt, Point eindpunt);
    }

    public abstract class StartpuntTool : ISchetsTool {
        protected Point startpunt;
        protected Brush kwast;

        public virtual void MuisVast(SchetsControl s, Point p) {
            startpunt = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p) {
            kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
        public abstract bool isBevat(Point p, Point beginpunt, Point eindpunt);

        public bool opLijn(Point p, Point beginpunt, Point eindpunt) {
            int crossProduct = (p.X - beginpunt.X) * (eindpunt.X - beginpunt.X) - (p.Y - beginpunt.Y) * (eindpunt.Y - beginpunt.Y);
            if (Math.Abs(crossProduct) > 0.1) return false;
            else return true;
        }
    }

    public class TekstTool : StartpuntTool {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c) {
            if (c >= 32) {
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz =
                gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
                gr.DrawString(tekst, font, kwast,
                                              this.startpunt, StringFormat.GenericTypographic);
                // gr.DrawRectangle(Pens.Black, startpunt.X, startpunt.Y, sz.Width, sz.Height);
                startpunt.X += (int)sz.Width;
                s.Invalidate();
            }
        }

        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) { return true;  }//Needs
    }

    public abstract class TweepuntTool : StartpuntTool {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2) {
            return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
                                , new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte) {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void MuisVast(SchetsControl s, Point p) {
            base.MuisVast(s, p);
            kwast = Brushes.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p) {
            s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p) {
            base.MuisLos(s, p);
            this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c) {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);

        public virtual void Compleet(Graphics g, Point p1, Point p2) {
            this.Bezig(g, p1, p2);
        }
    }

    public class RechthoekTool : TweepuntTool {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2) {
            g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) {
            return opLijn(p, beginpunt, new Point(beginpunt.X, eindpunt.Y)) || opLijn(p, beginpunt, new Point(beginpunt.X, eindpunt.X)) || opLijn(p, eindpunt, new Point(eindpunt.X, beginpunt.Y)) || opLijn(p, eindpunt, new Point(eindpunt.X, beginpunt.X));
        }
    }

    public class VolRechthoekTool : RechthoekTool {
        public override string ToString() { return "vlak"; }

        public override void Compleet(Graphics g, Point p1, Point p2) {
            g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) {
            Size siz = new Size((eindpunt.X - beginpunt.X), (eindpunt.Y - beginpunt.Y));
            return new Rectangle(beginpunt, siz).Contains(p);
        }
    }

    public class EllipsTool : TweepuntTool {
        public override string ToString() { return "ellips"; }

        public override void Bezig(Graphics g, Point p1, Point p2) {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) {
            return opLijn(p, beginpunt, new Point(beginpunt.X, eindpunt.Y)) || opLijn(p, beginpunt, new Point(beginpunt.X, eindpunt.X)) || opLijn(p, eindpunt, new Point(eindpunt.X, beginpunt.Y)) || opLijn(p, eindpunt, new Point(eindpunt.X, beginpunt.X));
        }
    }

    public class VolEllipsTool : EllipsTool {
        public override string ToString() { return "ovlak"; }

        public override void Compleet(Graphics g, Point p1, Point p2) {
            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }


        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) {
            Size siz = new Size((eindpunt.X - beginpunt.X), (eindpunt.Y - beginpunt.Y));
            Rectangle myEllipse = new Rectangle(beginpunt, siz);
            GraphicsPath myPath = new GraphicsPath();
            myPath.AddEllipse(myEllipse);
            return myPath.IsVisible(p);
        }
        
    }

    public class LijnTool : TweepuntTool {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2) {
            g.DrawLine(MaakPen(this.kwast, 3), p1, p2);
        }

        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) {
            return opLijn(p, beginpunt, eindpunt);
        }
    }

    public class PenTool : LijnTool {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p) {
            this.MuisLos(s, p);
            this.MuisVast(s, p);
        }

        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) {
            return opLijn(p, beginpunt, eindpunt);
        }
    }

    public class GumTool : StartpuntTool {
        public override string ToString() { return "gum"; }

        public override void MuisLos(SchetsControl s, Point p) {
            s.Schets.verwijderUitLijst(p);
            s.TekenUitLijst();
        }

        public override void MuisDrag(SchetsControl s, Point p) { }
        public override void Letter(SchetsControl s, char c) { }

        public override bool isBevat(Point p, Point beginpunt, Point eindpunt) { return false; }
        }
}
