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
using static fbnode.UserControl1;
using System.IO;
using System.Text.Json;
using System.Net.NetworkInformation;

namespace fbnode
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, ITMcraftNodeEntry
    {
        static TMcraftNodeAPI NodeUI;
        public static RecipesViewModel ViewModel {  get; set; }
        public UserControl1()
        {
            InitializeComponent();

            ViewModel = new RecipesViewModel();
            DataContext = ViewModel;
            recipe_reg = RecipeManager.LoadRecipes();
        }

        public void InitializeNode(TMcraftNodeAPI tmnodeapi)
        {
            NodeUI = tmnodeapi;
        }

        public void InscribeScript(ScriptWriteProvider scriptWriter)
        {

        }

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
        int[] errors = { 3, 5, 6, 7 };
        static public fbrecipe current_recipe = new fbrecipe();
        static public List<fbrecipe> recipe_reg;
        static public save_prompt save_Prompt = new save_prompt();
        static public int mode = 0;
        static public fbrecipe? selectedrecipe;
        static public List<VariableInfo>? global_variables;

        public struct fbrecipe
        {
            //type = 0: move
            public string name { get; set; }
            public int type { get; set; }
            public int movecount { get; set; }
            public int angle { get; set; }
            public int angleaux { get; set; }
            public int speed { get; set; }
            public int acc { get; set; }
            public int dec { get; set; }
        }
        //private void LoadRecipes()
        //{
        //    recipe_reg = RecipeManager.LoadRecipes();
        //    RecipeListBox.ItemsSource = recipe_reg;
        //    RecipeListBox.DisplayMemberPath = "Name";
        //}


        public static readonly DependencyProperty SharedAccProperty = DependencyProperty.Register("SharedAcc", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedAcc
        {
            get { return (int)GetValue(SharedAccProperty); }
            set { SetValue(SharedAccProperty, value); }
        }

        public static readonly DependencyProperty SharedDecProperty = DependencyProperty.Register("SharedDec", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedDec
        {
            get { return (int)GetValue(SharedDecProperty); }
            set { SetValue(SharedDecProperty, value); }
        }

        public static readonly DependencyProperty SharedSpeedProperty = DependencyProperty.Register("SharedSpeed", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedSpeed
        {
            get { return (int)GetValue(SharedSpeedProperty); }
            set { SetValue(SharedSpeedProperty, value); }
        }

        public static readonly DependencyProperty SharedAngleProperty = DependencyProperty.Register("SharedAngle", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedAngle
        {
            get { return (int)GetValue(SharedAngleProperty); }
            set { SetValue(SharedAngleProperty, value); }
        }

        public static readonly DependencyProperty SharedCCWAngleProperty = DependencyProperty.Register("SharedCCWAngle", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedCCWAngle
        {
            get { return (int)GetValue(SharedCCWAngleProperty); }
            set { SetValue(SharedCCWAngleProperty, value); }
        }

        public static readonly DependencyProperty SharedCountProperty = DependencyProperty.Register("SharedCount", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedCount
        {
            get { return (int)GetValue(SharedCountProperty); }
            set { SetValue(SharedCountProperty, value); }
        }

        public static readonly DependencyProperty ip1Property = DependencyProperty.Register("ip1", typeof(int), typeof(UserControl1), new PropertyMetadata(0, OnIp1PropertyChanged));
        public int ip1
        {
            get { return (int)GetValue(ip1Property); }
            set { SetValue(ip1Property, value); }
        }
        public static readonly DependencyProperty ip2Property = DependencyProperty.Register("ip2", typeof(int), typeof(UserControl1), new PropertyMetadata(0, OnIp2PropertyChanged));
        public int ip2
        {
            get { return (int)GetValue(ip2Property); }
            set { SetValue(ip2Property, value); }
        }
        public static readonly DependencyProperty ip3Property = DependencyProperty.Register("ip3", typeof(int), typeof(UserControl1), new PropertyMetadata(0, OnIp3PropertyChanged));
        public int ip3
        {
            get { return (int)GetValue(ip3Property); }
            set { SetValue(ip3Property, value); }
        }
        public static readonly DependencyProperty ip4Property = DependencyProperty.Register("ip4", typeof(int), typeof(UserControl1), new PropertyMetadata(0, OnIp4PropertyChanged));
        public int ip4
        {
            get { return (int)GetValue(ip4Property); }
            set { SetValue(ip4Property, value); }
        }

        private int saveparam(fbrecipe s_recipe, int option)
        {
            //option = 0: user saving attempt; 1: substitute
            //return = 0: succesfully saved; 1: name already exists;

            if (option == 0 || option == 1)
            {
                for (int i = 0; i < recipe_reg.Count; i++)
                {
                    if (recipe_reg[i].name == s_recipe.name)
                    {
                        if (option == 0) return 1;
                        else if (option == 1)
                        {
                            recipe_reg[i] = s_recipe;
                            return 0;
                        }
                    }
                }
                recipe_reg.Append(s_recipe);
                return 0;
            }
            else return -1;
        }
        private fbrecipe getparam(int type)
        {
            fbrecipe recipe = new fbrecipe();
            if(type == 0)
            {
                recipe.type = 0;
                recipe.movecount = 1;
                recipe.angle = (int)angleSlider.Value;
                recipe.angleaux = 0;
                recipe.speed = (int)speedSlider.Value;
                recipe.acc = (int)accSlider.Value;
                recipe.dec = (int)decSlider.Value;
            }
            return recipe;
        }
        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            current_recipe = getparam(0);
            mode = 0;
            save_Prompt.Show();
            
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
                TM_variables_synch();
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
            //saveparam();
            byte[] MySendByte = { 0, 0, 0 };
            if (errors.Contains(Sendcmd("RXV" + SharedSpeed * 3))) return;
            if (errors.Contains(Sendcmd("RXA" + SharedAcc * 3))) return;
            if (errors.Contains(Sendcmd("RXB" + SharedDec * 3))) return;
            if (errors.Contains(Sendcmd("FL" + SharedAngle * 3))) return;
        }

        private void Test_Shake_Click(object sender, RoutedEventArgs e)
        {

            //saveparam();
            byte[] MySendByte = { 0, 0, 0 };

            if (errors.Contains(Sendcmd("RXV" + SharedSpeed * 3))) return;
            if (errors.Contains(Sendcmd("RXA" + SharedAcc * 3))) return;
            if (errors.Contains(Sendcmd("RXB" + SharedDec * 3))) return;

            while (movecount > 0)
            {

                if (errors.Contains(Sendcmd("FL" + SharedAngle * 3))) return;
                if (errors.Contains(Sendcmd("FL" + SharedCCWAngle * 3))) return;
                movecount--;
            }
        }


        public int Sendcmd(string param)
        {
            byte[] paramBytes = ConstructMessageBytes(param);
            byte[] responseBuffer = new byte[20];

            using (Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString()+"."+ip2.ToString()+"."+ip3.ToString()+"."+ip4.ToString());
                int port = 7776;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(ipAddress);

                if (reply.Status == IPStatus.Success)
                {
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
                else
                {
                    MessageBox.Show("device not found");
                    return 7;
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

        public void TM_variables_synch()
        {
            NodeUI.VariableProvider.GetGlobalVariableList(ref global_variables);
            int parsedValue;
            if(global_variables != null)
            {

                if (NodeUI.VariableProvider.IsGlobalVariableExist("g_ip1"))
                {
                    int.TryParse(global_variables.FirstOrDefault(v => v.Name == "g_ip1").value, out parsedValue);
                    ip1 = parsedValue;
                }
                else
                {
                    NodeUI.VariableProvider.CreateGlobalVariable("ip1", VariableType.Integer, "0");
                }
                if (NodeUI.VariableProvider.IsGlobalVariableExist("g_ip2"))
                {
                    int.TryParse(global_variables.FirstOrDefault(v => v.Name == "g_ip2").value, out parsedValue);
                    ip2 = parsedValue;
                }
                else NodeUI.VariableProvider.CreateGlobalVariable("ip2", VariableType.Integer, "0");
                if (NodeUI.VariableProvider.IsGlobalVariableExist("g_ip3"))
                {
                    int.TryParse(global_variables.FirstOrDefault(v => v.Name == "g_ip3").value, out parsedValue);
                    ip3 = parsedValue;
                }
                else NodeUI.VariableProvider.CreateGlobalVariable("ip3", VariableType.Integer, "0");
                if (NodeUI.VariableProvider.IsGlobalVariableExist("g_ip4"))
                {
                    int.TryParse(global_variables.FirstOrDefault(v => v.Name == "g_ip4").value, out parsedValue);
                    ip4 = parsedValue;
                }
                else NodeUI.VariableProvider.CreateGlobalVariable("ip4", VariableType.Integer, "0");
            }
            else
            {
                NodeUI.VariableProvider.CreateGlobalVariable("ip1", VariableType.Integer, "0");
                NodeUI.VariableProvider.CreateGlobalVariable("ip2", VariableType.Integer, "0");
                NodeUI.VariableProvider.CreateGlobalVariable("ip3", VariableType.Integer, "0");
                NodeUI.VariableProvider.CreateGlobalVariable("ip4", VariableType.Integer, "0");
            }
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
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selectedrecipe = (fbrecipe)e.AddedItems[0];
            }
        }

        private void Load_click(object sender, RoutedEventArgs e)
        {
            if(selectedrecipe != null)
            {
                SharedAngle = selectedrecipe.Value.angle;
                SharedCCWAngle = selectedrecipe.Value.angleaux;
                SharedSpeed = selectedrecipe.Value.speed;
                SharedAcc = selectedrecipe.Value.acc;
                SharedDec = selectedrecipe.Value.dec;
            }
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            if (selectedrecipe != null)
            {
                save_Prompt.Show();
                save_Prompt.delete(selectedrecipe.Value);
            }
            else MessageBox.Show("No recipe selected");
        }
        private static void OnIp1PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as UserControl1;
            if (control != null)
            {
                string[] a = { "g_ip1", e.NewValue.ToString() };
                List<string[]> b = new List<string[]>(1); // Correctly initialize the list
                b.Add(a); // Add the array to the list
                if (NodeUI != null) NodeUI.VariableProvider.ChangeGlobalVariableValue(b);
            }
        }
        private static void OnIp2PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as UserControl1;
            if (control != null)
            {
                string[] a = { "g_ip2", e.NewValue.ToString() };
                List<string[]> b = new List<string[]>(1); // Correctly initialize the list
                b.Add(a); // Add the array to the list
                if (NodeUI != null) NodeUI.VariableProvider.ChangeGlobalVariableValue(b);
            }
        }
        private static void OnIp3PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as UserControl1;
            if (control != null)
            {
                string[] a = { "g_ip3", e.NewValue.ToString() };
                List<string[]> b = new List<string[]>(1); // Correctly initialize the list
                b.Add(a); // Add the array to the list
                if (NodeUI != null) NodeUI.VariableProvider.ChangeGlobalVariableValue(b);
            }
        }
        private static void OnIp4PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as UserControl1;
            if (control != null)
            {
                string[] a = { "g_ip4", e.NewValue.ToString() };
                List<string[]> b = new List<string[]>(1); // Correctly initialize the list
                b.Add(a); // Add the array to the list
                if (NodeUI != null) NodeUI.VariableProvider.ChangeGlobalVariableValue(b);
            }
        }

        private void ping_click(object sender, RoutedEventArgs e)
        {
            Ping pingSender = new Ping();
            IPAddress ipAddress = IPAddress.Parse(ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString());

            PingReply reply = pingSender.Send(ipAddress);

            if (reply.Status == IPStatus.Success) MessageBox.Show("Device found");
            else MessageBox.Show("Device not found");
        }

        private void default_ip(object sender, RoutedEventArgs e)
        {
            ip1 = 10;
            ip2 = 10;
            ip3 = 10;
            ip4 = 141;

        }
    }

}
