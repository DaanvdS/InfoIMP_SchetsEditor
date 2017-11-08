using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor {
    public interface ISchetsTool {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
        void Letter(SchetsControl s, char c, int gedraaid);
        bool isBevat(Point p, SchetsElement s);
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
        public abstract void Letter(SchetsControl s, char c, int gedraaid);
        public abstract bool isBevat(Point p, SchetsElement s);
    }

    public class TekstTool : StartpuntTool {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c) {
            if (c >= 32) {
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz = gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);

                GraphicsPath myPath = new GraphicsPath();
                myPath.AddString(c.ToString(), new FontFamily("Tahoma"), (int)FontStyle.Regular, 48, this.startpunt, StringFormat.GenericDefault);
                
                gr.FillPath(kwast, myPath);
                startpunt.X += (int)myPath.GetBounds().Width + 5;
                s.Invalidate();
            }
        }

        public override void Letter(SchetsControl s, char c, int gedraaid) {
            if (c >= 32) {
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz = gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);

                GraphicsPath myPath = new GraphicsPath();
                myPath.AddString(c.ToString(), new FontFamily("Tahoma"), (int)FontStyle.Regular, 48, this.startpunt, StringFormat.GenericDefault);
                int width = (int)myPath.GetBounds().Width + 5;
                Matrix m = new Matrix();
                m.RotateAt(gedraaid, startpunt);
                myPath.Transform(m);

                gr.FillPath(kwast, myPath);
                if (gedraaid == 0) {
                    startpunt.X += width;
                } else if (gedraaid == 90) {
                    startpunt.Y += width;
                } else if(gedraaid == 180) {
                    startpunt.X -= width;
                } else if (gedraaid == 270) {
                    startpunt.Y -= width;
                }

                s.Invalidate();
            }
        }
        

        public override bool isBevat(Point p, SchetsElement s) {
            GraphicsPath myPath = new GraphicsPath();
            myPath.AddString(s.Tekst, new FontFamily("Tahoma"), (int)FontStyle.Regular, 48, s.Beginpunt, StringFormat.GenericDefault);
            Matrix m = new Matrix();
            m.RotateAt(s.Gedraaid, s.Beginpunt);
            myPath.Transform(m);
            return myPath.IsVisible(p);
        }
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
        public override void Letter(SchetsControl s, char c) { }
        public override void Letter(SchetsControl s, char c, int gedraaid) { }
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

        public override bool isBevat(Point p, SchetsElement s) {
            Size siz = new Size((s.Eindpunt.X - s.Beginpunt.X), (s.Eindpunt.Y - s.Beginpunt.Y));
            Rectangle myRectangle = new Rectangle(s.Beginpunt, siz);
            GraphicsPath myPath = new GraphicsPath();
            myPath.AddRectangle(myRectangle);
            return myPath.IsOutlineVisible(p.X, p.Y, MaakPen(kwast, 3));
        }
    }

    public class VolRechthoekTool : RechthoekTool {
        public override string ToString() { return "vlak"; }

        public override void Compleet(Graphics g, Point p1, Point p2) {
            g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override bool isBevat(Point p, SchetsElement s) {
            Size siz = new Size((s.Eindpunt.X - s.Beginpunt.X), (s.Eindpunt.Y - s.Beginpunt.Y));
            return new Rectangle(s.Beginpunt, siz).Contains(p);
        }
    }

    public class EllipsTool : TweepuntTool {
        public override string ToString() { return "ellips"; }

        public override void Bezig(Graphics g, Point p1, Point p2) {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }

        public override bool isBevat(Point p, SchetsElement s) {
            Size siz = new Size((s.Eindpunt.X - s.Beginpunt.X), (s.Eindpunt.Y - s.Beginpunt.Y));
            Rectangle myEllipse = new Rectangle(s.Beginpunt, siz);
            GraphicsPath myPath = new GraphicsPath();
            myPath.AddEllipse(myEllipse);
            return myPath.IsOutlineVisible(p.X,p.Y,MaakPen(kwast,3));
        }
    }

    public class VolEllipsTool : EllipsTool {
        public override string ToString() { return "ovlak"; }

        public override void Compleet(Graphics g, Point p1, Point p2) {
            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }


        public override bool isBevat(Point p, SchetsElement s) {
            Size siz = new Size((s.Eindpunt.X - s.Beginpunt.X), (s.Eindpunt.Y - s.Beginpunt.Y));
            Rectangle myEllipse = new Rectangle(s.Beginpunt, siz);
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

        public override bool isBevat(Point p, SchetsElement s) {
            GraphicsPath myPath = new GraphicsPath();
            myPath.AddLine(s.Beginpunt, s.Eindpunt);
            myPath.Widen(MaakPen(kwast, 3));
            return myPath.IsVisible(p);
        }
    }

    public class PenTool : LijnTool {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p) {
            this.MuisLos(s, p);
            this.MuisVast(s, p);
        }

        public override bool isBevat(Point p, SchetsElement s) {
            for (int i = 0; i < (s.penPunten.Count - 1); i++) {
                GraphicsPath myPath = new GraphicsPath();
                myPath.AddLine(s.penPunten[i], s.penPunten[i+1]);
                myPath.Widen(MaakPen(kwast, 3));
                if (myPath.IsVisible(p)) { return true;  }
            }
            return false;
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
        public override void Letter(SchetsControl s, char c, int gedraaid) { }
        public override bool isBevat(Point p, SchetsElement s) {
            return false;
        }
    }
}
