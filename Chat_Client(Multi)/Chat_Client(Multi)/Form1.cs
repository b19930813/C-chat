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

namespace Chat_Client_Multi_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;//關閉檢查
        }
        //socket&執行續
        Socket socketClient = null;
        Thread threadClient = null;
        public const int SendBufferSize = 2 * 1024;
        public const int ReceiveBufferSize = 8 * 1024;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnConnectToServer_Click(object sender, EventArgs e)
        {
            //指定socket的聯繫方法
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //由IF輸入IP
            IPAddress serverIPAddress = IPAddress.Parse(txtIP.Text.Trim());
            //IF的Port
            int serverPort = int.Parse(txtPort.Text.Trim());
            //宣告一個IPEndPoint給予一個完整的IP+Port
            IPEndPoint endpoint = new IPEndPoint(serverIPAddress, serverPort);
            //藉由endpoint建立起socket連線
            socketClient.Connect(endpoint);
            //建立新的執行續來紀錄聊天訊息
            threadClient = new Thread(RecMsg);
            //允許在背景運作
            threadClient.IsBackground = true;
            //開始執行囉
            threadClient.Start();
            txtMsg.AppendText("連上服務端囉~可以開始聊天了!\r\n");
            //防呆，按鈕封印
            btnConnectToServer.Enabled = false;
        }

        private void RecMsg()
        {
           while(true)//持續Watch服務端丟出來的訊息
            {
                string strRecMsg = null;
                int length = 0;
                byte[] buffer = new byte[SendBufferSize];
                try
                {
                    //將客戶端收到的訊息丟到buffer+抓其的長度
                    length = socketClient.Receive(buffer);
                }
                catch(SocketException ex)//socket的錯誤例外
                {
                    txtMsg.AppendText("發生錯誤:"+ex.Message+"\r\n");
                    txtMsg.AppendText("已中斷與服務端的連線\r\n");
                    break;
                }
                catch(Exception ex)//一般的錯誤例外
                {
                    txtMsg.AppendText("系統錯誤，錯誤訊息為:"+ex.Message+"\r\n");
                    break;
                }
                //轉換UTF8編碼，避免亂碼
                strRecMsg = Encoding.UTF8.GetString(buffer, 0, length);
                //顯示格式為XXX+時間+XXX+訊息
                txtMsg.AppendText("服務端在" + GetCurrectTime() + "丟了訊息給你" +"\r\n"+ strRecMsg + "\r\n");
            }
        }

        private DateTime GetCurrectTime()//抓時間的沒什麼好解釋
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        private void btnCSend_Click(object sender, EventArgs e)
        {
            ClientSendMsg(txtCMsg.Text, 0);
        }

        private void ClientSendMsg(string sendMsg, byte symbol)
        {
            byte[] arrClientMsg = Encoding.UTF8.GetBytes(sendMsg);
            socketClient.Send(arrClientMsg);
            string username;
            if(txtName.Text.Trim()=="")
            {
                username = "NoNameRobot";
            }      
            else
            {
                username = txtName.Text.Trim();
            }
            txtMsg.AppendText(username + GetCurrectTime() + "\r\n"+sendMsg+"\r\n");
            txtCMsg.Text= "";
        }

        private void txtCMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                ClientSendMsg(txtCMsg.Text, 0);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
