using System;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace planimals
{
    public class Card : PictureBox
    {
        public bool Picked;

        public string scientific_name;
        public string common_name;
        public string description;
        private int hierarchy;
        public string habitat;

        public static int pictureBoxWidth = Form1.formHeight / 8;
        public static int pictureBoxHeight = Form1.formWidth / 10;
        public Card(string sname, string cname, string desc, string path, int hier, string habt, Point position)
        {
            scientific_name = sname;
            common_name = cname;
            description = desc;
            hierarchy = hier;
            habitat = habt;

            Height = pictureBoxHeight;
            Width = pictureBoxWidth;

            Image = Image.FromFile(path);
            SizeMode = PictureBoxSizeMode.Zoom;
            Size = new Size(pictureBoxWidth, pictureBoxHeight);
            Location = new Point(position.X, position.Y);
            BackColor = Color.Gray;
            Picked = false;

            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add("Show Info", new EventHandler(cardRightClick));
            ContextMenu = cm;

            MouseClick += new MouseEventHandler(cardLeftClick);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, Color.Black, ButtonBorderStyle.Solid);
            using (Font myFont = new Font("Arial", 8))
            {
                e.Graphics.DrawString(common_name, myFont, Brushes.Yellow, new Point(10, 14));
            }
        }

        public void cardRightClick(object sender, EventArgs e)
        {
            switch (hierarchy)
            {
                case (1):
                    MessageBox.Show($"Binomial name: {scientific_name}\nCommon name: {common_name}\n{description}\nRole in chain: Carnivore\nPrimarily lives in {habitat}");
                    break;
                case (2):
                    MessageBox.Show($"Binomial name: {scientific_name}\nCommon name: {common_name}\n{description}\nRole in chain: Carnivore/Omnivore\nPrimarily lives in {habitat}");
                    break;
                case (3):
                    MessageBox.Show($"Binomial name: {scientific_name}\nCommon name: {common_name}\n{description}\nRole in chain: Herbivore\nPrimarily lives in {habitat}");
                    break;
                case (4):
                    MessageBox.Show($"Binomial name: {scientific_name}\nCommon name: {common_name}\n{description}\nRole in chain: Producer\nPrimarily lives in {habitat}");
                    break;
            }
        }

        public void cardLeftClick(object sender, EventArgs e)
        {
            if (!Picked)
            {
                Picked = true;
                BackColor = Color.White;
                this.BringToFront();
            }
            else
            {
                Picked = false;
                BackColor = Color.Gray;
            }
        }
    }
}
