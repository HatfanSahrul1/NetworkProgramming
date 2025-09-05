using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MovingObjectClient
{
    public partial class Form1 : Form
    {
        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        private Thread clientThread;
        private int clientId = 0;

        public Form1()
        {
            InitializeComponent();
            this.ClientSize = new Size(800, 450);
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            clientThread = new Thread(() =>
            {
                try
                {
                    using var client = new TcpClient("127.0.0.1", 5000);
                    using var stream = client.GetStream();
                    var buffer = new byte[256];

                    while (true)
                    {
                        int bytes = stream.Read(buffer, 0, buffer.Length);
                        if (bytes > 0)
                        {
                            string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                            string[] parts = msg.Split('|');

                            if (parts[0] == "ID")
                            {
                                if (int.TryParse(parts[1], out int id))
                                    clientId = id;
                            }
                            else if (parts[0] == "POS")
                            {
                                if (int.TryParse(parts[1], out int posX))
                                {
                                    rect.X = posX;
                                    Invoke((MethodInvoker)(() => Invalidate()));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);

            if (clientId > 0)
            {
                using var font = new Font("Arial", 14);
                g.DrawString("Client " + clientId, font, Brushes.Black,
                    rect.X, rect.Y + 50);
            }
        }
    }
}
