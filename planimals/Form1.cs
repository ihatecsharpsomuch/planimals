﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;

namespace planimals
{
    public partial class Form1 : Form
    {
        ///UI
        //fix animation, make a dynamic cards bar


        ///logic
        //adapt the game only for one chain, then just iterate over the chains
        //fix out of bounds as playerChain.Clear makes playerChain empty not only playerChain[0]
        //push and pull changes to Cards.mdf


        private List<(Card, Point, Point, long, long)> MoveList;
        private static Random rnd;
        private Timer timer1;
        private Stopwatch sw1;


        private static string currentDir = Environment.CurrentDirectory;
        private static string dbPath = currentDir + "\\cards.mdf";
        private static string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;" + $"AttachDbFilename={dbPath}" + ";Integrated Security=True;Connect Timeout=30";
        private static readonly SqlConnection sqlConnection = new SqlConnection(connectionString);


        public static int workingHeight;
        public static int workingWidth;
        private static int ratio;


        public static List<Card> playerHand;
        public static List<List<Card>> playerChain;
        public static Rectangle fieldRectangle;


        private PictureBox drawCardButton;
        private Image drawCardButtonBack;
        private Rectangle cardRectangle;

        private PictureBox chainButton;
        private Image chainButtonBack;
        private Rectangle chainButtonRectangle;


        private Label label = new Label();

