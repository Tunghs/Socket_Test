using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace Socket_Test
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> messageList = new ObservableCollection<string>();
        private string chattingPartner = "";

        private string strSendMsg;
        private TcpClient server;
        private NetworkStream ns;

        private bool isRunning = true;

        public MainWindow()
        {
            InitializeComponent();

            chattingPartner = "테스트";
            messageListView.ItemsSource = messageList;

            try
            {
                string strRecvMsg;

                server = new TcpClient("127.0.0.1", 9090); //소켓생성,커넥트
                ns = server.GetStream();
                Thread recvThread = new Thread(new ThreadStart(RecvThread));
                recvThread.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void RecvThread()
        {
            byte[] buffer = new byte[1024];
            string msg;

            while (isRunning)
            {
                try
                {
                    ns.Read(buffer, 0, buffer.Length);
                    msg = Encoding.ASCII.GetString(buffer);
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        messageList.Add(msg);
                    }));
                    
                    // serverMessage.Invoke(new LogToForm(Log), new object[] { msg });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void Send_btn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Send_Text_Box.Text))
                return;

            string message = Send_Text_Box.Text;

            if (chattingPartner != null)
            {
                messageList.Add("나: " + message);
                strSendMsg = message;
                Send_Text_Box.Clear();
            }

            if (VisualTreeHelper.GetChildrenCount(messageListView) > 0) 
            {
                Border border = (Border)VisualTreeHelper.GetChild(messageListView, 0); 
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom(); 
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) 
        { 
            if (e.Key == Key.Enter) 
            { 
                if (string.IsNullOrEmpty(Send_Text_Box.Text)) 
                    return; 
                
                string message = Send_Text_Box.Text; 
                
                if (chattingPartner != null) 
                { 
                    messageList.Add("나: " + message); 
                    Send_Text_Box.Clear(); 
                } 
                if (VisualTreeHelper.GetChildrenCount(messageListView) > 0) 
                { 
                    Border border = (Border)VisualTreeHelper.GetChild(messageListView, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0); 
                    scrollViewer.ScrollToBottom(); 
                } 
            } 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isRunning = false;

            ns.Close();
            server.Close();
        }
    }
}
