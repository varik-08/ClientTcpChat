using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientTcpChat
{
    public partial class Form1 : Form
    {
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
            listBox1.SetSelected(0, true);
        }

        private void button2_Click(object sender, EventArgs e) // отправка сообщения
        {
            if (textBox3.Text != "")
            {
                string nameRoom = "/room:" + listBox1.SelectedItem.ToString() + "/-";
                string message = textBox3.Text + nameRoom;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            textBox3.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                string name = textBox1.Text;
                client = new TcpClient();
                try
                {
                    client.Connect(host, port); //подключение клиента
                    stream = client.GetStream(); // получаем поток

                    string nameRoom = "/room:" + listBox1.SelectedItem.ToString() + "/-";
                    string message = name + nameRoom;

                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // запускаем новый поток для получения данных
                    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                    receiveThread.Start(); //старт потока
                    textBox2.Text += $"Добро пожаловать, {name}\r\n";
                    button3.Enabled = true;
                    textBox4.Enabled = true;
                    textBox3.Enabled = true;
                    button2.Enabled = true;
                    label5.Visible = true;
                    listBox1.Visible = true;
                }
                catch (Exception ex)
                {
                    textBox2.Text += ex.Message + "\r\n";
                }
            }
        }
        
        // получение сообщений
        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    //textBox2.Text += message + "\r\n";//вывод сообщения
                    this.Invoke(new MethodInvoker(() =>
                    {
                        if (textBox2.Text != "")
                        {
                            textBox2.Text = textBox2.Text + "\r\n" + message;
                        }
                        else
                        {
                            textBox2.Text = textBox2.Text + message;
                        }
                    }));
                }
                catch
                {
                    MessageBox.Show("Подключение прервано!");
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                string nameRoom = textBox4.Text;
                listBox1.Items.Add(nameRoom);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string nameRoom = "/room:" + listBox1.SelectedItem.ToString() + "/-";
            string message = "/quit/" + nameRoom;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Disconnect();
        }
    }
}
