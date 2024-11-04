using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HCi_Gui
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private Thread receiveThread;

        Bitmap background = new Bitmap("mainbg.jpeg");
        // Elephant
        Bitmap elephant = new Bitmap("elephant-hd-png--17.png");
        Bitmap checkmarkelephant = new Bitmap("checkmark.jpeg");
        int xelephant = 150;
        int yelephant = 50;
        bool ballTouchedelephant = false;
        bool elephantapper = false;
        // The ball that helps detect that the Elephant is in the cage
        int xBall = 900;
        int yBall = 600;
        int ballRadius = 50;
        int checkx1 = 850;
        int checky1 = 500;
        bool moveelephant = false;

        //-------------------------------------------------------------------------------------
        //Alligator
        Bitmap alli = new Bitmap("Alligator-PNG-Download-Image.png");
        Bitmap checkmarkalli = new Bitmap("checkmark.jpeg");
        int xalli = 800;
        int yalli = 50;
        bool ballTouchedalli = false;
        bool alliapper = false;

        // The ball that helps detect that the alli is in the cage
        int xBallT = 600;
        int yBallT = 600;
        int ballRadiusT = 50;
        int checkx1T = 550;
        int checky1T = 500;
        bool movealli = false;
        //-------------------------------------------------------------------------------------
        //cheetah
        Bitmap cheetah = new Bitmap("cheetah.jpeg");
        Bitmap checkmarkcheetah = new Bitmap("checkmark.jpeg");
        int xcheetah = 500;
        int ycheetah = 50;
        bool ballTouchedcheetah = false;
        bool cheetahapper = true;
        // The ball that helps detect that the cheetah is in the cage
        int xBallF = 300;
        int yBallF = 600;
        int ballRadiusF = 50;
        int checkx1F = 250;
        int checky1F = 500;
        bool movecheetah = false;
        //---------------------------------------------------------------------------------------
        //winng picture
        Bitmap winning = new Bitmap("Winngbg.jpeg");
        bool c1 = false;
        bool c2 = false;
        bool c3 = false;
        //----------------------------------------------------------------------------------------
        public int flagE;
        public string data;
        BufferedGraphicsContext context;
        BufferedGraphics graphix;

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(background.Width, background.Height);
            this.Paint += Form1_Paint;
            this.Load += Form1_Load;
            context = BufferedGraphicsManager.Current;
            graphix = context.Allocate(this.CreateGraphics(), this.DisplayRectangle);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // throw new NotImplementedException();
            ConnectToServer("localhost", 5050);

            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //throw new NotImplementedException();
            DrawScene(graphix.Graphics);
            graphix.Render(e.Graphics);
        }
        private void DrawScene(Graphics g)
        {
            g.Clear(Color.White);
            g.DrawImage(background, 0, 0, background.Width, background.Height);
            g.DrawString("cheetah", new Font("Arial", 60), Brushes.Red, new PointF(250, 450));
            g.DrawString("alli", new Font("Arial", 60), Brushes.Red, new PointF(550, 450));
            g.DrawString("Elephant", new Font("Arial", 60), Brushes.Red, new PointF(800, 450));
            g.DrawString($"Welcome {data} Good luck ", new Font("Arial", 30), Brushes.Red, new PointF(30, 13));
            alli.MakeTransparent(Color.Transparent);
            cheetah.MakeTransparent(Color.Transparent);
            elephant.MakeTransparent(Color.Transparent);



            if (elephantapper)
            {
                g.DrawImage(elephant, xelephant, yelephant, elephant.Width / 5, elephant.Height / 5);
            }
            else
            {
                g.DrawImage(checkmarkelephant, checkx1, checky1, checkmarkelephant.Width / 5, checkmarkelephant.Height / 5);
                moveelephant = false;
                c1 = true;
            }
            if (alliapper)
            {
                g.DrawImage(alli, xalli, yalli, alli.Width / 5, alli.Height / 5);
            }
            else
            {
                g.DrawImage(checkmarkalli, checkx1T, checky1T, checkmarkalli.Width / 5, checkmarkalli.Height / 5);
                movealli = false;
                c2 = true;
            }
            if (cheetahapper)
            {
                g.DrawImage(cheetah, xcheetah, ycheetah, cheetah.Width / 5, cheetah.Height / 5);
            }
            else
            {
                g.DrawImage(checkmarkcheetah, checkx1F, checky1F, checkmarkcheetah.Width / 5, checkmarkcheetah.Height / 5);
                movecheetah = false;
                c3 = true;
            }
            if (c1 == true && c2 == true && c3 == true)
            {
                g.DrawImage(winning, 0, 0, winning.Width, winning.Height);
            }



            //DrawBall1(g, xBallT, yBallT, ballRadiusT);
            //DrawBall2(g, xBallF, yBallF, ballRadiusF);

        }

        private void ConnectToServer(string host, int portNumber)
        {
            try
            {
                tcpClient = new TcpClient(host, portNumber);
                stream = tcpClient.GetStream();
                Console.WriteLine("Connection Made! with " + host);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Connection Failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    byte[] receiveBuffer = new byte[1024];
                    int bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);

                    if (bytesRead == 0)
                    {
                        MessageBox.Show("Connection closed by the server.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    }
                    string receivedData = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                    Console.WriteLine($"Received data from the server: {receivedData}");


                    if (receivedData.Contains("abdelrahman") || receivedData.Contains("sohaila") || receivedData.Contains("Mohammad") || receivedData.Contains("reham"))
                    {

                        string[] parts = receivedData.Split(',');
                        if (parts.Length == 2)
                        {
                            data = parts[0].Trim();

                            this.Invalidate();
                        }
                    }


                    string[] coordinates = receivedData.Split(';');

                    if (coordinates.Length >= 2)
                    {
                        string[] pointingFingerCoordinates = coordinates[1].Split(',');


                        if (pointingFingerCoordinates.Length >= 3)
                        {
                            float x = float.Parse(pointingFingerCoordinates[0]);
                            float y = float.Parse(pointingFingerCoordinates[1]);
                            if (movealli == false && movecheetah == false)
                            {
                                Moveelephant(float.Parse(pointingFingerCoordinates[0]), float.Parse(pointingFingerCoordinates[1]));
                            }
                            if (moveelephant == false && movecheetah == false)
                            {
                                Movealli(float.Parse(pointingFingerCoordinates[0]), float.Parse(pointingFingerCoordinates[1]));
                            }
                            if (movealli == false && moveelephant == false)
                            {
                                Movecheetah(float.Parse(pointingFingerCoordinates[0]), float.Parse(pointingFingerCoordinates[1]));
                            }

                            else
                            {
                                Console.WriteLine("Invalid coordinate values received from the server.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid data format received from the server.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error receiving data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //elephant
        private void Moveelephant(float x, float y)
        {
            if (movealli == false && movecheetah == false)
            {

                if (ballTouchedelephant == false)
                {

                    int ballX = (xelephant + (elephant.Width / 10)) - 10;
                    int ballY = (yelephant + (elephant.Height / 10)) - 10;

                    int fingerX = (int)(x * this.ClientSize.Width);
                    int fingerY = (int)(y * this.ClientSize.Height);

                    double distance = Math.Sqrt(Math.Pow(ballX - fingerX, 2) + Math.Pow(ballY - fingerY, 2));

                    if (distance < 20 + Math.Min(elephant.Width, elephant.Height) / 10)
                    {
                        ballTouchedelephant = true;
                        moveelephant = true;
                    }

                }


                if (ballTouchedelephant == true && moveelephant == true)
                {
                    xelephant = (int)(x * this.ClientSize.Width) - (elephant.Width / 10);
                    yelephant = (int)(y * this.ClientSize.Height) - (elephant.Height / 10);
                }


                if (elephantapper && ballTouchedelephant)
                {
                    int elephantX = xelephant + (elephant.Width / 10);
                    int elephantY = yelephant + (elephant.Height / 10);

                    double elephantDistance = Math.Sqrt(Math.Pow(xBall - elephantX, 2) + Math.Pow(yBall - elephantY, 2));

                    if (elephantDistance < ballRadius)
                    {
                        elephantapper = false;

                    }
                }
            }

            this.Invalidate();
        }
        //------------------------------------------------------------------------
        //alli
        private void Movealli(float xt, float yt)
        {
            if (moveelephant == false && movecheetah == false)
            {

                if (ballTouchedalli == false)
                {

                    int ballXT = (xalli + (alli.Width / 10)) - 10;
                    int ballYT = (yalli + (alli.Height / 10)) - 10;

                    int fingerX = (int)(xt * this.ClientSize.Width);
                    int fingerY = (int)(yt * this.ClientSize.Height);

                    double distance = Math.Sqrt(Math.Pow(ballXT - fingerX, 2) + Math.Pow(ballYT - fingerY, 2));

                    if (distance < 20 + Math.Min(alli.Width, alli.Height) / 10)
                    {
                        ballTouchedalli = true;
                        movealli = true;
                    }


                }

                if (ballTouchedalli == true && movealli == true)
                {
                    xalli = (int)(xt * this.ClientSize.Width) - (alli.Width / 10);
                    yalli = (int)(yt * this.ClientSize.Height) - (elephant.Height / 10);
                }

                if (alliapper && ballTouchedalli)
                {
                    int alliX = xalli + (elephant.Width / 10);
                    int alliY = yalli + (elephant.Height / 10);

                    double alliDistance = Math.Sqrt(Math.Pow(xBallT - alliX, 2) + Math.Pow(yBallT - alliY, 2));
                    if (alliDistance < ballRadiusT)
                    {
                        alliapper = false;
                    }



                }
            }

            this.Invalidate();
        }
        //---------------------------------------------------------------------------------------------------
        //cheetah
        private void Movecheetah(float xf, float yf)
        {
            if (movealli == false && moveelephant == false)
            {

                if (ballTouchedcheetah == false)
                {

                    int ballXF = (xcheetah + (cheetah.Width / 10)) - 10;
                    int ballYF = (ycheetah + (cheetah.Height / 10)) - 10;

                    int fingerX = (int)(xf * this.ClientSize.Width);
                    int fingerY = (int)(yf * this.ClientSize.Height);

                    double distance = Math.Sqrt(Math.Pow(ballXF - fingerX, 2) + Math.Pow(ballYF - fingerY, 2));

                    if (distance < 20 + Math.Min(cheetah.Width, cheetah.Height) / 10)
                    {
                        ballTouchedcheetah = true;
                        movecheetah = true;
                    }

                }


                if (ballTouchedcheetah == true && movecheetah == true)
                {
                    xcheetah = (int)(xf * this.ClientSize.Width) - (cheetah.Width / 10);
                    ycheetah = (int)(yf * this.ClientSize.Height) - (cheetah.Height / 10);
                }


                if (cheetahapper && ballTouchedcheetah)
                {
                    int cheetahX = xcheetah + (cheetah.Width / 10);
                    int cheetahY = ycheetah + (elephant.Height / 10);

                    double cheetahDistance = Math.Sqrt(Math.Pow(xBallF - cheetahX, 2) + Math.Pow(yBallF - cheetahY, 2));

                    if (cheetahDistance < ballRadiusF)
                    {
                        cheetahapper = false;

                    }
                }
            }

            this.Invalidate();
        }
        //----------------------------------------------------------------------------------------


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            stream.Close();
            tcpClient.Close();
            Console.WriteLine("Connection terminated.");
        }
    }
}
