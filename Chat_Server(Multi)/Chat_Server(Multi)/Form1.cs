using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_Server_Multi_
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;//防止錯誤跳出
        }
        Socket socketWatch = null;
        Thread threadWatch = null;

        public const int SendBufferSizes = 2 * 1024;
        public const int ReceiveBufferSizes = 8 * 1024;

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            /*以下都是
             * 連線的
             * 資訊
             */
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipaddress = GetLocalIPv4Address();
            lblIP.Text = ipaddress.ToString();
            int port = 9487;
            lblPort.Text = port.ToString();
            IPEndPoint endpoint = new IPEndPoint(ipaddress, port);
            socketWatch.Bind(endpoint);
            socketWatch.Listen(20);
            threadWatch = new Thread(WatchConnecting);
            threadWatch.IsBackground = true;
            threadWatch.Start();
            txtMsg.AppendText("啟動囉，可以接受訊息" + "\r\n");
            btnStartServer.Enabled = false;
        }
        Dictionary<string, Socket> dicSocket = new Dictionary<string ,Socket>();
        Socket socConnection = null;
        string clientName = null;
        IPAddress clientIP;
        int clientPort;
        private void WatchConnecting()
        {
            while (true)
            {
                try
                {
                    socConnection = socketWatch.Accept();
                }
                catch(Exception ex)
                {
                    txtMsg.AppendText(ex.Message);
                    break;
                }
                clientIP = (socConnection.RemoteEndPoint as IPEndPoint).Address;
                clientPort = (socConnection.RemoteEndPoint as IPEndPoint).Port;
                clientName = "IP: " + clientIP + " Port: " + clientPort;

                lstClients.Items.Add(clientName); 
                dicSocket.Add(clientName, socConnection); 

               
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ServerRecMsg);
                Thread thread = new Thread(pts);
                thread.IsBackground = true;
            
                thread.Start(socConnection);
                txtMsg.AppendText("IP: " + clientIP + " Port: " + clientPort + " 可以聊天囉\r\n");
            }
        }

        private void ServerRecMsg(object socketClientPara)
        {
            Socket socketServer = socketClientPara as Socket;
            while (true)
            {
                byte[] arrServerRecMsg = new byte[ReceiveBufferSizes];
          
                int length = socketServer.Receive(arrServerRecMsg);
           
                string strSRecMsg = Encoding.UTF8.GetString(arrServerRecMsg, 0, length);
          
                txtMsg.AppendText("Frank:" + GetCurrentTime() + "\r\n"+ strSRecMsg + "\r\n");
            }
        }

        private IPAddress GetLocalIPv4Address()
        {
            IPAddress localIPv4 = null;
            IPAddress[] ipAddressList = Dns.GetHostAddresses(Dns.GetHostName());
            foreach(IPAddress ipAddress in ipAddressList)
            {
                if(ipAddress.AddressFamily==AddressFamily.InterNetwork)
                {
                    localIPv4 = ipAddress;
                }
                else
                {
                    continue;
                }
            }
            return localIPv4;
        }

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            ServerSendMsg(txtSendMsg.Text);
        }

        private void ServerSendMsg(string sendMsg)
        {


            byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            socConnection.Send(arrSendMsg);
            txtMsg.AppendText("客戶端:" + GetCurrentTime() + "\r\n" + sendMsg + "\r\n");
            txtSendMsg.Text = "";

        }

        private DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                ServerSendMsg(txtSendMsg.Text);
            }
        }
    }
}
