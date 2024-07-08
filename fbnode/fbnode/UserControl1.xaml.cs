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
using System.Diagnostics.Metrics;
using System.Linq;
using System.Windows.Threading;
using System.Reflection;

namespace fbnode
{
    public class CommandTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BlowtimeActiveTemplate { get; set; }
        public DataTemplate BlowtimeActive_Wait_Template { get; set; }
        public DataTemplate BlowtimeInactiveTemplate { get; set; }
        public DataTemplate BlowtimeInactive_Wait_Template { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var command = (fbcommand)item;
            if (command.type == 0) return BlowtimeActiveTemplate;
            else if (command.type == 1) return BlowtimeActive_Wait_Template;
            else if (command.type == 2) return BlowtimeInactiveTemplate;
            else return BlowtimeInactive_Wait_Template;
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
            string filePath2 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
            if (File.Exists(filePath))
            {
                string ip_address = File.ReadAllText(filePath).Trim();
                string port_file = File.ReadAllText(filePath2).Trim();
                string[] ip_parts = ip_address.Split('.');
                ip1 = int.Parse(ip_parts[0]);
                ip2 = int.Parse(ip_parts[1]);
                ip3 = int.Parse(ip_parts[2]);
                ip4 = int.Parse(ip_parts[3]);
                port = int.Parse(port_file);
                ip1_textbox.Text = ip1.ToString();
                ip2_textbox.Text = ip2.ToString();
                ip3_textbox.Text = ip3.ToString();
                ip4_textbox.Text = ip4.ToString();
                port_textbox.Text = port.ToString();

            }
            else
            {
                ip1 = 0;
                ip2 = 0;
                ip3 = 0;
                ip4 = 0;
                port = 0;
                ip1_textbox.Text = "0";
                ip2_textbox.Text = "0";
                ip3_textbox.Text = "0";
                ip4_textbox.Text = "0";
                port_textbox.Text = "0";

                File.WriteAllText(filePath, "0.0.0.0");
                File.WriteAllText(filePath2, "0");

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
        static public IPAddress ipaddress;

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
            //type = 0: blowtime -> user defined; 1: blowtime -> command duration
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
            public bool? blow_active { get; set; }
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
        public static readonly DependencyProperty SharedFlipcountProperty = DependencyProperty.Register("SharedFlipcount", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedFlipcount
        {
            get { return (int)GetValue(SharedFlipcountProperty); }
            set { SetValue(SharedFlipcountProperty, value); }
        }
        public static readonly DependencyProperty SharedBlowProperty = DependencyProperty.Register("SharedBlow", typeof(int), typeof(UserControl1), new PropertyMetadata(0));
        public int SharedBlow
        {
            get { return (int)GetValue(SharedBlowProperty); }
            set { SetValue(SharedBlowProperty, value); }
        }
        public static readonly DependencyProperty SharedBlowactiveProperty = DependencyProperty.Register("SharedBlowactive", typeof(bool?), typeof(UserControl1), new PropertyMetadata(false));
        public bool? SharedBlowactive
        {
            get { return (bool?)GetValue(SharedBlowactiveProperty); }
            set { SetValue(SharedBlowactiveProperty, value); }
        }

        public int ip1;
        public int ip2;
        public int ip3;
        public int ip4;
        public int port;

        private fbcommand getparam()
        {
            fbcommand command = new fbcommand();
            if (command_listbox.SelectedIndex == -1) command.seq_n = current_recipe.Commands.Count;
            else command.seq_n = command_listbox.SelectedIndex + 1;
            command.angle = (int)angleSlider.Value;
            command.speed = (int)speedSlider.Value;
            command.acc = (int)accSlider.Value;
            command.dec = (int)decSlider.Value;
            command.movecount = (int)countSlider.Value;
            command.wait_time = (int)waitSlider.Value;
            command.flip_count = (int)flipcountSlider.Value;
            command.flip_time = (int)flipSlider.Value;
            command.blow_active = blow_checkbox.IsChecked == true;
            command.angleaux = (int)angleccwSlider.Value;
            if (blow_checkbox.IsChecked == false && (int)waitSlider.Value == 0)
            {
                command.blow_time = (int)blowSlider.Value;
                command.type = 0;
            }
            else if (blow_checkbox.IsChecked == false && (int)waitSlider.Value > 0)
            {
                command.blow_time = (int)blowSlider.Value;
                command.type = 1;
            }
            else if (blow_checkbox.IsChecked == true && (int)waitSlider.Value == 0)
            {
                command.blow_time = 0;
                command.type = 2;
            }
            else if (blow_checkbox.IsChecked == true && (int)waitSlider.Value > 0)
            {
                command.blow_time = 0;
                command.type = 3;
            }
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
            StartTimerAndRunInParallel();
            /*
            bool? blowtime_active = SharedBlowactive;
            if (blowtime > 0 && (bool)blowtime_active)
            {
                test_button.IsEnabled = false;
                test_button.Opacity = 0.7;
                if (errors.Contains(Sendcmd("RXV" + SharedSpeed * 3))) return;
                if (errors.Contains(Sendcmd("RXA" + SharedAcc * 3))) return;
                if (errors.Contains(Sendcmd("RXB" + SharedDec * 3))) return;
                movecount = SharedCount;
                int flipcount = SharedFlipcount;
                int fliptime = SharedFlip;
                blowtime = SharedBlow;
                int ccwangle = SharedCCWAngle;

                if (errors.Contains(Sendcmd("IL3"))) return;
                if (!(bool)blowtime_active) blow_timer_set();
                while (movecount > 0)
                {
                    if (errors.Contains(Sendcmd("FL" + SharedAngle * 3))) return;
                    if (ccwangle != 0) {
                        if(errors.Contains(Sendcmd("FL" + ccwangle * 3))) return;
                    }
                    movecount--;
                }

                for (int i = 0; i < flipcount; i++)
                {

                    Sendcmd2("IL2");
                    Thread.Sleep(100);

                    Sendcmd2("IH2");
                    if (fliptime > 0) Thread.Sleep(fliptime);
                }
                int a = wait_for_free();
                if(errors.Contains(Sendcmd("IH3"))) return;

                test_button.IsEnabled = true;
                test_button.Opacity = 1;
                }*/
        }
        //_________________________________________________
        /*private void Test_Click(object sender, RoutedEventArgs e)
    {

        if (errors.Contains(Sendcmd("RXV" + SharedSpeed * 3))) return;
        if (errors.Contains(Sendcmd("RXA" + SharedAcc * 3))) return;
        if (errors.Contains(Sendcmd("RXB" + SharedDec * 3))) return;
        movecount = SharedCount;

        if (SharedBlow > 0) Sendcmd("IL3");
        while (movecount > 0)
        {
            if (errors.Contains(Sendcmd("FL" + SharedAngle * 3)));
            if(SharedCCWAngle != 0 && errors.Contains(Sendcmd("FL" + SharedCCWAngle * 3))) return;
            movecount--;
        }

        for (int i = 0; i < SharedFlipcount; i++)
        {

            Sendcmd("IL2");
            Thread.Sleep(100);

            Sendcmd("IH2");
            if(SharedFlip > 0) Thread.Sleep(SharedFlip);
        }
        if (SharedBlow > 0)
        {
            int a = wait_for_free();
            Sendcmd("IH3");
        }

        //_______________________________________________________________
        byte[] MySendByte = { 0, 0, 0 };
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

        if (SharedBlow > 0) blow_timer_set();
        if (SharedFlip > 0)
        {
            max_flip_n = SharedFlip;
            flip_onoff_time = 1000;
            flip_offon_time = SharedFlip;
            flip_timer_set();
        }
    }*/

        int temp_speed;
        int temp_acc;
        int temp_dec;
        int temp_count; 
        int temp_flipcount;
        int temp_blow;
        int temp_ccwangle;
        int temp_angle;
        int temp_fliptime;
        private bool? blow_is_on;
        int blowtime;
        bool recipe_run_done = false;
        private void StartTimerAndRunInParallel()
        {
            blow_is_on = SharedBlowactive;
            temp_speed = SharedSpeed;
            temp_acc = SharedAcc;
            temp_dec = SharedDec;
            temp_count = SharedCount;
            temp_flipcount = SharedFlipcount;
            temp_blow = SharedBlow;
            temp_ccwangle = SharedCCWAngle;
            temp_angle = SharedAngle;
            temp_fliptime = SharedFlip;
            if (blow_is_on == true || temp_blow > 0) Sendcmd2("IL3");
            else blow_off_sent = true;
            // Create a DispatcherTimer instance
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(SharedBlow) 
            };

            // Define the event handler for the timer tick
            timer.Tick += Timer_Tick;

            // Start the timer
            if(blow_is_on == false && temp_blow > 0) timer.Start();

            // Run subsequent code in parallel using Task.Run
            Task.Run(() => RunParallelCode());
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Stop the timer
            (sender as DispatcherTimer).Stop();

            // Perform the action when the timer fires
            while(can_send_blow_off == false)
            {
                Debug.WriteLine("waiting flag");
                Thread.Sleep(100);
            }
            can_send_blow_off = false;
            PerformTimerAction();
        }

        private void PerformTimerAction()
        {
            Sendcmd2("IH3");
            blow_off_sent = true;
        }

        bool can_send_blow_off = false;
        bool blow_off_sent = false;
        private void RunParallelCode()
        {
            //test_button.IsEnabled = false;
            //test_button.Opacity = 0.7;
            //if (errors.Contains(Sendcmd("RXV" + temp_speed * 3))) return;
            //if (errors.Contains(Sendcmd("RXA" + temp_acc * 3))) return;
            //if (errors.Contains(Sendcmd("RXB" + temp_dec * 3))) return;
            Sendcmd2("RXV" + temp_speed * 3);
            Sendcmd2("RXA" + temp_acc * 3);
            Sendcmd2("RXB" + temp_dec * 3);
            movecount = temp_count;
            int flipcount = temp_flipcount;
            int fliptime = temp_fliptime;
            blowtime = temp_blow;
            int ccwangle = temp_ccwangle;

            while (movecount > 0)
            {
                //if (errors.Contains(Sendcmd("FL" + temp_angle * 3))) return;
                Sendcmd2("FL" + temp_angle * 3);
                if (ccwangle != 0)
                {
                    //if (errors.Contains(Sendcmd("FL" + ccwangle * 3))) return;
                    Sendcmd2("FL" + ccwangle * 3);
                }
                movecount--;
                if (movecount == 0) can_send_blow_off = true;
            }

            for (int i = 0; i < flipcount; i++)
            {

                Sendcmd2("IL2");
                Thread.Sleep(100);

                Sendcmd2("IH2");
                if (fliptime > 0) Thread.Sleep(fliptime);
            }
            int a;
            if (blow_is_on == true)
            {
                a = wait_for_free();
                Sendcmd2("IH3");
                blow_off_sent = true;
            }

            byte[] responseBuffer = new byte[20];
            using (Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString());
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                sender.Connect(remoteEP);



                // Receive the response from the server
                int bytesRead = 0;
                string response = "";
                int i = 0;
                while (response != "SC=0001")
                {
                    try
                    {
                        sender.Send(ConstructMessageBytes("SC"));
                        bytesRead = sender.Receive(responseBuffer);
                        response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                        if (response == "SC=0201")
                        {
                            Sendcmd2("IH3");
                            alarm_rectangle.Visibility = Visibility.Visible;
                        }
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show("response error");
                    }
                    //Thread.Sleep(100);
                }
                recipe_run_done = true;
            }
            //test_button.IsEnabled = true;
            //test_button.Opacity = 1;
        }


        private System.Timers.Timer blow_timer;
        public void blow_timer_set()
        {
            Task.Run(() =>
            {
                bool isOn = false;
                for (int i = 0; i < 2; i++)
                {
                    // Toggle the state
                    isOn = !isOn;

                    // Call the function with the current state
                    if (isOn) Dispatcher.Invoke(() => Sendcmd2("IL3"));
                    else Dispatcher.Invoke(() => Sendcmd2("IH3"));
                    Debug.WriteLine("blow");

                    // Wait for the appropriate interval
                    Thread.Sleep(blowtime); // 1 second for on-off, 2 seconds for off-on
                }
            });


            /*
            // Initialize the timer
            blow_timer = new System.Timers.Timer(SharedBlow); // 2000 milliseconds = 2 seconds

            // Define what happens when the timer elapses
            blow_timer.Elapsed += (sender, e) =>
            {
                // This will run on a separate thread, so we need to marshal the call back to the UI thread
                Dispatcher.Invoke(() =>
                {
                    // Toggle the state
                    blow_is_on = !blow_is_on;

                    // Call the function with the current state

                    // Stop the timer if we just turned off
                    if (!blow_is_on)
                    {

                        Debug.WriteLine("BLOW OFF");
                        Sendcmd2("IH2");
                        blow_timer.Stop();
                    }
                });
            };

            // Turn on the thing initially
            blow_is_on = true;

            Debug.WriteLine("BLOW ON");
            Sendcmd2("IL2");

            // Start the timer
            blow_timer.Start();*/
        }
        private System.Timers.Timer flip_timer;
        private bool flip_is_on;
        private int flip_count;
        private int max_flip_n;
        private int flip_onoff_time;
        private int flip_offon_time;
        public void flip_timer_set()
        {
            // Initialize the timer
            flip_timer = new System.Timers.Timer();
            flip_timer.Interval = flip_offon_time; // Initial interval to turn on

            // Define what happens when the timer elapses
            flip_timer.Elapsed += (sender, e) =>
            {
                // This will run on a separate thread, so we need to marshal the call back to the UI thread
                Dispatcher.Invoke(() =>
                {
                    // Toggle the state
                    flip_is_on = !flip_is_on;


                    // Adjust the timer interval based on the current state
                    if (flip_is_on)
                    {
                        Sendcmd("IL2");
                        flip_timer.Interval = flip_onoff_time; // 1 second interval between on-off
                    }
                    else
                    {
                        Sendcmd("IH2");
                        flip_timer.Interval = flip_offon_time; // 2 seconds interval between off-on
                    }

                    // Increment the counter
                    flip_count++;

                    // Stop the timer after the specified number of cycles
                    if (flip_count >= max_flip_n * 2) // *2 because we toggle each cycle
                    {
                        flip_timer.Stop();
                    }
                });
            };

            // Initialize counter and state
            flip_count = 0;
            flip_is_on = false;

            // Start the timer
            flip_timer.Start();
        }

        public int wait_for_free()
        {
            byte[] responseBuffer = new byte[20];
            using (Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString());
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                sender.Connect(remoteEP);



                // Receive the response from the server
                int bytesRead = 0;
                string response = "";
                int i = 0;
                while (response != "SC=0001")
                {
                    try
                    {
                        sender.Send(ConstructMessageBytes("SC"));
                        bytesRead = sender.Receive(responseBuffer);
                        response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                        if(response == "SC=0201")
                        {
                            Sendcmd2("IH3");
                            alarm_rectangle.Visibility = Visibility.Visible;
                            return -1;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("response error");
                        return -1;
                    }
                    //Thread.Sleep(100);
                }
                return 1;
            }
        }
        public int Sendcmd(string param)
        {
            byte[] paramBytes = ConstructMessageBytes(param);
            byte[] responseBuffer = new byte[20];

            using (Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString()+"."+ip2.ToString()+"."+ip3.ToString()+"."+ip4.ToString());
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
        public void Sendcmd2(string param)
        {
            byte[] paramBytes = ConstructMessageBytes(param);
            byte[] responseBuffer = new byte[20];

            using (Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString());
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                sender.Connect(remoteEP);


                // Send the data through the socket
                int bytesSent = sender.Send(paramBytes);
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
        static string ExtractMessage(byte[] packet)
        {
            // Validate the packet format
            if (packet.Length < 3 || packet[0] != 0 || packet[1] != 7 || packet[packet.Length - 1] != 13)
            {
                throw new ArgumentException("Invalid packet format");
            }

            // Extract the message bytes
            byte[] messageBytes = new byte[packet.Length - 3];
            Array.Copy(packet, 2, messageBytes, 0, messageBytes.Length);

            // Convert the message bytes to a string
            string message = Encoding.ASCII.GetString(messageBytes);
            return message;
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
            string ipaddress = ip1.ToString()+"."+ ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString();
            string[] cmd = { "\"RXV" + current_recipe.Commands[0].speed * 3 + "\"", "\"RXA" + current_recipe.Commands[0].acc * 3 + "\"", "\"RXB" + current_recipe.Commands[0].dec * 3 + "\"", "\"FL" + current_recipe.Commands[0].angle * 3 + "\"" };
            _TMscript = "Socket ntd_fb = \"" + ipaddress + "\", " + port + "\n" +
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
            if (e.AddedItems.Count > 0 && recipe_isrunning == false)
            {
                selected_command = (fbcommand)e.AddedItems[0];
                Load_click();
            }
            if(e.AddedItems.Count > 0 && recipe_isrunning == true)
            {
                selected_command = (fbcommand)e.AddedItems[0];
                Debug.WriteLine(selected_command.Value.seq_n.ToString());

                StartTimerAndRunInParallel();
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
            SharedBlowactive = selected_command.Value.blow_active;
            SharedBlow = selected_command.Value.blow_time;
            if(selected_command.Value.blow_active == true)
            {
                blowSlider.IsEnabled = false;
                blowSlider.Opacity = 0.7;
                blow_textbox.IsEnabled = false;
                blow_textbox.Opacity = 0.7;
            }
            if (selected_command.Value.blow_active == false)
            {
                blowSlider.IsEnabled = true;
                blowSlider.Opacity = 1;
                blow_textbox.IsEnabled = true;
                blow_textbox.Opacity = 1;
            }
        }
        bool flag_AddRecipeWindow_RecipeDeleted = false;
        private void Delete_click(object sender, RoutedEventArgs e)
        {
            if (current_recipe != null)
            {
                if (!flag_AddRecipeWindow_RecipeDeleted)
                {
                    save_Prompt.RecipeDeleted += AddRecipeWindow_RecipeDeleted;
                    flag_AddRecipeWindow_RecipeDeleted = true;
                }
                save_Prompt.Show();
                save_Prompt.delete(current_recipe);

                command_listbox.Items.Refresh();
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
            if(command_listbox.SelectedItem != null)
            {
                int temp = command_listbox.SelectedIndex;
                add_click(sender, e);
                remove_click(sender, e);
                command_listbox.SelectedIndex = temp;
            }
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
        bool flag_AddRecipeWindow_RecipeAdded = false;
        public void new_recipe_click(object sender, RoutedEventArgs e)
        {
            if (!flag_AddRecipeWindow_RecipeAdded)
            {
                save_Prompt.RecipeAdded += AddRecipeWindow_RecipeAdded;
                flag_AddRecipeWindow_RecipeAdded = true;
            }
            save_Prompt.Show();
            save_Prompt.reset();
        }

        private void add_click(object sender, RoutedEventArgs e)
        {
            if (current_recipe != null)
            {
                if (current_recipe.Commands.Count < 13)
                {
                    if (command_listbox.SelectedIndex == -1) current_recipe.Commands.Insert(current_recipe.Commands.Count, getparam());
                    else
                    {
                        var temp = new fbcommand();
                        for (int i = command_listbox.SelectedIndex + 1; i < current_recipe.Commands.Count; i++)
                        {
                            temp = current_recipe.Commands[i];
                            temp.seq_n++;
                            current_recipe.Commands[i] = temp;
                        }
                        current_recipe.Commands.Insert(command_listbox.SelectedIndex + 1, getparam());
                    }


                    if (RecipeManager.UpdateRecipe(current_recipe))
                    {
                        RecipeManager.LoadReg();
                        update_listbox();

                        command_listbox.Items.Refresh();
                    }
                }
                else MessageBox.Show("commands limit reached");
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
        public void UpdateCombobox2()
        {
            recipe_combobox.ItemsSource = null;
            recipe_combobox.ItemsSource = recipe_reg;
            command_listbox.Items.Refresh();
        }
        public void UpdateCombobox3()
        {
            recipe_combobox.ItemsSource = null;
            recipe_combobox.ItemsSource = recipe_reg;
            recipe_combobox.SelectedIndex = recipe_combobox.Items.Count;
            command_listbox.Items.Refresh();
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
            //else MessageBox.Show("update listbox error");
        }

        private void remove_click(object sender, RoutedEventArgs e)
        {
            if(current_recipe.Commands.Count > 0)
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
            port_textbox.IsEnabled = true;
            port_textbox.Opacity = 1;
        }

        private void save_ip_click(object sender, RoutedEventArgs e)
        {
            ip1 = int.Parse(ip1_textbox.Text);
            ip2 = int.Parse(ip2_textbox.Text);
            ip3 = int.Parse(ip3_textbox.Text);
            ip4 = int.Parse(ip4_textbox.Text);
            port = int.Parse(port_textbox.Text);
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ip_address.txt");
            string filePath2 = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "port.txt");
            File.WriteAllText(filePath, ip1_textbox.Text + "." + ip2_textbox.Text + "." + ip3_textbox.Text + "." + ip4_textbox.Text);
            File.WriteAllText(filePath2, port_textbox.Text);
            ip1_textbox.IsEnabled = false;
            ip1_textbox.Opacity = 0.7;
            ip2_textbox.IsEnabled = false;
            ip2_textbox.Opacity = 0.7;
            ip3_textbox.IsEnabled = false;
            ip3_textbox.Opacity = 0.7;
            ip4_textbox.IsEnabled = false;
            ip4_textbox.Opacity = 0.7;
            port_textbox.IsEnabled = false;
            port_textbox.Opacity = 0.7;
        }

        bool recipe_isrunning = false;
        int recipe_run_index;
        private async void test_recipe_click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < current_recipe.Commands.Count; i++)
            {

                selected_command = current_recipe.Commands[i];
                Load_click();
                StartTimerAndRunInParallel();
                while (recipe_run_done == false || blow_off_sent == false)
                {
                    await Task.Delay(200);
                }
                blow_off_sent = false;
                recipe_run_done = false;
            }

            /*byte[] responseBuffer = new byte[20];

            using (Socket sender2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString());
                int port = 7776;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                sender2.Connect(remoteEP);
                int bytesRead = 0;
                string response = "";

                for(int i = 0; i < current_recipe.Commands.Count; i++)
                {
                    Debug.WriteLine("running command: " + i.ToString());
                    blow_off_sent = false;
                    selected_command = current_recipe.Commands[i];
                    Load_click();
                    StartTimerAndRunInParallel();
                    await Task.Delay(1000);
                    for(int j=0; j<20; j++)
                    {

                        await Task.Delay(100);
                        sender2.Send(ConstructMessageBytes("SC"));
                        bytesRead = sender2.Receive(responseBuffer);
                        response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                        Debug.WriteLine(response + "____");
                    }
                    while (response != "SC=0001")
                    {
                        await Task.Delay(200);
                        sender2.Send(ConstructMessageBytes("SC"));
                        bytesRead = sender2.Receive(responseBuffer);
                        response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                        Debug.WriteLine(response+ " ("+i.ToString()+")");
                    }
                    while (blow_off_sent == false)
                    {
                        await Task.Delay(200);
                    }

                }
                    
            }*/


        }

        private void blow_checked(object sender, RoutedEventArgs e)
        {
            blowSlider.IsEnabled = false;
            blowSlider.Opacity = 0.7;
            blow_textbox.IsEnabled = false;
            blow_textbox.Opacity = 0.7;
            current_command.blow_active = true;
        }
        private void blow_unchecked(object sender, RoutedEventArgs e)
        {
            blowSlider.IsEnabled = true;
            blowSlider.Opacity = 1;
            blow_textbox.IsEnabled = true;
            blow_textbox.Opacity = 1;
            current_command.blow_active = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Sendcmd2("IH3");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            MessageBox.Show( command_listbox.SelectedIndex.ToString());

            /*using (Socket sender3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString());
                int port = 7776;
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                sender3.Connect(remoteEP);
                sender3.Send(ConstructMessageBytes("51583132"));
            }*/
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

            byte[] responseBuffer = new byte[20];
            using (Socket sender3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress ipAddress = IPAddress.Parse(ip1.ToString() + "." + ip2.ToString() + "." + ip3.ToString() + "." + ip4.ToString());
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                sender3.Connect(remoteEP);



                // Receive the response from the server
                int bytesRead = 0;
                string response = "";
                sender3.Send(ConstructMessageBytes("SC"));
                bytesRead = sender3.Receive(responseBuffer);
                response = Encoding.ASCII.GetString(responseBuffer, 2, bytesRead - 3);
                Debug.WriteLine(response);
            }
        }
        private void AddRecipeWindow_RecipeAdded(object sender, RecipeAddedEventArgs e)
        {
            // Add the new recipe to the list
            recipe_reg.Add(e.NewRecipe);

            // Update the ComboBox to refresh its items
            UpdateCombobox3();
        }
        private void AddRecipeWindow_RecipeDeleted(object sender, RecipeDeletedEventArgs e)
        {
            recipe_combobox.SelectedItem = null;
            UpdateCombobox2();
        }
        public void param_lost_focus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox == null) return;

            fbcommand? command = textbox.DataContext as fbcommand?;
            if (command == null) return;

            string propertyName = textbox.Name;
            int newValue;

            if (int.TryParse(textbox.Text, out newValue))
            {
                var property = typeof(fbcommand).GetProperty(propertyName);
                if (property != null && property.CanWrite && property.PropertyType == typeof(int))
                {
                    property.SetValue(command, newValue);

                    // Assuming current_recipe is accessible and contains the updated command
                    current_recipe.Commands[command.Value.seq_n] = (fbcommand)command;
                }
                RecipeManager.UpdateRecipe(current_recipe);
            }
            else
            {
                // Handle invalid integer input if necessary
                MessageBox.Show("Invalid input. Please enter a valid integer.");
            }
        }
        public void blow_active_click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("wdccdcasqrev");
            CheckBox checkbox = sender as CheckBox;
            fbcommand? command = checkbox.DataContext as fbcommand?;
            if (command == null) return;
            string propertyName = checkbox.Name;
            var property = typeof(fbcommand).GetProperty(propertyName);
            if ((bool)checkbox.IsChecked) property.SetValue(command, true);
            else property.SetValue(command, false);
            current_recipe.Commands[command.Value.seq_n] = (fbcommand)command;
            RecipeManager.UpdateRecipe(current_recipe);

        }
    }

}
