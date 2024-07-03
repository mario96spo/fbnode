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
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace fbnode
{
    public class CommandTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StandardTemplate { get; set; }
        public DataTemplate MixTemplate { get; set; }
        public DataTemplate WaitStandardTemplate { get; set; }
        public DataTemplate WaitMixTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var command = (fbcommand)item;
            if (command.type == 0) return StandardTemplate;
            else if (command.type == 1) return WaitStandardTemplate;
            else if (command.type == 2) return MixTemplate;
            else return WaitMixTemplate;
        }
    }
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
            DataContext = this;
            RecipesFolderExists();
            RecipeManager.LoadReg();
            UpdateCombobox();
            recipe_combobox.Items.Clear();

            recipe_combobox.ItemsSource = recipe_reg;

            IpLoad();

            //recipe_reg_shake = RecipeManager.LoadRecipes(2);

        }

        public void InitializeNode(TMcraftNodeAPI tmnodeapi)
        {
            NodeUI = tmnodeapi;
        }

        public void InscribeScript(ScriptWriteProvider scriptWriter)
        {

        }

        private void RecipesFolderExists()
        {
            if (!Directory.Exists(recipesFolderPath))
            {
                Directory.CreateDirectory(recipesFolderPath);
            }
        }
        private void IpLoad()
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ip_address.txt");
            if (File.Exists(filePath))
            {
                string ip_address = File.ReadAllText(filePath).Trim();
                string[] ip_parts = ip_address.Split('.');
                ip1 = int.Parse(ip_parts[0]);
                ip2 = int.Parse(ip_parts[1]);
                ip3 = int.Parse(ip_parts[2]);
                ip4 = int.Parse(ip_parts[3]);
                ip1_textbox.Text = ip1.ToString();
                ip2_textbox.Text = ip2.ToString();
                ip3_textbox.Text = ip3.ToString();
                ip4_textbox.Text = ip4.ToString();

            }
            else
            {
                ip1 = 0;
                ip2 = 0;
                ip3 = 0;
                ip4 = 0;
                ip1_textbox.Text = "0";
                ip2_textbox.Text = "0";
                ip3_textbox.Text = "0";
                ip4_textbox.Text = "0";

                File.WriteAllText(filePath, "0.0.0.0");
            }
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
        static public fbcommand current_command = new fbcommand();
        static public Recipe current_recipe;
        static public List<Recipe> recipe_reg = new List<Recipe>();
        static public save_prompt save_Prompt = new save_prompt();
        static public int mode = 0; //mode= 0: move tab; 1: shake tab
        static public fbcommand? selected_command;
        static public List<VariableInfo>? global_variables;
        static public IPAddress ipaddress = IPAddress.Parse("10.10.10.141");
        static public int port = 7776;

        public static string recipesFolderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recipes");

        public class Recipe
        {
            public string Name { get; set; }
            public List<fbcommand> Commands { get; set; }

            public Recipe()
            {
                Commands = new List<fbcommand>();
            }
        }
        public struct fbcommand
        {
            //type = 0: standard; 1: wait-standard; 2: mix; 3: wait-mix
            public int seq_n { get; set; }
            public int type { get; set; }
            public int movecount { get; set; }
            public int angle { get; set; }
            public int angleaux { get; set; }
            public int speed { get; set; }
            public int acc { get; set; }
            public int dec { get; set; }
            public int wait_time { get; set; }
            public int flip_count { get; set; }
            public int flip_time { get; set; }
            public int blow_time { get; set; }
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
        public static readonly DependencyProperty SharedWaitProperty = DependencyProperty.Register("SharedWait", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedWait
        {
            get { return (int)GetValue(SharedWaitProperty); }
            set { SetValue(SharedWaitProperty, value); }
        }
        public static readonly DependencyProperty SharedFlipProperty = DependencyProperty.Register("SharedFlip", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedFlip
        {
            get { return (int)GetValue(SharedFlipProperty); }
            set { SetValue(SharedFlipProperty, value); }
        }
        public static readonly DependencyProperty SharedFlipcountProperty = DependencyProperty.Register("SharedBlow", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedFlipcount
        {
            get { return (int)GetValue(SharedFlipcountProperty); }
            set { SetValue(SharedFlipcountProperty, value); }
        }
        public static readonly DependencyProperty SharedBlowProperty = DependencyProperty.Register("SharedFlipcount", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedBlow
        {
            get { return (int)GetValue(SharedBlowProperty); }
            set { SetValue(SharedBlowProperty, value); }
        }

        public int ip1;
        public int ip2;
        public int ip3;
        public int ip4;

        private fbcommand getparam()
        {
            fbcommand command = new fbcommand();
            command.seq_n = current_recipe.Commands.Count;
            command.angle = (int)angleSlider.Value;
            command.speed = (int)speedSlider.Value;
            command.acc = (int)accSlider.Value;
            command.dec = (int)decSlider.Value;
            command.movecount = (int)countSlider.Value;
            command.wait_time = (int)waitSlider.Value;
            command.flip_count = (int)flipcountSlider.Value;
            command.flip_time = (int)flipSlider.Value;
            command.blow_time = (int)blowSlider.Value;
            if ((int)flipSlider.Value == 0 && (int)blowSlider.Value == 0 && (int)waitSlider.Value == 0) command.type = 0;
            else if ((int)flipSlider.Value == 0 && (int)blowSlider.Value == 0 && (int)waitSlider.Value != 0) command.type = 1;
            else if ((int)waitSlider.Value == 0) command.type = 2;
            else if ((int)waitSlider.Value != 0) command.type = 3;
            if ((int)countSlider.Value > 1) command.angleaux = 0;
            else command.angleaux = (int)angleccwSlider.Value;
            return command;
    }
        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            current_command = getparam();
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
                //TM_variables_synch();
                //Text_Title.Text = str_config1;


            }
        }
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            byte[] MySendByte = { 0, 0, 0 };
            if(mode == 0)
            {

                if (errors.Contains(Sendcmd("RXV" + SharedSpeed * 3))) return;
                if (errors.Contains(Sendcmd("RXA" + SharedAcc * 3))) return;
                if (errors.Contains(Sendcmd("RXB" + SharedDec * 3))) return;
                
                movecount = SharedCount;

                while (movecount > 0)
                {

                    if (errors.Contains(Sendcmd("FL" + SharedAngle * 3))) return;
                    if (errors.Contains(Sendcmd("FL" + SharedCCWAngle * 3))) return;
                    movecount--;
                }
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
                            /*if (param.Substring(0, 2) == "FL")
                            {
                                while (response != "SC=0001")
                                {
                                    sender.Send(ConstructMessageBytes("SC"));
                                    bytesRead = sender.Receive(responseBuffer);
                                    response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                                }
                            }*/
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
            _TMscript = "Socket ntd_fb = \"" + ipaddress.ToString() + "\", " + port + "\n" +
                        "string cmd = " + cmd[0] + "\n" +
                        "byte[] cmd_byte = String_ToByte(cmd)\n" +
                        "byte[] msg = {0, 7, 13}\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)\n" +
                        "socket_send(\"ntd_fb\", msg)\n" +
                        "cmd = " + cmd[1] + "\n" +
                        "cmd_byte = String_ToByte(cmd)\n" +
                        "msg = {0, 7, 13}\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)\n" +
                        "socket_send(\"ntd_fb\", msg)\n" +
                        "cmd = " + cmd[2] + "\n" +
                        "cmd_byte = String_ToByte(cmd)\n" +
                        "msg = {0, 7, 13}\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)\n" +
                        "socket_send(\"ntd_fb\", msg)\n" +
                        "cmd = " + cmd[3] + "\n" +
                        "cmd_byte = String_ToByte(cmd)\n" +
                        "msg = {0, 7, 13}\n" +
                        "msg = Array_Insert(msg, 2, cmd_byte)\n" +
                        "socket_send(\"ntd_fb\", msg)";
            ScriptWriteProvider scriptWriter = new ScriptWriteProvider();
            scriptWriter.AppendScript(_TMscript);
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                selected_command = (fbcommand)e.AddedItems[0];
                Load_click();
            }
        }

        private void Load_click()
        {
            save_to_current_button.IsEnabled = true;
            save_to_current_button.Opacity = 1;
            current_command.seq_n = selected_command.Value.seq_n;
            current_textblock.Text = selected_command.Value.seq_n.ToString();
            current_textblock.Visibility = Visibility.Visible;
            current_x_button.Visibility = Visibility.Visible;

            SharedAngle = selected_command.Value.angle;
            SharedSpeed = selected_command.Value.speed;
            SharedAcc = selected_command.Value.acc;
            SharedDec = selected_command.Value.dec;
            SharedCCWAngle = selected_command.Value.angleaux;
            SharedCount = selected_command.Value.movecount;
            SharedWait = selected_command.Value.wait_time;
            SharedFlipcount = selected_command.Value.flip_count;
            SharedFlip = selected_command.Value.flip_time;
            SharedBlow = selected_command.Value.blow_time;
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            if (current_recipe != null)
            {
                save_Prompt.Show();
                save_Prompt.delete(current_recipe);
                //save_Prompt.delete(selected_command.Value);
            }
            else MessageBox.Show("no recipe selected");
        }

        private void ping_click(object sender, RoutedEventArgs e)
        {
            string ip = ip1_textbox.Text+"."+ip2_textbox.Text+"."+ip3_textbox.Text+"."+ip4_textbox.Text;
            Ping pingSender = new Ping();
            IPAddress ipAddress = IPAddress.Parse(ip);
            try
            {
                PingReply reply = pingSender.Send(ipAddress);

                if (reply.Status == IPStatus.Success) MessageBox.Show("Device found");
                else MessageBox.Show("Device not found");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ping error.");
            }
        }

        private void save_to_current_Click(object sender, RoutedEventArgs e)
        {
            current_command = getparam();
            current_command.seq_n = int.Parse(current_textblock.Text);
            //save_Prompt.save_reg(current_command, 1);
            //ViewModel.LoadRecipes();
        }

        public void current_x_Click(object sender, RoutedEventArgs e)
        {
            current_x_hide();
            selected_command = null;
        }
        public void current_x_hide()
        {
            current_textblock.Visibility = Visibility.Collapsed;
            current_x_button.Visibility = Visibility.Collapsed;
            current_textblock.Text = "";
            save_to_current_button.IsEnabled = false;
            save_to_current_button.Opacity = 0.5;
        }
        public void new_recipe_click(object sender, RoutedEventArgs e)
        {
            save_Prompt.Show();
            save_Prompt.reset();
        }

        private void add_click(object sender, RoutedEventArgs e)
        {
            current_recipe.Commands.Add(getparam());
            if (RecipeManager.UpdateRecipe(current_recipe))
            {
                RecipeManager.LoadReg();
                update_listbox();

                command_listbox.Items.Refresh();
            }
        }
        public void UpdateCombobox()
        {
            recipe_combobox.Items.Clear();
            foreach (var recipe in recipe_reg)
            {
                recipe_combobox.Items.Add(recipe.Name);
            }
        }
        private void recipe_combobox_DropDownOpened(object sender, EventArgs e)
        {
            RecipeManager.LoadReg();
            UpdateCombobox();
        }
        private void combobox_selectionchanged(object sender, RoutedEventArgs e)
        {
            current_recipe = recipe_combobox.SelectedItem as Recipe;
            update_listbox();
        }
        private void update_listbox()
        {
            if (current_recipe != null)
            {
                command_listbox.ItemsSource = current_recipe.Commands;
            }
            else MessageBox.Show(".");
        }

        private void remove_click(object sender, RoutedEventArgs e)
        {
            fbcommand f_command = current_recipe.Commands.Find(x => x.seq_n == current_command.seq_n);
            var temp = new fbcommand();
            for (int i = f_command.seq_n + 1; i < current_recipe.Commands.Count; i++)
            {
                temp = current_recipe.Commands[i];
                temp.seq_n--;
                current_recipe.Commands[i] = temp;
            }
            current_recipe.Commands.RemoveAt(f_command.seq_n);
            RecipeManager.UpdateRecipe(current_recipe);
            command_listbox.Items.Refresh();
        }



        private void edit_ip_click(object sender, RoutedEventArgs e)
        {
            ip1_textbox.IsEnabled = true;
            ip1_textbox.Opacity = 1;
            ip2_textbox.IsEnabled = true;
            ip2_textbox.Opacity = 1;
            ip3_textbox.IsEnabled = true;
            ip3_textbox.Opacity = 1;
            ip4_textbox.IsEnabled = true;
            ip4_textbox.Opacity = 1;
        }

        private void save_ip_click(object sender, RoutedEventArgs e)
        {
            ip1 = int.Parse(ip1_textbox.Text);
            ip2 = int.Parse(ip2_textbox.Text);
            ip3 = int.Parse(ip3_textbox.Text);
            ip4 = int.Parse(ip4_textbox.Text);
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ip_address.txt");
            File.WriteAllText(filePath, ip1_textbox.Text + "." + ip2_textbox.Text + "." + ip3_textbox.Text + "." + ip4_textbox.Text);
            ip1_textbox.IsEnabled = false;
            ip1_textbox.Opacity = 0.7;
            ip2_textbox.IsEnabled = false;
            ip2_textbox.Opacity = 0.7;
            ip3_textbox.IsEnabled = false;
            ip3_textbox.Opacity = 0.7;
            ip4_textbox.IsEnabled = false;
            ip4_textbox.Opacity = 0.7;
        }

        private void test_recipe_click(object sender, RoutedEventArgs e)
        {

        }
    }

}
