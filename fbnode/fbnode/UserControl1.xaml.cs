using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TMcraft;

namespace fbnode
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, ITMcraftNodeEntry
    {
        TMcraftNodeAPI NodeUI;
        public UserControl1()
        {
            InitializeComponent();
        }

        public void InitializeNode(TMcraftNodeAPI tmnodeapi)
        {
            NodeUI = tmnodeapi;
        }

        public void InscribeScript(ScriptWriteProvider scriptWriter)
        {

        }

        bool fgSave = false;
        string _TMscript = string.Empty;
        string script_write_1 = string.Empty;
        string script_write_2 = string.Empty;
        int movecount;
        int angle;
        int angleaux;
        int speed;
        int acc;
        int dec;
        static float command;
        int flipcount = 1;
        int fliptontime = 10;
        int flipofftime = 50;
        int airblowtime = 200;
        bool backlighton = true;
        int[] errors = { 3, 5, 6 };
        fbrecipe simrecipe = new fbrecipe();

        public struct fbrecipe
        {
            public int movecount;
            public int angle;
            public int angleaux;
            public int speed;
            public int acc;
            public int dec;
        }


        public static readonly DependencyProperty SharedAccProperty = DependencyProperty.Register("SharedAcc", typeof(double), typeof(UserControl1), new PropertyMetadata(0.0));
        public double SharedAcc
        {
            get { return (double)GetValue(SharedAccProperty); }
            set { SetValue(SharedAccProperty, value); }
        }

        public static readonly DependencyProperty SharedDecProperty = DependencyProperty.Register("SharedDec", typeof(double), typeof(UserControl1), new PropertyMetadata(0.0));
        public double SharedDec
        {
            get { return (double)GetValue(SharedDecProperty); }
            set { SetValue(SharedDecProperty, value); }
        }

        public static readonly DependencyProperty SharedSpeedProperty = DependencyProperty.Register("SharedSpeed", typeof(double), typeof(UserControl1), new PropertyMetadata(0.0));
        public double SharedSpeed
        {
            get { return (double)GetValue(SharedSpeedProperty); }
            set { SetValue(SharedSpeedProperty, value); }
        }

        public static readonly DependencyProperty SharedAngleProperty = DependencyProperty.Register("SharedAngle", typeof(double), typeof(UserControl1), new PropertyMetadata(0.0));
        public double SharedAngle
        {
            get { return (double)GetValue(SharedAngleProperty); }
            set { SetValue(SharedAngleProperty, value); }
        }

        public static readonly DependencyProperty SharedCCWAngleProperty = DependencyProperty.Register("SharedCCWAngle", typeof(double), typeof(UserControl1), new PropertyMetadata(0.0));
        public double SharedCCWAngle
        {
            get { return (double)GetValue(SharedCCWAngleProperty); }
            set { SetValue(SharedCCWAngleProperty, value); }
        }

        public static readonly DependencyProperty SharedCountProperty = DependencyProperty.Register("SharedCount", typeof(double), typeof(UserControl1), new PropertyMetadata(0.0));
        public double SharedCount
        {
            get { return (double)GetValue(SharedCountProperty); }
            set { SetValue(SharedCountProperty, value); }
        }

        private void saveparam()
        {
            /*movecount = (int)countSlider.Value;
            angle = (int)angleSlider.Value;
            angleaux = (int)angleccwSlider.Value;
            speed = (int)speedSlider.Value;
            acc = (int)accSlider.Value;
            dec = (int)decSlider.Value;
            flipcount = 1;
            fliptontime = 10;
            flipofftime = 50;
            airblowtime = 200;
            backlighton = true;*/
            simrecipe.movecount = (int)countSlider.Value;
            simrecipe.angle = (int)angleSlider.Value;
            simrecipe.angleaux = (int)angleccwSlider.Value;
            simrecipe.speed = (int)speedSlider.Value;
            simrecipe.acc = (int)accSlider.Value;
            simrecipe.dec = (int)decSlider.Value;
        }
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {

            if (NodeUI == null)
            {
                MessageBox.Show("No connection with TMflow...");
            }
            else
            {
                saveparam();
                ////////////// CREATE FLEXIBOWL VARIABLES IN TMFLOW /////////////////
                /*NodeUI.VariableProvider.CreateProjectVariable("fb_speed", VariableType.Float, speed.ToString());
                NodeUI.VariableProvider.CreateProjectVariable("fb_acc", VariableType.Float, acc.ToString());
                NodeUI.VariableProvider.CreateProjectVariable("fb_dec", VariableType.Float, dec.ToString());
                NodeUI.VariableProvider.CreateProjectVariable("fb_angle", VariableType.Float, angle.ToString());*/
                /////////////////////////////////////////////////////////////////////



                /*string str = Text_Title.Text;
                _TMscript = "Display(\"Green\", \"Black\", \"" + str + "\", \"Training Center NO.1\")"+"\n" +
                            "IO[\"ControlBox\"].DO[1] = \"" + script_write_1 + "\"\n" +
                            "Robot[0].CameraLight = \"" + script_write_2 + "\"\n";
                NodeUI.DataStorageProvider.SaveData("config1", str);*/
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (NodeUI == null)
            {
                MessageBox.Show("No connection with TMflow...");
            }
            else
            {
                string str_config1 = string.Empty;
                NodeUI.DataStorageProvider.GetData("config1", out str_config1);
                //Text_Title.Text = str_config1;
            }
        }
        private void Light_Checked(object sender, RoutedEventArgs e)
        {
            script_write_1 = "1";
            NodeUI.IOProvider.SetCameraLight(true);
        }
        private void Light_unChecked(object sender, RoutedEventArgs e)
        {
            script_write_1 = "0";
            NodeUI.IOProvider.SetCameraLight(false);
        }

        private void DO1_Checked(object sender, RoutedEventArgs e)
        {
            script_write_2 = "1";
            NodeUI.IOProvider.WriteDigitOutput(IO_TYPE.CONTROL_BOX, 0, 0, true);
        }

        private void DO1_unChecked(object sender, RoutedEventArgs e)
        {
            script_write_2 = "0";
            NodeUI.IOProvider.WriteDigitOutput(IO_TYPE.CONTROL_BOX, 0, 0, false);
        }

        private void Test_Move_Click(object sender, RoutedEventArgs e)
        {
            saveparam();
            byte[] MySendByte = { 0, 0, 0 };
            if (errors.Contains(Sendcmd("RXV" + speed * 3))) return;
            if (errors.Contains(Sendcmd("RXA" + acc * 3))) return;
            if (errors.Contains(Sendcmd("RXB" + dec * 3))) return;
            if (errors.Contains(Sendcmd("FL" + angle * 3))) return;
        }

        private void Test_Shake_Click(object sender, RoutedEventArgs e)
        {

            saveparam();
            byte[] MySendByte = { 0, 0, 0 };

            if (errors.Contains(Sendcmd("RXV" + speed * 3))) return;
            if (errors.Contains(Sendcmd("RXA" + acc * 3))) return;
            if (errors.Contains(Sendcmd("RXB" + dec * 3))) return;

            while (movecount > 0)
            {

                if (errors.Contains(Sendcmd("FL" + angle * 3))) return;
                if (errors.Contains(Sendcmd("FL" + angleaux * 3))) return;
                movecount--;
            }
        }


        static int Sendcmd(string param)
        {
            byte[] paramBytes = ConstructMessageBytes(param);
            byte[] responseBuffer = new byte[20];

            using (Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse("10.10.10.143");
                int port = 7776;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    // Send the data through the socket
                    int bytesSent = sender.Send(paramBytes);
                    if (bytesSent != paramBytes.Length)
                    {
                        MessageBox.Show("FLEXIBOWL: COMMUNICATION ERROR");
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                        return 6;
                    }

                    // Receive the response from the server
                    int bytesRead = 0;
                    string response = "";
                    while (bytesRead == 0)
                    {
                        bytesRead = sender.Receive(responseBuffer);
                        response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                    }

                    if (!(responseBuffer[0] == 0 && responseBuffer[1] == 7 && responseBuffer[bytesRead - 1] == 13))
                    {
                        MessageBox.Show("FLEXIBOWL: COMMUNICATION ERROR");
                        return 6;
                    }
                    else if (response == "%")
                    {
                        if (param.Substring(0, 2) == "FL")
                        {
                            while (response != "SC=0001")
                            {
                                Debug.WriteLine(response);
                                sender.Send(ConstructMessageBytes("SC"));
                                bytesRead = sender.Receive(responseBuffer);
                                response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                            }
                        }
                        command = 0;
                        return 1;
                    }
                    else if (response == "*")
                    {
                        command = 0;
                        return 2;
                    }
                    else if (response == "?")
                    {
                        command = int.Parse(response.Substring(1));
                        MessageBox.Show("FLEXIBOWL: UNDEFINED COMMAND (" + param + ") - ERROR " + command);
                        return 3;
                    }
                    else if (response.Substring(0, param.Length + 1) == param + "=")
                    {
                        command = int.Parse(response.Substring(param.Length + 1));
                        if (param == "BS") return -1;
                        if (param == "SC") return int.Parse(response.Substring(3));
                        return 4;
                    }
                    else
                    {
                        command = 0;
                        MessageBox.Show("FLEXIBOWL: COMMAND ERROR (" + param + ")");
                        return 5;
                    }
                }
                catch (Exception e)
                {
                    command = 0;
                    MessageBox.Show("An exception occurred: {0}", e.ToString());
                    return 6;
                }
            }
        }
        static byte[] ConstructMessageBytes(string message)
        {
            byte[] messageBytes = new byte[3 + message.Length];
            messageBytes[0] = 0;
            messageBytes[1] = 7;
            Encoding.ASCII.GetBytes(message, 0, message.Length, messageBytes, 2);
            messageBytes[message.Length + 2] = 13;
            return messageBytes;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (NodeUI == null)
            {
                MessageBox.Show("No connection with TMflow ...");
            }
            else
            {

                NodeUI.VisionProvider.CreateVisionJob("v_job_1");

            }
        }

        private void append_button_Click(object sender, RoutedEventArgs e)
        {
            string[] cmd = { "\"RXV" + speed * 3 + "\"", "\"RXA" + acc * 3 + "\"", "\"RXB" + dec * 3 + "\"", "\"FL" + angle * 3 + "\"" };
            _TMscript = "string cmd = " + cmd[0] + "\n" +
                        "byte[] cmd_byte = String_ToByte(cmd)" + "\n" +
                        "byte[] msg = {0, 7, 13}" + "\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)" + "\n" +
                        "socket_send(\"ntd_flexibowl\", msg)" + "\n" +
                        "cmd = " + cmd[1] + "\n" +
                        "cmd_byte = String_ToByte(cmd)" + "\n" +
                        "msg = {0, 7, 13}" + "\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)" + "\n" +
                        "socket_send(\"ntd_flexibowl\", msg)" + "\n" +
                        "cmd = " + cmd[2] + "\n" +
                        "cmd_byte = String_ToByte(cmd)" + "\n" +
                        "msg = {0, 7, 13}" + "\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)" + "\n" +
                        "socket_send(\"ntd_flexibowl\", msg)" + "\n" +
                        "cmd = " + cmd[3] + "\n" +
                        "cmd_byte = String_ToByte(cmd)" + "\n" +
                        "msg = {0, 7, 13}" + "\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)" + "\n" +
                        "socket_send(\"ntd_flexibowl\", msg)";
            ScriptWriteProvider scriptWriter = new ScriptWriteProvider();
            scriptWriter.AppendScript(_TMscript);
        }
    }

}
