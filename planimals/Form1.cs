using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Documents;
using System.Windows.Forms;

namespace planimals
{
    public partial class Form1 : Form
    {
        //instantiatig a card is good, now have to make a hand of em
        //store hand into the Hand table
        //done that thing also^^^^
        //now i have to draw a field where cards are going to be placed
        //if the card is not placed in the field put it to the initial location
        //if it is then start the q of cards
        //after that i need a button that will start the process of validating the chain
        //if the chain is correct add 10 points to the score and draw a new card

        List<(Card, Point, Point, long, long)> MoveList;
        static Random rnd;
        public static List<Card> playerHand;
        private Timer timer;
        Stopwatch sw;

        public List<Card> chain;

        public static int workingHeight;
        public static int workingWidth;

        private static string currentDir = Environment.CurrentDirectory;
        private static string dbPath = currentDir + "\\cards.mdf";

        private static string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;" +
            $"AttachDbFilename={dbPath}" +
            ";Integrated Security=True;Connect Timeout=30";
        private static readonly SqlConnection sqlConnection = new SqlConnection(connectionString);

        private Rectangle fieldRectangle;

        public Form1()
        {

            InitializeComponent();
            FormBorderStyle = FormBorderStyle.Fixed3D;
            var bounds = Screen.PrimaryScreen.Bounds;
            MinimizeBox = false;
            Text = "Planimals";
            StartPosition = FormStartPosition.CenterScreen;

            Height = Screen.PrimaryScreen.WorkingArea.Height;
            Width = Screen.PrimaryScreen.WorkingArea.Width;

            workingHeight = ClientRectangle.Height;
            workingWidth = ClientRectangle.Width;

            fieldRectangle = new Rectangle(workingWidth/100 * 20, workingHeight / 4, workingWidth / 10 * 6, workingHeight / 2);

            BackColor = Color.Black;

            var drawCardButton = new PictureBox();
            drawCardButton.Width = workingHeight / 8;
            drawCardButton.Height = workingWidth / 10;
            drawCardButton.SizeMode = PictureBoxSizeMode.StretchImage;
            drawCardButton.Location = new Point(workingWidth - drawCardButton.Width - workingHeight / 100 * 5, workingHeight / 2 - drawCardButton.Height / 2);
            Rectangle cardRectangle = new Rectangle(workingWidth - drawCardButton.Width - workingHeight / 100 * 5, workingHeight / 2 - drawCardButton.Height / 2, workingHeight / 8, workingWidth / 10);
            drawCardButton.Image = Image.FromFile(currentDir + "\\assets\\photos\\back.png");
            Controls.Add(drawCardButton);
            drawCardButton.Click += new EventHandler(drawCardButton_Click);

            if (MousePosition.X < cardRectangle.Right && MousePosition.X > cardRectangle.Left && MousePosition.Y < cardRectangle.Bottom && MousePosition.Y > cardRectangle.Top) 
            {
                Bitmap b = new Bitmap(drawCardButton.Image);
            }



            MoveList = new List<(Card, Point, Point, long, long)>();
            timer = new Timer();
            timer.Tick += new EventHandler(MoveCards);
            timer.Interval = 10;
            sw = new Stopwatch();
            timer.Start();
            sw.Start();

            rnd = new Random();
            playerHand = new List<Card>();

            this.MouseClick += new MouseEventHandler(MouseLeftClick);
            this.Paint += new PaintEventHandler(DrawFieldBorders);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 1; i++)
            {
                DrawCard(playerHand);
            }
            foreach (Card card in playerHand)
            {
                Controls.Add(card);
            }
        }
        public void DrawFieldBorders(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.White, 10.0f))
            {
                e.Graphics.DrawRectangle(pen, fieldRectangle);
            }
        }

        public void drawCardButton_Click(object sender, EventArgs e)
        {
            DrawCard(playerHand);
            Controls.Add(playerHand[playerHand.Count - 1]);
        }

        public int GetNumberOfOrganisms()
        {
            int count = 0;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) AS num FROM Organisms", sqlConnection);
                sqlConnection.Open();
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = int.Parse(reader["num"].ToString());
                    }
                }
            }
            return count;
        }
        public string GetRandomScientificName()
        {
            int noOfOrganisms = GetNumberOfOrganisms();
            int randInx = rnd.Next(noOfOrganisms);
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand($"WITH Numbered AS (SELECT Scientific_name, ROW_NUMBER() OVER(ORDER BY Scientific_name) as ROW_NUM FROM Organisms) SELECT Scientific_name FROM Numbered where ROW_NUM = {randInx}", sqlConnection);
                sqlConnection.Open();
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["Scientific_name"].ToString();
                    }
                }
            }
            return null;
        }
        public void DrawCard(List<Card> playerHand)
        {
            string sciname = GetRandomScientificName();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand($"SELECT * FROM Organisms WHERE Scientific_name='{sciname}'", sqlConnection);
                sqlConnection.Open();
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cname = reader["Common_name"].ToString();
                        var desc = reader["Description"].ToString();
                        var path = currentDir + "\\assets\\photos\\" + $"{sciname}.jpg";
                        int hierarchy = (int)reader["Hierarchy"];
                        var habitat = reader["Habitat"].ToString();

                        if (playerHand.Count == 0)
                        {
                            Card c = new Card(sciname, cname, desc, path, hierarchy, habitat, new Point(Width/2 - Card.pictureBoxWidth/2, Height - Card.pictureBoxHeight));
                            playerHand.Add(c);
                        } 
                        else if (playerHand[playerHand.Count - 1].Location.Y != Height - Card.pictureBoxHeight) 
                        {
                            Card c = new Card(sciname, cname, desc, path, hierarchy, habitat, new Point(Width / 2 - Card.pictureBoxWidth / 2, Height - Card.pictureBoxHeight));
                            playerHand.Add(c);
                        }
                        else
                        {
                            foreach (Card card in playerHand)
                            {
                                if (card.Location.Y == Height - Card.pictureBoxHeight)
                                {
                                    card.Location = new Point(int.Parse((card.Location.X - Math.Pow(playerHand.Count, 2)).ToString()), card.Location.Y);
                                }
                            }
                            Card c = new Card(sciname, cname, desc, path, hierarchy, habitat, new Point(playerHand[(playerHand.Count) - 1].Location.X + 100, playerHand[(playerHand.Count) - 1].Location.Y));
                            playerHand.Add(c);
                        }
                    }
                }
            }
        }
        //Daniel's code
        #region fancy card moving
        private void EaseInOut(Card card, Point endPosition, long length)
        {
            if (endPosition.X < fieldRectangle.Right && endPosition.X > fieldRectangle.Left && endPosition.Y > fieldRectangle.Top && endPosition.Y < fieldRectangle.Bottom)
            {
                Point offset = new Point(endPosition.X - card.Location.X - card.Width / 2, endPosition.Y - card.Location.Y - card.Height / 2);
                (Card, Point, Point, long, long) data = (card, card.Location, offset, length, sw.ElapsedMilliseconds);
                MoveList.Add(data);
                card.Picked = false;
                card.BackColor = Color.Gray;
            } else
            {
                card.Picked = false;
                card.BackColor = Color.Gray;
            }
        }
        private void MoveCards(object sender, EventArgs e)
        {
            long currentTime = sw.ElapsedMilliseconds;
            List<int> purgeIndexes = new List<int>();
            int index = 0;
            foreach ((Card card, Point startPosition, Point offset, long length, long startTime) in MoveList)
            {
                long dt = currentTime - startTime;
                if (dt > length)
                {
                    card.Location = new Point(startPosition.X + offset.X, startPosition.Y + offset.Y);
                    purgeIndexes.Add(index);
                    continue;
                }
                double f = F((double)(dt) / length, -3f);
                card.Location = new Point((int)(offset.X * f) + startPosition.X, (int)(offset.Y * f) + startPosition.Y);
                index++;
            }

            int shift = 0;
            foreach (int i in purgeIndexes)
            {
                MoveList.RemoveAt(i - shift);
                shift++;
            }
        }
        private static double F(double timeThrough, double a)
        {
            if (timeThrough >= 1) { return 1; }
            double x = timeThrough;
            double y = Math.Pow(1 - Math.Pow(x - 1, 2), 0.5f);
            return y;
        }

        private void MouseLeftClick(object sender, MouseEventArgs e)
        {
            MoveList.Clear();
            int i = SearchPickedCard();
            if (i == 0 && playerHand[0].Picked)
            {
                EaseInOut(playerHand[i], e.Location, 1000);
            }
            else if (i != 0)
            {
                EaseInOut(playerHand[i], e.Location, 1000);
            }
        }

        private int SearchPickedCard()
        {
            foreach (Card c in playerHand)
            {
                if (c.Picked == true)
                {
                    return playerHand.IndexOf(c);
                }
            }
            return 0;
        }
        #endregion
    }
}
