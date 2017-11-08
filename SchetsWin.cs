using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.Linq;
using System.Text;
using System.IO;

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
            ofd_Schets.Filter = "SchetsPlus|*.txt|JPEG Image|*.jpg|Bitmap Image|*.bmp|PNG Image|*.png";
            ofd_Schets.Title = "Open image";
            ofd_Schets.ShowDialog();

            if (ofd_Schets.FileName != "") {
                FileStream fs_Schets = (FileStream)ofd_Schets.OpenFile();
                if (ofd_Schets.FilterIndex == 1) {
                    StreamReader reader = new StreamReader(fs_Schets);
                    string fileContents = reader.ReadToEnd();
                    MessageBox.Show(fileContents, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.schetscontrol.Schets.schetslijst = stringNaarLijst(fileContents);
                    this.schetscontrol.TekenUitLijst();
                    fs_Schets.Close();
                } else {
                    this.schetscontrol.Schets.bitmap = new Bitmap(Image.FromStream(fs_Schets));
                    this.schetscontrol.Invalidate();
                    fs_Schets.Close();
                }
            }
        }

        private void opslaan(object sender, EventArgs e) {
            //Show file dialog
            SaveFileDialog sfd_Schets = new SaveFileDialog();
            sfd_Schets.Filter = "SchetsPlus|*.txt|JPEG Image|*.jpg|Bitmap Image|*.bmp|PNG Image|*.png";
            sfd_Schets.Title = "Save image";
            sfd_Schets.ShowDialog();

            //Check which image type to save to, and save
            if (sfd_Schets.FileName != "") {
                System.IO.FileStream fs_Schets = (System.IO.FileStream)sfd_Schets.OpenFile();
                switch (sfd_Schets.FilterIndex) {
                    case 1:
                        try {
                            string s_lijst = lijstNaarString(schetscontrol.Schets.schetslijst);
                            byte[] bArray = Encoding.UTF8.GetBytes(s_lijst);
                            fs_Schets.Write(bArray, 0, bArray.Count());

                        } catch (Exception) {
                            MessageBox.Show("Some error in hetnieuweopslaan", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            MessageBox.Show("", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case 2:
                        this.schetscontrol.Schets.bitmap.Save(fs_Schets, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 3:
                        this.schetscontrol.Schets.bitmap.Save(fs_Schets, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;

                    case 4:
                        this.schetscontrol.Schets.bitmap.Save(fs_Schets, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }

                fs_Schets.Close();

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
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) => {
                vast = true;
                huidigeTool.MuisVast(schetscontrol, mea.Location);
                if (huidigeTool != deTools[7]) {
                    schetscontrol.Schets.voegtoeAanLijst(huidigeTool, new Point(mea.X, mea.Y), schetscontrol.PenKleur);                    
                }
            };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) => {
                if (vast) {
                    huidigeTool.MuisDrag(schetscontrol, mea.Location);
                    //schetscontrol.Schets.voegtoeAanLijst(huidigeTool, new Point(mea.X, mea.Y), schetscontrol.PenKleur);
                    if (huidigeTool == deTools[0]) schetscontrol.Schets.Schetslijst[schetscontrol.Schets.Schetslijst.Count - 1].penPunten.Add(new Point(mea.X, mea.Y));
                }
            };
            schetscontrol.MouseUp += (object o, MouseEventArgs mea) => {
                if (vast)
                    huidigeTool.MuisLos(schetscontrol, mea.Location);
                vast = false;

                if (huidigeTool != deTools[7]) schetscontrol.Schets.Schetslijst[schetscontrol.Schets.Schetslijst.Count - 1].Eindpunt = new Point(mea.X, mea.Y);
            };
            schetscontrol.KeyPress += (object o, KeyPressEventArgs kpea) => {
                huidigeTool.Letter(schetscontrol, kpea.KeyChar);
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
            menu.DropDownItems.Add("Undo", null, this.undo);
            menu.DropDownItems.Add("Redo", null, this.redo);
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

        private string lijstNaarString(List<SchetsElement> s_list) {
            string s_string = "";
            foreach (SchetsElement i in s_list) {
                try {
                    s_string += i.Soort.ToString();
                    s_string += i.Beginpunt.ToString();
                    s_string += i.Eindpunt.ToString();
                    s_string += i.Kleur.Name;
                    s_string += " ";
                    s_string += i.Tekst?.ToString();
                    s_string += "\n";
                } catch (Exception) {
                    MessageBox.Show(("Some error in building the string at element " + i), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            MessageBox.Show(s_string, "De string s list", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return s_string;
        }

        private void undo(object sender, EventArgs e) {
            this.schetscontrol.Schets.Schetslijst[this.schetscontrol.Schets.Schetslijst.Count - 1].Zichtbaar = false;
            this.schetscontrol.TekenUitLijst();
        }

        private void redo(object sender, EventArgs e) {
            this.schetscontrol.Schets.Schetslijst[this.schetscontrol.Schets.Schetslijst.Count - 1].Zichtbaar = true;
            this.schetscontrol.TekenUitLijst();
        }

        private List<SchetsElement> stringNaarLijst(string l_string) {
            List<SchetsElement> l_list = new List<SchetsElement>();
            int j = 0;
            int index0 = 0;
            int index1 = 0;
            int index2 = 0;
            int index3 = 0;
            int index4 = 0;
            int index5 = 0;
            int index6 = 0;
            int index7 = 0;

            for (int i = 0; i < l_string.Length; i++) {
                if (l_string[i] == '{' && j == 0) {
                    index1 = i;
                    j++;
                } else if (l_string[i] == ',' && j == 1) {
                    index2 = i;
                    j++;
                } else if (l_string[i] == '}' && j == 2) {
                    index3 = i;
                    j++;
                } else if (l_string[i] == ',' && j == 3) {
                    index4 = i;
                    j++;
                } else if (l_string[i] == '}' && j == 4) {
                    index5 = i;
                    j++;
                } else if (l_string[i] == ' ' && j == 5) {
                    index6 = i;
                    j++;
                } else if (l_string[i] == '\n' && j == 6) {
                    index7 = i;
                    string soort = l_string.Substring(index0, (index1 - index0));
                    ISchetsTool e_soort = new PenTool();
                    if (soort == "pen") {
                        e_soort = new PenTool();
                    } else if (soort == "lijn") {
                        e_soort = new LijnTool();
                    } else if (soort == "kader") {
                        e_soort = new RechthoekTool();
                    } else if (soort == "vlak") {
                        e_soort = new VolRechthoekTool();
                    } else if (soort == "ellips") {
                        e_soort = new EllipsTool();
                    } else if (soort == "ovlak") {
                        e_soort = new VolEllipsTool();
                    } else if (soort == "tekst") {
                        e_soort = new TekstTool();
                    }
                    string x1 = l_string.Substring(index1 + 3, (index2 - (index1 + 3)));
                    string y1 = l_string.Substring(index2 + 3, (index3 - (index2 + 3)));
                    Point e_beginpunt = new Point(int.Parse(x1), int.Parse(y1));
                    string x2 = l_string.Substring(index3 + 4, (index4 - (index3 + 4)));
                    string y2 = l_string.Substring(index4 + 3, (index5 - (index4 + 3)));
                    Point e_eindpunt = new Point(int.Parse(x2), int.Parse(y2));
                    string kleur = l_string.Substring(index5 + 1, (index6 - (index5 + 1)));
                    Color e_kleur = Color.FromName(kleur);
                    string e_tekst = l_string.Substring(index6 + 1, (index7 - (index6 + 1)));
                    if (e_tekst == "")
                        e_tekst = null;
                    l_list.Add(new SchetsElement(e_soort, e_beginpunt, e_eindpunt, e_kleur, e_tekst));

                    j = 0;
                    index0 = index7 + 1;
                }
            }
            return l_list;
        }
    }
}
