using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MovingObject
{
    public partial class Form1 : Form
    {
        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        int slide = 10;

        private TcpListener listener;
        private Thread serverThread;
        private List<TcpClient> clients = new List<TcpClient>();

        private int clientCount = 0;

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 50;
            timer1.Enabled = true;

            StartServer();
        }

        public int getRectX()
        {
            return rect.X;
        }

        private void StartServer()
        {
            serverThread = new Thread(() =>
            {
                try
                {
                    listener = new TcpListener(IPAddress.Any, 5000);
                    listener.Start();

                    while (true)
                    {
                        var client = listener.AcceptTcpClient();
                        lock (clients) clients.Add(client);

                        clientCount++;
                        int id = clientCount;

                        byte[] msg = Encoding.UTF8.GetBytes("ID|" + id.ToString());
                        client.GetStream().Write(msg, 0, msg.Length);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            back();

            rect.X += slide;
            Invalidate();

            Broadcast("POS|" + getRectX().ToString());
        }

        private void Broadcast(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            lock (clients)
            {
                for (int i = clients.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (clients[i].Connected)
                        {
                            clients[i].GetStream().Write(data, 0, data.Length);
                        }
                        else
                        {
                            clients.RemoveAt(i);
                        }
                    }
                    catch
                    {
                        clients.RemoveAt(i);
                    }
                }
            }
        }

        private void back()
        {
            if (rect.X >= this.Width - rect.Width * 2)
                slide = -10;
            else if (rect.X <= rect.Width / 2)
                slide = 10;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }
    }
}
