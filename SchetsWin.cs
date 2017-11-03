using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;

namespace SchetsEditor {
    public class SchetsWin : Form {
        MenuStrip menuStrip;
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast;
        bool onopgeslagenVeranderingen = true;

        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );

        private void veranderAfmeting(object o, EventArgs ea) {
            schetscontrol.Size = new Size(this.ClientSize.Width - 70
                                          , this.ClientSize.Height - 50);
            paneel.Location = new Point(64, this.ClientSize.Height - 30);
        }

        private void klikToolMenu(object obj, EventArgs ea) {
            this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
        }

        private void klikToolButton(object obj, EventArgs ea) {
            this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
        }

        public void laden() {
            //Show file dialog
            OpenFileDialog ofd_Schets = new OpenFileDialog();
            ofd_Schets.Filter = "JPEG Image|*.jpg|Bitmap Image|*.bmp|PNG Image|*.png";
            ofd_Schets.Title = "Open image";
            ofd_Schets.ShowDialog();

            if (ofd_Schets.FileName != "") {
                System.IO.FileStream fs_Schets = (System.IO.FileStream)ofd_Schets.OpenFile();
                this.schetscontrol.Schets.bitmap = new Bitmap(Image.FromStream(fs_Schets));
                this.schetscontrol.Invalidate();
                fs_Schets.Close();
            }
        }

        private void opslaan(object sender, EventArgs e) {
            //Show file dialog
            SaveFileDialog sfd_Schets = new SaveFileDialog();
            sfd_Schets.Filter = "JPEG Image|*.jpg|Bitmap Image|*.bmp|PNG Image|*.png";
            sfd_Schets.Title = "Save image";
            sfd_Schets.ShowDialog();

            //Check which image type to save to, and save
            if (sfd_Schets.FileName != "") {
                System.IO.FileStream fs_Schets = (System.IO.FileStream)sfd_Schets.OpenFile();
                switch (sfd_Schets.FilterIndex) {
                    case 1:
                        this.schetscontrol.Schets.bitmap.Save(fs_Schets, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        this.schetscontrol.Schets.bitmap.Save(fs_Schets, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 3:
                        this.schetscontrol.Schets.bitmap.Save(fs_Schets, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }

                fs_Schets.Close();

                //Show file dialog
                sfd_Schets = new SaveFileDialog();
                sfd_Schets.Filter = "*.sef";
                sfd_Schets.Title = "Save image";
                sfd_Schets.ShowDialog();
                fs_Schets = (System.IO.FileStream)sfd_Schets.OpenFile();
                //fs_Schets.WriteAllText(this.schetscontrol.Schets.slaLijstOp());
            }
        }

        private void afsluiten(object obj, EventArgs ea) {
            if (this.onopgeslagenVeranderingen) {
                if (MessageBox.Show("Er zijn nog onopgeslagen veranderingen!", "Oeps!", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                    this.Close();
                }
            }

        }

        private void afsluiten(object obj, FormClosingEventArgs ea) {
            if (this.onopgeslagenVeranderingen) {
                if (MessageBox.Show("Er zijn nog onopgeslagen veranderingen!", "Oeps!", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                } else {
                    ea.Cancel = true;
                }
            }

        }

        public SchetsWin() {
            ISchetsTool[] deTools = { new PenTool()
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new EllipsTool()
                                    , new VolEllipsTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan", "White"
                                 };

            this.ClientSize = new Size(700, 500);
            huidigeTool = deTools[0];

            schetscontrol = new SchetsControl();
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {   vast=true;  
                                           huidigeTool.MuisVast(schetscontrol, mea.Location);
                                           //nieuw
                                           if(huidigeTool!=deTools[7])schetscontrol.Schets.voegtoeAanLijst(huidigeTool, new Point(mea.X, mea.Y), schetscontrol.PenKleur );
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location);
                                           //nieuw
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisLos (schetscontrol, mea.Location);
                                           vast = false;

                                           //nieuw
                                           if (huidigeTool != deTools[7]) schetscontrol.Schets.Schetslijst[schetscontrol.Schets.Schetslijst.Count -1].Eindpunt = new Point(mea.X, mea.Y);
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar);
                                           if (huidigeTool == deTools[6]) schetscontrol.Schets.Schetslijst[schetscontrol.Schets.Schetslijst.Count - 1].Tekst += kpea.KeyChar;
                                       };
            this.Controls.Add(schetscontrol);

            menuStrip = new MenuStrip();
            menuStrip.Visible = false;
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakAktieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }

        private void maakFileMenu() {
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
            menu.DropDownItems.Add("Opslaan", null, this.opslaan);
            menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }

        private void maakToolMenu(ICollection<ISchetsTool> tools) {
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools) {
                ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakAktieMenu(String[] kleuren) {
            ToolStripMenuItem menu = new ToolStripMenuItem("Aktie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon);
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer);
            ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
            foreach (string k in kleuren)
                submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
            menu.DropDownItems.Add(submenu);
            menuStrip.Items.Add(menu);
        }

        private void maakToolButtons(ICollection<ISchetsTool> tools) {
            int t = 0;
            foreach (ISchetsTool tool in tools) {
                RadioButton b = new RadioButton();
                b.Appearance = Appearance.Button;
                b.Size = new Size(45, 62);
                b.Location = new Point(10, 10 + t * 62);
                b.Tag = tool;
                b.Text = tool.ToString();
                b.Image = (Image)resourcemanager.GetObject(tool.ToString());
                b.TextAlign = ContentAlignment.TopCenter;
                b.ImageAlign = ContentAlignment.BottomCenter;
                b.Click += this.klikToolButton;
                this.Controls.Add(b);
                if (t == 0) b.Select();
                t++;
            }
        }

        private void maakAktieButtons(String[] kleuren) {
            paneel = new Panel();
            paneel.Size = new Size(600, 24);
            this.Controls.Add(paneel);

            Button b; Label l; ComboBox cbb;
            b = new Button();
            b.Text = "Clear";
            b.Location = new Point(0, 0);
            b.Click += schetscontrol.Schoon;
            paneel.Controls.Add(b);

            b = new Button();
            b.Text = "Rotate";
            b.Location = new Point(80, 0);
            b.Click += schetscontrol.Roteer;
            paneel.Controls.Add(b);

            l = new Label();
            l.Text = "Penkleur:";
            l.Location = new Point(180, 3);
            l.AutoSize = true;
            paneel.Controls.Add(l);

            cbb = new ComboBox(); cbb.Location = new Point(240, 0);
            cbb.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            paneel.Controls.Add(cbb);
        }
    }
}