        public Form1()
        {

            InitializeComponent();

            MoveList = new List<(Card, Point, Point, long, long)>();
            timer1 = new Timer();
            timer1.Tick += new EventHandler(MoveCards);
            timer1.Interval = 10;
            sw1 = new Stopwatch();

            timer1.Start();
            sw1.Start();

            #region UI
            FormBorderStyle = FormBorderStyle.Fixed3D;
            MinimizeBox = false;
            Text = "Planimals";
            StartPosition = FormStartPosition.CenterScreen;

            Height = Screen.PrimaryScreen.WorkingArea.Height;
            Width = Screen.PrimaryScreen.WorkingArea.Width;
            workingHeight = ClientRectangle.Height;
            workingWidth = ClientRectangle.Width;
            ratio = workingHeight / workingWidth;


            fieldRectangle = new Rectangle(
                workingWidth / 100 * 20,
                workingHeight / 4,
                workingWidth / 10 * 6,
                workingHeight / 2
            );

            BackColor = Color.Black;

            drawCardButton = new PictureBox();
            drawCardButtonBack = Image.FromFile(currentDir + "\\assets\\photos\\back.png");
            drawCardButton.SizeMode = PictureBoxSizeMode.StretchImage;
            drawCardButton.Width = workingHeight / 8;
            drawCardButton.Height = workingWidth / 10;
            drawCardButton.Location = new Point(
                drawCardButton.Width - workingHeight / 100 * 5,
                workingHeight / 2 - drawCardButton.Height / 2
            );
            cardRectangle = new Rectangle(
                drawCardButton.Width - workingHeight / 100 * 5,
                workingHeight / 2 - drawCardButton.Height / 2,
                workingHeight / 8,
                workingWidth / 10
            );
            drawCardButton.Image = drawCardButtonBack;
            Controls.Add(drawCardButton);
            drawCardButton.Click += new EventHandler(drawCardButton_Click);
            drawCardButton.MouseMove += DrawCardButton_MouseMove;

            chainButton = new PictureBox();
            chainButtonBack = Image.FromFile(currentDir + "\\assets\\photos\\chain.png");
            chainButton.SizeMode = PictureBoxSizeMode.StretchImage;
            chainButton.Width = workingWidth / 10;
            chainButton.Height = workingHeight / 10;
            chainButton.Location = new Point(
                workingWidth - drawCardButton.Width - workingHeight / 10,
                workingHeight / 2 - drawCardButton.Height / 2);
            chainButtonRectangle = new Rectangle(
                workingWidth - drawCardButton.Width - workingHeight / 10,
                workingHeight / 2 - drawCardButton.Height / 2,
                workingWidth / 10,
                workingHeight / 10
            );
            chainButton.Image = chainButtonBack;
            Controls.Add(chainButton);
            chainButton.Click += new EventHandler(chainButton_Click);
            chainButton.MouseMove += chainButton_MouseMove;

            label.Location = new Point(workingWidth / 10, workingHeight / 20);
            label.ForeColor = Color.White;
            label.AutoSize = true;
            Controls.Add(label);

            #endregion

            rnd = new Random();
            playerHand = new List<Card>();

            playerChain = new List<List<Card>>() { new List<Card>() { } };

            MouseClick += new MouseEventHandler(MouseLeftClick);
            Paint += new PaintEventHandler(DrawFieldBorders);
            MouseMove += DrawCardButton_MouseMove;
            //Resize += new EventHandler(OnResize);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void OnResize(object sender, EventArgs e)
        {
            workingHeight = ClientRectangle.Height;
            workingWidth = ClientRectangle.Width;


            fieldRectangle = new Rectangle(
                workingWidth / 100 * 20,
                workingHeight / 4,
                workingWidth / 10 * 6,
                workingHeight / 2
            );
            drawCardButton.Width = workingHeight / 8;
            drawCardButton.Height = workingWidth / 10;
            drawCardButton.Location = new Point(
                drawCardButton.Width - workingHeight / 100 * 5,
                workingHeight / 2 - drawCardButton.Height / 2
            );

            chainButton.Width = workingWidth / 10;
            chainButton.Height = workingHeight / 10;
            chainButton.Location = new Point(
                workingWidth - drawCardButton.Width - workingHeight / 10,
                workingHeight / 2 - drawCardButton.Height / 2
            );

            foreach (Card c in playerHand)
            {
                c.Width = workingHeight / 8;
                c.Height = workingWidth / 10;
                c.Location = new Point(
                    workingWidth / 2 - c.Width,
                    workingHeight - c.Height
                );
            }
            Invalidate();
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
        }
        private void DrawCardButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (MousePosition.X < cardRectangle.Right && MousePosition.X > cardRectangle.Left && MousePosition.Y < cardRectangle.Bottom && MousePosition.Y > cardRectangle.Top)
            {
                drawCardButton.Width = workingHeight / 8 + 5;
                drawCardButton.Height = workingWidth / 10 + 5;
            }
            else
            {
                drawCardButton.Width = workingHeight / 8;
                drawCardButton.Height = workingWidth / 10;
            }
        }
        private void Chain(List<List<Card>> chain)
        {
            bool valid = true;
            FixChainIndices();
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                if (chain[0].Count < 2) { label.Text = "The chain must consist of at least two organisms."; }
                else
                {
                    sqlConnection.Open();
                    for (int i = 0; i < chain[0].Count; i++)
                    {
                        if (i == chain.Count - 1)
                        {
                            break;
                        }
                        SqlCommand cmd = new SqlCommand($"SELECT COUNT(*) from Relations where Consumer='{chain[0][i + 1].scientific_name}' AND Consumed='{chain[0][i].scientific_name}'", sqlConnection);
                        int b = (int)cmd.ExecuteScalar(); //1 - then relation exists. 0 - does not exist
                        if (b == 0) {
                            label.Text = "Food chain is invalid";
                            valid = false;
                            for (int j = 0; j < playerChain[0].Count; i++)
                            {
                                EaseInOut(playerChain[0][j], playerChain[0][j].prevLocation, 200);
                            }
                            break;
                        }
                    }
                    sqlConnection.Close();
                    if (valid)
                    {
                        label.Text = $"+{CalcScore(chain[0].Count)} points";
                        foreach (Card c in chain[0]) { 
                            Controls.Remove(c);
                            c.Image.Dispose();
                            playerHand.Remove(c);
                        }
                        playerChain.Clear();

                        for (int j = 0; j < playerHand.Count; j++)
                        {
                            playerHand[0].Location = new Point(Card.pictureBoxWidth * (j + 1) * playerHand.Count, Height - Card.pictureBoxHeight);
                        }
                    }
                }
            }
        }
        private void FixChainIndices() {
            for (int i = 0; i < playerChain[0].Count; i++) {
                if (i == playerChain[0].Count - 1) {
                    break;
                }
                if (playerChain[0][i].Location.X > playerChain[0][i + 1].Location.X) {
                    Card temp = playerChain[0][i];
                    playerChain[0][i] = playerChain[0][i + 1];
                    playerChain[0][i + 1] = temp;
                }
            }
        }

