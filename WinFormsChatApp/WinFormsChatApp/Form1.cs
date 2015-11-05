using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;


namespace WinFormsChatApp
{
    public partial class Form1 : Form
    {
        Socket sck;
        EndPoint epLocal, epRemote;
        byte[] buffer;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // get user ip
            textLocalIp.Text = GetLocalIp();
            textRemoteIp.Text = GetLocalIp();
        }

        private string GetLocalIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                      return ip.ToString();
                }
            }

            return "127.0.0.1";
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                //binding socket
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text), Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);
                // connecting to remote IP
                epRemote = new IPEndPoint(IPAddress.Parse(textRemoteIp.Text), Convert.ToInt32(textRemotePort.Text));
                sck.Connect(epRemote);

                //listening specific port
                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);

                lblConStatus.Text = "Connected!";
                buttonDisconnect.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed", ex.ToString());
            }
        
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                byte[] receivedData = new byte[1500];
                receivedData = (byte[])aResult.AsyncState;
                // converting recieved bytes to string
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                string receivedMessage = aEncoding.GetString(receivedData);

                //adding message to listbox
                listMessage.Items.Add("Friend: " + receivedMessage);

                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (lblConStatus.Text == "Connected!")
            {
                //convert string message to byte
                ASCIIEncoding aEncoding = new ASCIIEncoding();
                byte[] sendingMessage = new byte[1500];
                sendingMessage = aEncoding.GetBytes(textMessage.Text);
                //sending message
                sck.Send(sendingMessage);
                //adding message to listbox
                listMessage.Items.Add("Me: " + textMessage.Text);
                textMessage.Text = "";
            }
            
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            epLocal = null;
            epRemote = null;
            lblConStatus.Text = "Connection status";
            buttonDisconnect.Visible = false;
        }
    }
}