        private int CalcScore(int noOfCards) {
            int score = 0;
            for (int i = 0; i < noOfCards; i++)
            {
                score += 5 * (i+1);
            }
            return score;
        }

        public void chainButton_Click(object sender, EventArgs e)
        {
            Chain(playerChain);
        }
        private void chainButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (MousePosition.X < chainButtonRectangle.Right && MousePosition.X > chainButtonRectangle.Left && MousePosition.Y < chainButtonRectangle.Bottom && MousePosition.Y > chainButtonRectangle.Top)
            {
                chainButton.Width = workingWidth / 10 + 5;
                chainButton.Height = workingHeight / 10 + 5;
            }
            else
            {
                chainButton.Width = workingWidth / 10;
                chainButton.Height = workingHeight / 10;
            }
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
            if (playerHand.Count < 15)
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
                            Card c = new Card(sciname, cname, desc, path, hierarchy, habitat, new Point(Card.pictureBoxWidth*playerHand.Count, Height - Card.pictureBoxHeight));
                            playerHand.Add(c);
                            Controls.Add(c);
                        }
                    }
                    sqlConnection.Close();
                }
            }
            else
            {
                label.Text = "Cannot hold more than 15 cards.";
            }

        }
        #region fancy card moving
        private void EaseInOut(Card c, Point endPosition, long length)
        {
            if (InRectangle(endPosition)) //check whether user wants to move c onto the table or just messing with them          
            {
                Point offset = new Point(endPosition.X - c.Location.X - c.Width / 2, endPosition.Y - c.Location.Y - c.Height / 2);
                (Card, Point, Point, long, long) data = (c, c.Location, offset, length, sw1.ElapsedMilliseconds);
                MoveList.Add(data);
                c.Picked = false;
                c.BackColor = Color.Gray;
                playerChain[0].Add(c);
            }
            if (InRectangle(c.Location) && !InRectangle(endPosition)) {
                Point offset = new Point(c.prevLocation.X - c.Location.X, c.prevLocation.Y - c.Location.Y);
                (Card, Point, Point, long, long) data = (c, c.Location, offset, length, sw1.ElapsedMilliseconds);
                MoveList.Add(data);
                c.Picked = false;
                c.BackColor = Color.Gray;
                playerChain[0].Remove(c);
            }
            else
            {
                c.Picked = false;
                c.BackColor = Color.Gray;
            }
        }
        private bool InRectangle(Point p) {
            return p.X < fieldRectangle.Right && p.X > fieldRectangle.Left && p.Y > fieldRectangle.Top && p.Y < fieldRectangle.Bottom;
        }
        private void MoveCards(object sender, EventArgs e)
        {
            long currentTime = sw1.ElapsedMilliseconds;
            List<int> purgeIndexes = new List<int>();
            int index = 0;
            foreach ((Card c, Point startPosition, Point offset, long length, long startTime) in MoveList)
            {
                long dt = currentTime - startTime;
                if (dt > length)
                {
                    c.Location = new Point(startPosition.X + offset.X, startPosition.Y + offset.Y);
                    purgeIndexes.Add(index);
                    continue;
                }
                double f = F((double)(dt) / length, -3f);
                c.Location = new Point((int)(offset.X * f) + startPosition.X, (int)(offset.Y * f) + startPosition.Y);
                index++;
            }

            for (int i = purgeIndexes.Count - 1; i >= 0; i--)
            {
                MoveList.RemoveAt(purgeIndexes[i]);
            }
        }
        private static double F(double timeThrough, double a)
        {
            if (timeThrough >= 1) { return 1; }
            double x = timeThrough;
            double y = Math.Pow(1 - Math.Pow(x - 1, 2), 0.5f);
            //double y = Math.Sin((Math.PI * x)/2);
            return y;
        }
        private void MouseLeftClick(object sender, MouseEventArgs e)
        {
            foreach (Card c in playerHand)
            {
                if (c.Picked == true)
                {
                    MoveList.Clear();
                    EaseInOut(c, e.Location, 500);
                }
            }
        }
        #endregion
    }
}
