using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Device.Location;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Timers;

// Graph
using ScottPlot;
using ScottPlot.Styles;
using System.Security.Permissions;

//Map
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.ObjectModel;
using GMap.NET.WindowsForms.ToolTips;


//using Windows.UI.Xaml.Controls;
using GMap.NET.WindowsPresentation;
using System.Windows.Media.Media3D;
using System.Text.RegularExpressions;
using ScottPlot.Drawing.Colormaps;
using Microsoft.Win32;
using ScottPlot.Plottable;
using System.Reflection;
using System.Data.Entity.Infrastructure;


//3D
using HelixToolkit.Wpf;


namespace GCS_EEPISAT_05__COSMO_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : System.Windows.Window
    {
        readonly SerialPort _serialPort = new();
        enum Sequencer { readSensor };
        string dataSensor;

        public delegate void uiupdater();

        int dataSum = 0;
        bool isAscii = false;
        string[] splitData;

        public delegate void AddDataDelegate(String myString);

        public AddDataDelegate WriteTelemetryData;

        StreamWriter writeCan;
        string cansatFileLog = "";


        // bin App path
        readonly string binAppPath = System.AppDomain.CurrentDomain.BaseDirectory;


        public static GMap.NET.WindowsForms.GMapMarker? mapMarkerPayload;
        public static GMap.NET.WindowsForms.GMapMarker? mapMarkerGCS;
        public static GMapOverlay? mapOverlay;

        // Variabel CanSat Log
        uint teamId = 2032;
        string state;
        string missionTime;
        //string missionTimeOld = null;

        uint packetCount;
        uint max_packetCount;
        uint max_max_packetCount;
        char mode; // F & S

        float altitude;
        float last_altitude = 0.0f;
        float max_altitude;
        float max_max_altitude;
        float speed;
        float air_speed;
        //float max_air_speed;
        float max_max_air_speed;
        char hs_status;
        char pc_status;
        float temperature;
        float max_temperature;
        float max_max_temperature;
        float pressure;
        float min_pressure;
        float min_min_pressure = 0.0f;
        float voltage;
        float max_volt;
        float max_max_volt;
        string gps_time;
        float gps_altitude;
        float gps_latitude;
        float gps_longitude;
        uint gps_sats_count;
        float tilt_x;
        float tilt_y;
        float rot_z;
        float heading;
        string cmd_echo;
        bool checkSumHasil;
        int validCount = 0;
        int corruptCount = 0;
        float[] tilt_x_gen = new float[24];
        float[] tilt_y_gen = new float[24];

        bool hs_deployed = false;
        bool pc_deployed = false;
        bool auto_scroll = false;
        bool minimap_check = false;
        bool openHeatShieldSimulation = false;
        int countOpenHeatShield = 0;
        string statusRegion = "Surabaya";
        float gcs_latitude = 0.0f;
        float gcs_longitude = 0.0f;

        int totalCanSatData = 0;

        Microsoft.Win32.OpenFileDialog openFileDialog;

        // battery
        static int jumlahDataRegLin;
        static float sumX, sumY, sumX2, sumXY;
        static float x, y, m, c;
        private float hasilRegLin;
        readonly int lineCommand = 0;

        // 3D
        Vector3D axis;
        const int angle = 50;

        // graph
        bool graphOnline = false;


        // Variabel CSV Simulation Mode
        readonly List<String> Col4 = new();

        int timerCSV = 0;

        private readonly object _lockObj = new object();
        private System.Threading.Timer _timerCommand;
        private int _counterCommand = 0;
        private bool _isTimerRunning = false;


        readonly System.Windows.Threading.DispatcherTimer timerSimulation = new();
        readonly System.Windows.Threading.DispatcherTimer timergraph = new();
        readonly System.Windows.Threading.DispatcherTimer timerGen3d = new();

        string fileobj;
        string fileobj2;
        string fileobj3;


        Model3DGroup modelMain;
        Model3DGroup model1;
        Model3DGroup model2;
        Model3DGroup model3;


        readonly private GeoCoordinateWatcher watcher;
        double distance = 0;


        public delegate void MethodInvoker();

        // Serial COM PORT
        ManagementEventWatcher detector;
        string SerialPortNumber = "";
        string SerialPortName = "";
        string SerialPortBaudrate = "";


        readonly System.Windows.Threading.DispatcherTimer timer = new();

        //readonly double[] GPSSats = new double[100_000];
        //readonly double[] Long = new double[100_000];
        //readonly double[] Lat = new double[100_000];
        //readonly double[] TiltX = new double[100_000];
        //readonly double[] TiltY = new double[100_000];
        //readonly double[] Pressure = new double[100_000];
        //readonly double[] Temperature = new double[100_000];
        //readonly double[] Voltage = new double[100_000];
        //readonly double[] CanSatAlt = new double[100_000];
        //readonly double[] GPSAlt = new double[100_000];
        //readonly double[] AirSpeed = new double[100_000];

        readonly double[] GPSSats = { 5, 6, 5, 12, 2 };
        readonly double[] Long = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] Lat = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] TiltX = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] TiltY = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] Pressure = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] Temperature = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] Voltage = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] CanSatAlt = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] GPSAlt = { 10.0, 10.2, 8.0, 0.7 };
        readonly double[] AirSpeed = { 10.0, 10.2, 8.0, 0.7 };

        readonly ScottPlot.Plottable.SignalPlot SignalPlot;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot2;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot3;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot4;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot5;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot6;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot7;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot8;
        readonly ScottPlot.Plottable.SignalPlot SignalPlot9;
        int NextPointIndex = 0;

        SolidColorBrush kuning = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFC12F");
        SolidColorBrush merah = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF6157");
        SolidColorBrush merahdark = (SolidColorBrush)new BrushConverter().ConvertFromString("#B22222");


        public MainWindow()
        {

            InitializeComponent();
            Loaded += WindowLoaded;


            //SoundPlayer player = new(binAppPath + "/Audio/GCSSTART.wav");
            //player.Play();

            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            watcher.Start();


            _serialPort.DataReceived += new SerialDataReceivedEventHandler(Serialport_Datareceive);


            timerSimulation.Interval = TimeSpan.FromMilliseconds(1000);
            timerSimulation.Tick += TimerSimulation_Tick;

            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

            if (!Directory.Exists(binAppPath + "\\LogData\\"))
            {
                Directory.CreateDirectory(binAppPath + "\\LogData\\");
            }
            if (!Directory.Exists(binAppPath + "\\LogData\\FLIGHT"))
            {
                Directory.CreateDirectory(binAppPath + "\\LogData\\FLIGHT");
            }
            if (!Directory.Exists(binAppPath + "\\LogData\\SIMULATION"))
            {
                Directory.CreateDirectory(binAppPath + "\\LogData\\SIMULATION");
            }

            // 3D

            fileobj = System.AppDomain.CurrentDomain.BaseDirectory + "/Assets/3D/CanSat Inside Rocket/Assembly_3D GCS.obj";
            fileobj2 = System.AppDomain.CurrentDomain.BaseDirectory + "/Assets/3D/CanSat Parachute Deploy/Assembly_3D GCS.obj";
            fileobj3 = System.AppDomain.CurrentDomain.BaseDirectory + "/Assets/3D/CanSat Separated/Assembly_3D GCS.obj";
            ModelImporter import = new();
            this.Dispatcher.Invoke(() =>
            {
                model1 = import.Load(fileobj);
                model2 = import.Load(fileobj2);
                model3 = import.Load(fileobj3);
            });

            PressurePlot.Refresh();
            AltitudePlot.Refresh();
            VoltagePlot.Refresh();
            TiltPlot.Refresh();
            TemperaturePlot.Refresh();
            SatellitCountPlot.Refresh();
            AirSpeedPlot.Refresh();
            LatLongPlot.Refresh();


            // setup graph

            PressurePlot.Plot.Style(ScottPlot.Style.Light1);
            AltitudePlot.Plot.Style(ScottPlot.Style.Light1);
            VoltagePlot.Plot.Style(ScottPlot.Style.Light1);
            TiltPlot.Plot.Style(ScottPlot.Style.Light1);
            TemperaturePlot.Plot.Style(ScottPlot.Style.Light1);
            SatellitCountPlot.Plot.Style(ScottPlot.Style.Light1);
            AirSpeedPlot.Plot.Style(ScottPlot.Style.Light1);
            LatLongPlot.Plot.Style(ScottPlot.Style.Light1);


            SignalPlot = SatellitCountPlot.Plot.AddSignal(GPSSats, color: System.Drawing.Color.FromArgb(255, 222, 158, 93), label: "GPS Sats");
            SatellitCountPlot.Plot.Legend();
            SatellitCountPlot.Plot.SetAxisLimits(0, 5, -13, 30);

            SignalPlot2 = PressurePlot.Plot.AddSignal(Pressure, color: System.Drawing.Color.FromArgb(255, 222, 158, 93), label: "Pressure");
            PressurePlot.Plot.Legend();
            PressurePlot.Plot.SetAxisLimits(0, 5, -15, 130);

            SignalPlot3 = TemperaturePlot.Plot.AddSignal(Temperature, color: System.Drawing.Color.FromArgb(255, 222, 158, 93), label: "Temperature");
            TemperaturePlot.Plot.Legend();
            TemperaturePlot.Plot.SetAxisLimits(0, 5, -30, 90);

            SignalPlot4 = VoltagePlot.Plot.AddSignal(Voltage, color: System.Drawing.Color.FromArgb(255, 222, 158, 93), label: "Voltage");
            VoltagePlot.Plot.Legend();
            VoltagePlot.Plot.SetAxisLimits(0, 5, -10, 10);

            SignalPlot5 = TiltPlot.Plot.AddSignal(TiltX, label: "Tilt X");
            SignalPlot7 = TiltPlot.Plot.AddSignal(TiltY, label: "Tilt Y");
            TiltPlot.Plot.Legend();
            TiltPlot.Plot.SetAxisLimits(0, 5, -200, 200);

            LatLongPlot.Plot.AddScatter(Lat, Long, color: System.Drawing.Color.FromArgb(255, 222, 158, 93), markerSize: 0, label: "Lat & Long");
            //LatLongPlot.Plot.AddSignal(Lat, label: "Latitude");
            //LatLongPlot.Plot.AddSignal(Long, label: "Longitude");
            LatLongPlot.Plot.Legend();
            LatLongPlot.Plot.SetAxisLimits(-185, 185, -95, 95);

            SignalPlot6 = AltitudePlot.Plot.AddSignal(CanSatAlt, color: System.Drawing.Color.FromArgb(255, 222, 158, 93), label: "CanSat");
            SignalPlot8 = AltitudePlot.Plot.AddSignal(GPSAlt, color: System.Drawing.Color.FromArgb(255, 106, 128, 184), label: "GPS");
            AltitudePlot.Plot.Legend();
            AltitudePlot.Plot.SetAxisLimits(0, 5, -300, 1500);

            SignalPlot9 = AirSpeedPlot.Plot.AddSignal(Pressure, color: System.Drawing.Color.FromArgb(255, 222, 158, 93), label: "Air Speed");
            AirSpeedPlot.Plot.Legend();
            AirSpeedPlot.Plot.SetAxisLimits(0, 5, -15, 50);

            timergraph.Interval = new TimeSpan(0, 0, 1);
            timergraph.Tick += new EventHandler(TimerGraph_Tick);


            // dummy graph
            double currentRightEdge = SatellitCountPlot.Plot.GetAxisLimits().XMax;

            if (6 > currentRightEdge)
            {
                SatellitCountPlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                PressurePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                VoltagePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                TemperaturePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                TiltPlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                AltitudePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                AirSpeedPlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                if (6 != 0 && 5 % 10 == 0)
                {
                    SatellitCountPlot.Plot.SetAxisLimits(xMin: 6 - 10);
                    PressurePlot.Plot.SetAxisLimits(xMin: 6 - 10);
                    VoltagePlot.Plot.SetAxisLimits(xMin: 6 - 10);
                    TemperaturePlot.Plot.SetAxisLimits(xMin: 6 - 10);
                    TiltPlot.Plot.SetAxisLimits(xMin: 6 - 10);
                    AltitudePlot.Plot.SetAxisLimits(xMin: 6 - 10);
                }
            }

            SatellitCountPlot.Render();
            PressurePlot.Render();
            VoltagePlot.Render();
            TemperaturePlot.Render();
            TiltPlot.Render();
            AltitudePlot.Render();
            LatLongPlot.Render();


            // live time update
            System.Windows.Threading.DispatcherTimer LiveTime = new System.Windows.Threading.DispatcherTimer();
            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += timer_Tick;
            LiveTime.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            GCSTimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void TimerGraph_Tick(object sender, EventArgs e)
        {
            //if (missionTimeOld != null && missionTimeOld == missionTime)
            //{
            // timergraph.Stop();
            //  missionTimeOld = null;
            //}

            //TimerSatOne_Tick(sender, e);
            //TimerSatTwo_Tick(sender, e);
        }

        private void TimerSatOne_Tick(object sender, EventArgs e)
        {
            if (graphOnline)
            {
                GPSSats[NextPointIndex] = gps_sats_count;
                CanSatAlt[NextPointIndex] = altitude;
                GPSAlt[NextPointIndex] = gps_altitude;
                Pressure[NextPointIndex] = pressure;
                Temperature[NextPointIndex] = temperature;
                Voltage[NextPointIndex] = voltage;
                Lat[NextPointIndex] = gps_latitude;
                Long[NextPointIndex] = gps_longitude;
                TiltX[NextPointIndex] = tilt_x;
                TiltY[NextPointIndex] = tilt_y;
            }
            else
            {
                Pressure[NextPointIndex] = 0;
                CanSatAlt[NextPointIndex] = 0;
                GPSAlt[NextPointIndex] = 0;
                Voltage[NextPointIndex] = 0;
                TiltX[NextPointIndex] = 0;
                TiltY[NextPointIndex] = 0;
                Temperature[NextPointIndex] = 0;
                GPSSats[NextPointIndex] = 0;
                AirSpeed[NextPointIndex] = 0;
                Lat[NextPointIndex] = 0;
                Long[NextPointIndex] = 0;

            }

            SignalPlot.MaxRenderIndex = NextPointIndex;
            SignalPlot2.MaxRenderIndex = NextPointIndex;
            SignalPlot3.MaxRenderIndex = NextPointIndex;
            SignalPlot4.MaxRenderIndex = NextPointIndex;
            SignalPlot5.MaxRenderIndex = NextPointIndex;
            SignalPlot6.MaxRenderIndex = NextPointIndex;
            SignalPlot7.MaxRenderIndex = NextPointIndex;
            SignalPlot8.MaxRenderIndex = NextPointIndex;
            NextPointIndex += 1;
        }

        private void TimerSatTwo_Tick(object sender, EventArgs e)
        {
            // adjust the axis limits only when needed
            double currentRightEdge = SatellitCountPlot.Plot.GetAxisLimits().XMax;

            if (NextPointIndex > currentRightEdge)
            {
                SatellitCountPlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                PressurePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                VoltagePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                TemperaturePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                TiltPlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                AltitudePlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                AirSpeedPlot.Plot.SetAxisLimits(xMax: currentRightEdge + 1);
                if (NextPointIndex != 0 && NextPointIndex % 10 == 0)
                {
                    SatellitCountPlot.Plot.SetAxisLimits(xMin: NextPointIndex - 10);
                    PressurePlot.Plot.SetAxisLimits(xMin: NextPointIndex - 10);
                    VoltagePlot.Plot.SetAxisLimits(xMin: NextPointIndex - 10);
                    TemperaturePlot.Plot.SetAxisLimits(xMin: NextPointIndex - 10);
                    TiltPlot.Plot.SetAxisLimits(xMin: NextPointIndex - 10);
                    AltitudePlot.Plot.SetAxisLimits(xMin: NextPointIndex - 10);
                }
            }

            SatellitCountPlot.Render();
            PressurePlot.Render();
            VoltagePlot.Render();
            TemperaturePlot.Render();
            TiltPlot.Render();
            AltitudePlot.Render();
            LatLongPlot.Render();
            //missionTimeOld = missionTime;
            if (state == "LANDED" && teamId != 1000)
            {
                timergraph.Stop();
                //missionTimeOld = null;
            }
        }


        private void HomeNavClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MainPage.SelectedIndex = 0;
        }

        private void GraphNavClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MainPage.SelectedIndex = 1;
        }

        private void MapNavClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MainPage.SelectedIndex = 2;
        }

        public void USBChangedEvent(object sender, EventArrivedEventArgs e)
        {
            ((ManagementEventWatcher)sender).Stop();

            Dispatcher.Invoke((MethodInvoker)delegate
            {
                ManagementObjectSearcher deviceList = new("Select Name, Description, DeviceID from Win32_SerialPort");

                // List to store available USB serial devices plugged in 
                List<String> CompPortList = new();

                ComportDropdown.Items.Clear();
                // Any results? There should be!
                if (deviceList != null)
                {
                    // Enumerate the devices
                    foreach (ManagementObject device in deviceList.Get().Cast<ManagementObject>())
                    {
                        SerialPortNumber = device["DeviceID"].ToString();
                        string serialName = device["Name"].ToString();
                        string SerialDescription = device["Description"].ToString();
                        CompPortList.Add(SerialPortNumber);
                        ComportDropdown.Items.Add(SerialPortNumber);
                    }
                }
                else
                {
                    ComportDropdown.Items.Add("NO SerialPorts AVAILABLE!");
                }
            });
            ((ManagementEventWatcher)sender).Start();
        }

        public void Serialport_Datareceive(object sender, SerialDataReceivedEventArgs e)
        {
            //if (stepData == Sequencer.readSensor)
            //{
            try
            {
                this.dataSensor = _serialPort.ReadLine();
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new uiupdater(VerifyData));
                // Debug.WriteLine("Test " + _serialPort.ReadExisting());
            }

            catch (NullReferenceException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine("Error - " + ex.Message);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //}
        }

        public async void VerifyData()
        {
            await SerialControlTextBox.Dispatcher.BeginInvoke(this.WriteTelemetryData, new Object[]
            {
                string.Concat(" \0" +dataSensor, "\t------------#END OF PACKET DATA------------\n")
            });
            // Debug.WriteLine("{0}", dataSensor);
            SerialControlTextBox.SelectionLength = SerialControlTextBox.Text.Length;
            SerialControlTextBox.ScrollToEnd();
            CMDTextBox1.IsReadOnly = false;
            //CMDTextBox1.Focus();
            CMDTextBox2.SelectAll();
            CMDTextBox2.SelectionLength = CMDTextBox2.Text.Length;
            CMDTextBox2.ScrollToEnd();
            dataSum++;
            Debug.WriteLine("Masuk Cak 1");

            if (dataSum == 50)
            {
                SerialControlTextBox.Clear();
                dataSum = 0;
            }
            isAscii = Regex.IsMatch(dataSensor, @"[^\u0021-\u007E]", RegexOptions.None);
            Debug.WriteLine("Masuk Cak 2 " + isAscii + dataSensor);

            if (isAscii)
            {
                Debug.WriteLine("Masuk Cak 3");

                try
                {
                    splitData = dataSensor.Split((char)44); // (char)44 = ','
                    Debug.WriteLine(splitData.Length);
                    //System.Diagnostics.Debug.WriteLine("VerifyData : Receiver Checksum " + recCheckSum);
                    //Pengecekan 
                    //  Panjang Data               Team ID                  Team ID Testing                                         Team ID                                                                         Mission Time                Packet Count         
                    if ((splitData.Length == 20 || splitData.Length == 21 || splitData.Length == 22 || splitData.Length == 23 || splitData.Length == 24) && (splitData[0] == "2032" || splitData[0] == "1000") && splitData[0].Length == 4 && splitData[1].Length == 8 && splitData[2].Length <= 4)
                    {
                        Debug.WriteLine("Masuk Cak 4");

                        try
                        {
                            CheckTelemetryData();
                            // CanDataLog();
                            // WriteLogPayload();
                            GC.Collect();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    //Cek cmdEcho
                    else if (splitData.Length == 1 && splitData[0].Length <= 7)

                    {
                        try
                        {
                            CheckTelemetryData();
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        public static byte CheckHasil(char[] data_, byte checksum, byte length)
        {
            ushort buff = 0;
            byte hasil, buffhasil;
            for (int i = 0; i < 175; i++)
            {
                buff += data_[i];
                if (data_[i] == '\0' || i > length - 2) break;
            }
            buff += checksum;
            hasil = (byte)buff;
            buffhasil = (byte)(buff >> 8);
            hasil += buffhasil;
            return hasil;
        }

        private void CheckTelemetryData()
        {
            Debug.WriteLine("Masuk Cak 5");
            try
            {
                if (splitData.Length == 20 || splitData.Length == 21 || splitData.Length == 22 || splitData.Length == 23 || splitData.Length == 24)
                {
                    Debug.WriteLine("Masuk Cak 6");

                    // checksum avail or not 
                    if (splitData.Length == 23 || splitData.Length == 24)
                    {
                        System.Diagnostics.Debug.WriteLine("CheckSum - CanSat " + splitData[23]);

                        totalCanSatData++;
                        int checkSum = Int32.Parse(splitData[23]);
                        // Find the last comma position
                        int lastCommaPosition = dataSensor.LastIndexOf(',');

                        // Trim the string after the last comma
                        string result = dataSensor.Substring(0, lastCommaPosition + 1);
                        System.Diagnostics.Debug.WriteLine("Split Data - GCS " + result);

                        string checkString = result;
                        char[] dataSensorChar = checkString.ToCharArray();
                        byte hasil = CheckHasil(dataSensorChar, (byte)checkSum, (byte)dataSensorChar.Length);
                        System.Diagnostics.Debug.WriteLine("CheckSum - GCS " + hasil);

                        if (hasil == 255)
                        {
                            validCount++;
                            checkSumHasil = true;
                        }
                        else
                        {
                            corruptCount++;
                            checkSumHasil = false;
                        }
                    }

                    Debug.WriteLine("Masuk Cak 7");

                    if (splitData[0].Length > 0 && splitData[0].All(c => Char.IsNumber(c)))
                    {
                        teamId = Convert.ToUInt32(splitData[0]);
                    }
                    else
                    {
                        teamId = 0;
                    }


                    if (splitData[2].Length > 0 && splitData[2].All(c => Char.IsNumber(c)))
                    {
                        packetCount = Convert.ToUInt32(splitData[2]);
                    }
                    else
                    {
                        packetCount = 0;
                    }

                    if (splitData[5].Length > 0 && splitData[5].All(c => Char.IsNumber(c) || c == '.'))
                    {
                        altitude = Convert.ToSingle(splitData[5]);
                    }
                    else
                    {
                        altitude = 0.0f;
                    }

                    if (splitData[6].Length > 0 && splitData[6].All(c => Char.IsNumber(c) || c == '.'))
                    {
                        air_speed = Convert.ToSingle(splitData[6]);
                    }
                    else
                    {
                        air_speed = 0.0f;
                    }

                    if (splitData[9].Length > 0 && splitData[9].All(c => Char.IsNumber(c) || c == '.'))
                    {
                        temperature = Convert.ToSingle(splitData[9]);
                    }
                    else
                    {
                        temperature = 0.0f;
                    }

                    if (splitData[10].Length > 0 && splitData[10].All(c => Char.IsNumber(c) || c == '.'))
                    {
                        voltage = Convert.ToSingle(splitData[10]);
                    }
                    else
                    {
                        voltage = 0.0f;
                    }

                    if (splitData[11].Length > 0 && splitData[11].All(c => Char.IsNumber(c) || c == '.'))
                    {
                        pressure = Convert.ToSingle(splitData[11]);
                    }
                    else
                    {
                        pressure = 0.0f;
                    }

                    if (splitData[12].Length > 0 && splitData[12].All(c => Char.IsNumber(c) || c == '.' || c == ':'))
                    {
                        gps_time = Convert.ToString(splitData[12]);
                    }
                    else
                    {
                        gps_time = "00:00:00";
                    }

                    if (splitData[13].Length > 0 && splitData[13].All(c => Char.IsNumber(c) || c == '.'))
                    {
                        gps_altitude = Convert.ToSingle(splitData[13]);
                    }
                    else
                    {
                        gps_altitude = 0.0f;
                    }

                    gps_latitude = Convert.ToSingle(splitData[14]);
                    gps_longitude = Convert.ToSingle(splitData[15]);

                    if (splitData[16].Length > 0 && splitData[16].All(c => Char.IsNumber(c)))
                    {
                        gps_sats_count = Convert.ToUInt32(splitData[16]);
                    }
                    else
                    {
                        gps_sats_count = 0;
                    }

                    tilt_x = Convert.ToSingle(splitData[17]);
                    tilt_y = Convert.ToSingle(splitData[18]);
                    rot_z = Convert.ToSingle(splitData[19]);

                    heading = Convert.ToSingle(splitData[22]);

                    missionTime = splitData[1];
                    mode = splitData[3].ToCharArray()[0];
                    state = splitData[4];
                    hs_status = splitData[7].ToCharArray()[0];
                    pc_status = splitData[8].ToCharArray()[0];

                    cmd_echo = splitData[20];
                    if (missionTime.Length > 8)
                    {
                        missionTime = missionTime.Substring(0, missionTime.Length - 3);
                    }
                    if (gps_time.Length > 8)
                    {
                        gps_time = gps_time.Substring(0, gps_time.Length - 3);
                    }
                    System.Diagnostics.Debug.WriteLine("VerifyData : The Data " + teamId + "," + missionTime + "," + packetCount + "," + mode + "," + state + "," + altitude + "," + air_speed + "," + hs_status + "," + pc_status + "," + temperature + "," + voltage + "," + pressure + "," + gps_time + "," + gps_altitude + "," + gps_latitude + "," + gps_longitude + "," + gps_sats_count + "," + tilt_x + "," + tilt_y + "," + rot_z + "," + cmd_echo + "," + heading);
                }
                else if (splitData.Length == 1)
                {
                    cmd_echo = splitData[20];
                }


                // checkSum = Convert.ToDouble(splitData[34]);

                max_volt = voltage;
                if (Math.Abs(max_volt) > this.max_max_volt)
                {
                    max_max_volt = Math.Abs(max_volt);
                }

                max_altitude = altitude;
                if (Math.Abs(max_altitude) > this.max_max_altitude)
                {
                    max_max_altitude = Math.Abs(max_altitude);
                }

                speed = Math.Abs(altitude - last_altitude);
                last_altitude = altitude;

                System.Diagnostics.Debug.WriteLine("VerifyData : The Speed " + speed);
                max_temperature = temperature;
                if (Math.Abs(max_temperature) > this.max_max_temperature)
                {
                    max_max_temperature = Math.Abs(max_temperature);
                }

                min_pressure = pressure;
                if (Math.Abs(min_pressure) < this.min_min_pressure || this.min_min_pressure == 0.0f)
                {
                    min_min_pressure = Math.Abs(min_pressure);
                }

                max_packetCount = packetCount;
                if (Math.Abs(max_packetCount) > this.max_max_packetCount)
                {
                    max_max_packetCount = max_packetCount;
                }


                if (!timergraph.IsEnabled)
                {
                    timergraph.Start();
                }
                this.Dispatcher.BeginInvoke(new uiupdater(ShowTelemetryData));
                if (model.Content != null) //3D
                {
                    Debug.WriteLine("Get Rotated");
                    this.Dispatcher.Invoke(() =>
                    {
                        RotateBackModel();
                    });
                }
                //graphOnline = true;
                this.Dispatcher.BeginInvoke(new uiupdater(GmapView_Region));
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch
            {
                return;
            }
        }

        private void RotateBackModel()
        {
            model.Content.Transform = new MatrixTransform3D(Matrix3D.Identity);
            //Matrix3D matrix = model.Content.Transform.Value;
            //axis = axis*-1;

            //matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(axis, 0));
            //model.Content.Transform = new MatrixTransform3D(matrix);
        }

        internal void ShowTelemetryData()
        {
            Debug.WriteLine("Tampilkan data telemetry");
            MissionTimeLabel.Text = String.Format("{00:00:00}", missionTime);
            SoftwareStateLabel.Text = String.Format("{0}", state);

            //#region 3D Item


            if ((string)ThreeDModelBtn.Content != "Started")
            {
                //ThreeDModelStatus.Content = "Started";
                ThreeDModelBtn.Content = "Started";
                this.Dispatcher.Invoke(() =>
                {
                    HelixViewport3D_Load();
                });
            }


            if (hs_status == 'P' && !hs_deployed)
            {
                this.Dispatcher.Invoke(() =>
                {
                    modelCamera.FieldOfView = 35;
                    modelCamera.Position = new Point3D(0.000, 100.000, 1800.000);
                    HelixViewport3D_Load();
                });
                hs_deployed = true;
                pc_deployed = false;

            }
            if (pc_status == 'C' && !pc_deployed)
            {
                this.Dispatcher.Invoke(() =>
                {
                    modelCamera.FieldOfView = 60;
                    modelCamera.Position = new Point3D(0.000, 340.000, 1800.000);
                    HelixViewport3D_Load();
                });
                pc_deployed = true;

                hs_deployed = false;
            }



            if (tilt_x == 0.00 && tilt_y == 0.00)
            {
                axis = new Vector3D(0.0000000000001, 0.0000000000001, 0.0000000000001);
            }
            else
            {
                if (tilt_x == 0.00)
                {
                    axis = new Vector3D(0.0000000000001, 0.0000000000001, (double)tilt_y);
                }
                else if (tilt_y == 0.00)
                {
                    axis = new Vector3D((double)tilt_x, 0.0000000000001, 0.0000000000001);
                }
                else
                {
                    Debug.WriteLine("Axis created");
                    axis = new Vector3D((double)tilt_x, 0.0000000000001, (double)tilt_y);
                }
            }

            // Create a new quaternion based on axis and angle
            double angle = Math.Sqrt(tilt_x * tilt_x + tilt_y * tilt_y);
            Debug.WriteLine("Axis and Angle : {0} {1} {2} {3}", angle, axis.X, axis.Y, axis.Z);
            System.Windows.Media.Media3D.Quaternion rotation = new System.Windows.Media.Media3D.Quaternion(axis, angle);

            // Get the current transformation matrix from the model
            Matrix3D matrix = model.Content.Transform.Value;

            // Apply the rotation to the existing matrix
            matrix.Rotate(rotation);

            // Set the updated transformation matrix back to the model
            this.Dispatcher.Invoke(() =>
            {
                model.Content.Transform = new MatrixTransform3D(matrix);
            });

            //#endregion

            try
            {
                // Graph Label
                SatellitCountPlot.Plot.XLabel("Sats: " + gps_sats_count);
                SatellitCountPlot.Plot.XAxis.Label(size: 14);
                PressurePlot.Plot.XLabel("Pressure: " + pressure + " kPa");
                PressurePlot.Plot.XAxis.Label(size: 14);
                AirSpeedPlot.Plot.XLabel("AirSpeed: " + pressure + " m/s");
                AirSpeedPlot.Plot.XAxis.Label(size: 14);
                AltitudePlot.Plot.XLabel("CanSat: " + altitude + " m GPS: " + gps_altitude + " m" + " Speed: " + speed + " m/s");
                AltitudePlot.Plot.XAxis.Label(size: 14);
                TemperaturePlot.Plot.XLabel("Temp: " + temperature + " °C");
                TemperaturePlot.Plot.XAxis.Label(size: 14);
                VoltagePlot.Plot.XLabel("Volt: " + voltage + " volts");
                VoltagePlot.Plot.XAxis.Label(size: 14);
                LatLongPlot.Plot.XLabel("Lat: " + gps_latitude + " Long: " + gps_longitude);
                LatLongPlot.Plot.XAxis.Label(size: 14);
                TiltPlot.Plot.XLabel("X: " + tilt_x + " Y: " + tilt_y);
                TiltPlot.Plot.XAxis.Label(size: 14);


                // Checksum
                lblValidatedData.Text = validCount.ToString();
                lblCorruptedData.Text = corruptCount.ToString();
                lblTotalData.Text = totalCanSatData.ToString();
                //    #region Dashboard Item

                // GPS
                //GPSTimeLabel.Text = String.Format("{00:00:00}", gps_time);
                GPSCountLabel.Text = String.Format("{0:00}", gps_sats_count);
                GPSLongitudeLabel.Text = String.Format("{0:0.0000}", gps_longitude);
                GPSLatitudeLabel.Text = String.Format("{0:0.0000}", gps_latitude);
                GPSAltitudeLabel.Text = String.Format("{0:0.0}", gps_altitude);

                // Cansat
                MaxAirSpeedLabel.Text = String.Format("{0:0.0}", max_max_air_speed);
                MaxAltitudeLabel.Text = String.Format("{0:0.0}", max_max_altitude);
                MaxVoltageLabel.Text = String.Format("{0:0.0}", max_max_volt);
                MaxTemperatureLabel.Text = String.Format("{0:0.0}", max_max_temperature);
                MaxPressureLabel.Text = String.Format("{0:0.0}", min_min_pressure);
                MaxPacketCountLabel.Text = String.Format("{0:0000}", max_max_packetCount);
                PacketCountLabel.Text = String.Format("{0:0000}", totalCanSatData);
                AltitudeLabel.Text = String.Format("{0:0.0}", altitude);
                AirSpeedLabel.Text = String.Format("{0:0.0}", air_speed) + "m/s";
                TemperatureLabel.Text = String.Format("{0:0.0}", temperature);
                VoltageLabel.Text = String.Format("{0:0.0}", voltage);
                PressureLabel.Text = String.Format("{0:0.0}", pressure);
                TiltXLabel.Text = String.Format("{0:0.00}", tilt_x);
                TiltYLabel.Text = String.Format("{0:0.00}", tilt_y);
                RotZLabel.Text = String.Format("{0:0.00}", rot_z);
                HeadingLabel.Text = String.Format("{0:0.00}", heading);
                //CMDEchoLabel.Text = String.Format("{0}", cmd_echo);
                //    #endregion

                // CanSat Battery
                CanSatBaterryVoltage.Text = String.Format("{0:0.0}", voltage);

                SolidColorBrush red = (SolidColorBrush)new BrushConverter().ConvertFromString("#7D1C1C");
                SolidColorBrush white = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");
                SolidColorBrush LimeGreen = (SolidColorBrush)new BrushConverter().ConvertFromString("#5BC12C");
                SolidColorBrush black = (SolidColorBrush)new BrushConverter().ConvertFromString("#000000");
                if (pc_status == 'N')
                {
                    ParachuteReleasedPane.Background = red;
                    ParachuteReleased.Foreground = white;
                }
                else
                {
                    ParachuteReleasedPane.Background = LimeGreen;
                    ParachuteReleased.Foreground = black;
                }

                if (hs_status == 'N')
                {
                    HeatShieldReleasedPane.Background = red;
                    HeatShieldReleased.Foreground = white;
                }
                else
                {
                    HeatShieldReleasedPane.Background = LimeGreen;
                    HeatShieldReleased.Foreground = black;
                }

                ParachuteReleased.Content = String.Format("{0}", pc_status);
                HeatShieldReleased.Content = String.Format("{0}", hs_status);


                if (cmd_echo == "SIMENABLE" && mode == 'F')
                {
                    CanSatMode.Content = "S";
                    CanSatMode.Foreground = System.Windows.Media.Brushes.White;
                    CanSatModePane.Background = System.Windows.Media.Brushes.Red;


                    SimulationStatus.Content = "ENABLE";
                    SimulationStatus.Foreground = kuning;
                    SimulationStatus.Background = System.Windows.Media.Brushes.LimeGreen;


                }
                else if (cmd_echo == "SIMACTIVATE" && mode == 'S')
                {

                    CanSatMode.Content = "S";
                    CanSatMode.Foreground = System.Windows.Media.Brushes.White;
                    CanSatModePane.Background = System.Windows.Media.Brushes.Red;

                    SimulationStatus.Content = "ACTIVE";
                    SimulationStatus.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    SimulationStatus.Background = System.Windows.Media.Brushes.LimeGreen;
                }
                else if (cmd_echo == "SIMDISABLE" && mode == 'F')
                {
                    CanSatMode.Content = "F";
                    CanSatMode.Foreground = System.Windows.Media.Brushes.Black;
                    CanSatModePane.Background = System.Windows.Media.Brushes.LimeGreen;

                    SimulationStatus.Content = "DISABLE";
                    SimulationStatus.Foreground = merah;
                    SimulationStatus.Background = System.Windows.Media.Brushes.Red;
                }

            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void HelixViewport3D_Load()
        {

            if (hs_status == 'N' && pc_status == 'N' || hs_status == '\0' && pc_status == '\0')
            {
                modelMain = model1;
            }
            if (hs_status == 'P' && !hs_deployed)
            {
                modelMain = model2;
            }
            if (pc_status == 'C' && !pc_deployed)
            {
                modelMain = model3;
            }

            // Set the new 3D model to the content of the Viewport3D
            model.Content = modelMain;

            // Reset the transformation matrix to the identity matrix
            //model.Content.Transform = new MatrixTransform3D(Matrix3D.Identity);


        }

        private void CanDataLog()
        {
            if (_serialPort.IsOpen == true)
            {
                try
                {
                    if (mode == 'S')
                    {
                        cansatFileLog = binAppPath + "\\LogData\\SIMULATION\\Flight_" + teamId + ".csv";
                    }
                    else
                    {
                        cansatFileLog = binAppPath + "\\LogData\\FLIGHT\\Flight_" + teamId + ".csv";
                    }
                    BackgroundWorker worker = new();

                    worker.DoWork += delegate (object s, DoWorkEventArgs args)
                    {


                        var fileCon = System.IO.File.Open(cansatFileLog, (FileMode)FileIOPermissionAccess.Append, FileAccess.Write, FileShare.Read);
                        writeCan = new StreamWriter(fileCon, Encoding.GetEncoding(1252))
                        {
                            AutoFlush = true
                        };
                        writeCan.Write("TEAM_ID,MISSION_TIME,PACKET_COUNT,MODE,STATE,ALTITUDE,AIR_SPEED,HS_DEPLOYED," +
                                        "PC_DEPLOYED,TEMPERATURE,VOLTAGE,PRESSURE,GPS_TIME,GPS_ALTITUDE,GPS_LATITUDE,GPS_LONGITUDE,GPS_SATS," +
                                        "TILT_X,TILT_Y,ROT_Z,CMD_ECHO,HEADING \n");
                    };
                    worker.RunWorkerAsync();
                }
                catch (NullReferenceException)
                {
                    return;
                }
                catch (IndexOutOfRangeException)
                {
                    return;
                }
                catch (FormatException)
                {
                    return;
                }
                catch
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void BindData(string filePath) //Pengambilan Data
        {
            try
            {
                Col4.Clear();
                string ext = System.IO.Path.GetExtension(filePath);
                DataTable dt = new();
                if (ext == ".csv")
                {
                    string[] data = System.IO.File.ReadAllLines(filePath);
                    string[] kolom = null;
                    int x = 0;

                    foreach (string baris in data)
                    {
                        kolom = baris.Split(',');

                        if (x == 0)
                        {
                            for (int i = 0; i <= kolom.Count() - 1; i++)
                            {
                                dt.Columns.Add(kolom[i]);
                            }
                            x++;
                        }
                        else
                        {
                            kolom[1] = teamId.ToString();
                            dt.Rows.Add(kolom);
                        }
                    }
                }
                else if (ext == ".txt")
                {
                    using StreamReader file = new(filePath);

                    string ln;
                    string[] kolom = null;
                    string[] header = { "Command", "Team ID", "Simp", "Pressure" };
                    int x = 0;
                    while ((ln = file.ReadLine()) != null)
                    {
                        if (ln.StartsWith("#")) continue;
                        if (ln == "") continue;
                        kolom = ln.Split(',');
                        kolom[1] = teamId.ToString();
                        if (x == 0)
                        {
                            for (int i = 0; i <= kolom.Count() - 1; i++)
                            {
                                dt.Columns.Add(header[i]);
                            }
                            x++;
                        }
                        if (x != 0)
                        {
                            dt.Rows.Add(kolom);
                        }
                    }
                    file.Close();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("File yang anda pilih bukan file CSV atau TXT");
                }
                DataCsv.ItemsSource = dt.DefaultView;
                DataCsv.ScrollIntoView(DataCsv.Items[DataCsv.Items.Count - 1]);

            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch
            {
                return;
            }
        }

        private void SimDataClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DataPage.SelectedIndex = 0;
        }

        private void CanSatDataClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DataPage.SelectedIndex = 1;
        }

        private void ConnectPortBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_serialPort.IsOpen == false)
            {
                try
                {
                    if (ComportDropdown.SelectedItem == null)
                    {
                        System.Windows.Forms.MessageBox.Show("Comport Can't be Empty!", "Please select a Comport first!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    else if (BaudrateDropdown.SelectedItem == null)
                    {
                        System.Windows.Forms.MessageBox.Show("Baudrate Can't be Empty!", "Please select a Baudrate first!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    else
                    {
                        SerialPortName = ComportDropdown.SelectedItem.ToString();
                        SerialPortBaudrate = BaudrateDropdown.SelectedItem.ToString();
                        _serialPort.PortName = ComportDropdown.SelectedItem.ToString();
                        _serialPort.BaudRate = Convert.ToInt32(BaudrateDropdown.SelectedItem);
                        _serialPort.NewLine = "\r\n";
                        _serialPort.Close();
                        _serialPort.Open();
                        TransmissionStatus.Text = "Connected";
                        PortStatus.Content = "Connected";
                        PortStatus.Foreground = System.Windows.Media.Brushes.LimeGreen;
                        PortStatusPane.Background = System.Windows.Media.Brushes.LimeGreen;
                        ConnectPortBtn.Visibility = System.Windows.Visibility.Hidden;
                        DisconnectPortBtn.Visibility = System.Windows.Visibility.Visible;
                        CMDTextBox1.IsReadOnly = false;
                    }
                }
                catch (Exception)
                {

                    //System.Windows.Forms.MessageBox.Show(coy.Message, "Waduh, belom nyambung serial port weh!", MessageBoxButtons.OK, MessageBoxIcon.Error); //Can't Connect To Serial Port, Try Again

                    PortStatus.Content = "Try Again!";
                    PortStatusPane.Background = System.Windows.Media.Brushes.Red;

                    return;


                }
            }
        }



        private void DisconnectPortBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_serialPort.IsOpen == true)
            {
                try
                {
                    _serialPort.Close();
                    //PortStatus.Content = "Disconnected From Serial Port";
                    PortStatus.Content = "Disconnected";
                    //SerialDataStatus.Content = "Idle";
                    PortStatus.Foreground = merah;
                    PortStatusPane.Background = System.Windows.Media.Brushes.Red;
                    ConnectPortBtn.Visibility = System.Windows.Visibility.Visible;
                    DisconnectPortBtn.Visibility = System.Windows.Visibility.Hidden;
                    //CMDTextBox1.IsReadOnly = true;
                    timergraph.Stop();
                    //missionTimeOld = null;
                }
                catch (Exception ex)
                {
                    //#region messageBox
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Error when attempting to disconnect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //#endregion
                    return;
                }
            }
        }

        private void ClearRefreshListPort()
        {
            string[] CompPorts = SerialPort.GetPortNames();
            ComportDropdown.Items.Clear();
            foreach (string ComPort in CompPorts)
            {
                ComportDropdown.Items.Add(ComPort);
            }
        }

        private void RestartPortBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                _serialPort.Close();
                PortStatus.Content = "Disconnected";
                PortStatusPane.Background = System.Windows.Media.Brushes.Red;
                PortStatus.Foreground = System.Windows.Media.Brushes.Red;

                ComportDropdown.SelectedValue = null;
                BaudrateDropdown.SelectedValue = null;

                ConnectPortBtn.Visibility = System.Windows.Visibility.Visible;
                DisconnectPortBtn.Visibility = System.Windows.Visibility.Hidden;

                ClearRefreshListPort();
            }
            catch
            {
                return;
            }
        }

        private void WindowLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // inisiasi file flight dan simulation
            cansatFileLog = binAppPath + "\\LogData\\SIMULATION\\Flight_" + teamId + ".csv";
            if (System.IO.File.Exists(cansatFileLog))
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd hh-mm tt");
                //string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                if (!Directory.Exists(binAppPath + "\\LogData\\SIMULATION\\" + date + "\\"))
                {
                    Directory.CreateDirectory(binAppPath + "\\LogData\\SIMULATION\\" + date + "\\");
                }
                System.IO.File.Move(cansatFileLog, binAppPath + "\\LogData\\SIMULATION\\" + date + "\\Flight_" + teamId + ".csv");
            }
            cansatFileLog = binAppPath + "\\LogData\\FLIGHT\\Flight_" + teamId + ".csv";
            if (System.IO.File.Exists(cansatFileLog))
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd hh-mm tt");
                //string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                if (!Directory.Exists(binAppPath + "\\LogData\\FLIGHT\\" + date + "\\"))
                {
                    Directory.CreateDirectory(binAppPath + "\\LogData\\FLIGHT\\" + date + "\\");
                }
                System.IO.File.Move(cansatFileLog, binAppPath + "\\LogData\\FLIGHT\\" + date + "\\Flight_" + teamId + ".csv");
            }

            watcher.StatusChanged += Watcher_StatusChanged;
            //watcher.Start();


            try
            {
                string[] CompPorts = SerialPort.GetPortNames();
                foreach (string ComPort in CompPorts)
                {
                    ComportDropdown.Items.Add(ComPort);
                }
                string[] BaudRate = { "2400", "4800", "9600", "19200", "31250", "38400", "57600", "74880", "115200", "230400", "250000" };
                foreach (string baud in BaudRate)
                {
                    BaudrateDropdown.Items.Add(baud);
                }

                CanSatMode.Content = "F";
                CanSatModePane.Background = System.Windows.Media.Brushes.LimeGreen;
                SimulationStatus.Content = "DISABLE";
                SimulationStatusPane.Background = System.Windows.Media.Brushes.Red;

                this.WriteTelemetryData = new AddDataDelegate(NewLine);
                GC.Collect();
            }
            catch
            {
                return;
            }

        }

        private void NewLine(string NewLine)
        {
            SerialControlTextBox.AppendText(NewLine);
        }

        private void GmapView_Load()
        {
            try
            {
                //Bitmap IconGCS = new Bitmap(binAppPath + "/Icon/EEPISat.png");
                GmapView.MapProvider = GoogleMapProvider.Instance;
                GmapView.Manager.Mode = AccessMode.ServerAndCache;
                GmapView.CacheLocation = binAppPath + "\\MapCache\\";
                GmapView.MinZoom = 0;
                GmapView.MaxZoom = 18;
                GmapView.Zoom = 15;
                GmapView.ShowCenter = false;
                GmapView.DragButton = MouseButtons.Left;
                if (watcher.Position.Location.Latitude.ToString() != "NaN" && watcher.Position.Location.Longitude.ToString() != "NaN")
                {
                    GmapView.Position = new PointLatLng(watcher.Position.Location.Latitude, watcher.Position.Location.Longitude);
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        GMapOverlay markers = new("markers");
                        var markerGCS = new GMapOverlay("markers");
                        mapMarkerGCS = new GMarkerGoogle(new PointLatLng(watcher.Position.Location.Latitude, watcher.Position.Location.Longitude), GMarkerGoogleType.arrow);

                        markers.Markers.Add(mapMarkerGCS);
                        GmapView.Overlays.Add(markers);
                    }));
                }
                else if (statusRegion == "Virginia")
                {
                    GmapView.Position = new PointLatLng(37.196334, -80.578348);
                }
                else if (statusRegion == "Surabaya")
                {
                    GmapView.Position = new PointLatLng(-7.2740428, 112.7986227);
                }
                GC.Collect();
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch (OverflowException)
            {
                return;
            }
            catch
            {
                return;
            }
        }

        private void GmapViewHome_Load(object sender, EventArgs e)
        {
            try
            {
                //Bitmap IconGCS = new Bitmap(binAppPath + "/Icon/EEPISat.png");

                GmapViewHome.MapProvider = GoogleSatelliteMapProvider.Instance;
                GmapViewHome.Manager.Mode = AccessMode.ServerAndCache;
                GmapViewHome.CacheLocation = binAppPath + "\\MapCache\\";
                GmapViewHome.MinZoom = 0;
                GmapViewHome.MaxZoom = 18;
                GmapViewHome.Zoom = 16;
                GmapViewHome.ShowCenter = false;
                GmapViewHome.DragButton = MouseButtons.Left;
                if (watcher.Position.Location.Latitude.ToString() != "NaN" && watcher.Position.Location.Longitude.ToString() != "NaN")
                {
                    GmapViewHome.Position = new PointLatLng(watcher.Position.Location.Latitude, watcher.Position.Location.Longitude);
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        GCSStatusText.Content = "In Service";
                        GCSStatusIcon.Foreground = System.Windows.Media.Brushes.LimeGreen;
                        GCSCoordinateText.Content = watcher.Position.Location.Latitude.ToString().Substring(0, 7) + ", " + watcher.Position.Location.Longitude.ToString().Substring(0, 7);
                        GCSLocationStatus.Text = watcher.Position.Location.Latitude.ToString().Substring(0, 7) + ", " + watcher.Position.Location.Longitude.ToString().Substring(0, 7);

                        // hardware butuh
                        //// dummy gcs
                        //double tempLat = -7.2081;
                        //double tempLong = 112.77;
                        //GCSCoordinateText.Content = tempLat.ToString() + ", " + tempLong.ToString();
                        // dummy payload
                        //double tempGpsLat = -7.1827;
                        //double tempGpsLong = 112.7806;
                        //string Platitude = tempGpsLat.ToString();
                        //string Plongitude = tempGpsLong.ToString();
                        //PayloadCoordinateText.Content = Platitude + ", " + Plongitude;
                        //GMapOverlay markersP = new("markers");
                        //var markerPayload = new GMapOverlay("markers");
                        //mapMarkerPayload = new GMarkerGoogle(new PointLatLng(tempGpsLat, tempGpsLong), GMarkerGoogleType.blue);

                        //markersP.Markers.Add(mapMarkerPayload);
                        //GmapViewHome.Overlays.Add(markersP);

                        // end of hardware butuh

                        GMapOverlay markers = new("markers");
                        var markerGCS = new GMapOverlay("markers");
                        mapMarkerGCS = new GMarkerGoogle(new PointLatLng(watcher.Position.Location.Latitude, watcher.Position.Location.Longitude), GMarkerGoogleType.arrow);
                        // hardware butuh
                        //mapMarkerGCS = new GMarkerGoogle(new PointLatLng(tempLat, tempLong), GMarkerGoogleType.arrow);
                        // end of hardware butuh
                        markers.Markers.Add(mapMarkerGCS);
                        GmapViewHome.Overlays.Add(markers);
                    }));
                }
                else if (statusRegion == "Virginia")
                {
                    GmapViewHome.Position = new PointLatLng(37.196334, -80.578348);
                }
                else if (statusRegion == "Surabaya")
                {
                    GmapViewHome.Position = new PointLatLng(-7.2740428, 112.7986227);
                }


                if (_serialPort.IsOpen == true)
                {
                    if (gps_latitude != 0 && gps_longitude != 0)
                    {
                        if (mapMarkerPayload == null)
                        {
                            CanSatStatusText.Content = "In Service";
                            CanSatStatusIcon.Foreground = System.Windows.Media.Brushes.LimeGreen;
                            string Platitude = gps_latitude.ToString();
                            string Plongitude = gps_longitude.ToString();
                            PayloadCoordinateText.Content = Platitude.Substring(0, 7) + ", " + Plongitude.Substring(0, 7);
                            GMapOverlay markersP = new("markers");
                            var markerPayload = new GMapOverlay("markers");
                            mapMarkerPayload = new GMarkerGoogle(new PointLatLng(gps_latitude, gps_longitude), GMarkerGoogleType.blue);

                            markersP.Markers.Add(mapMarkerPayload);
                            GmapViewHome.Overlays.Add(markersP);
                        }
                    }
                    else
                    {
                        CanSatStatusText.Content = "Locking GPS....";
                        CanSatStatusIcon.Foreground = System.Windows.Media.Brushes.Blue;
                    }
                }
                else
                {
                    //PayloadStatusText.Content = "Out Of Service";
                    //PayloadStatusIcon.Foreground = System.Windows.Media.Brushes.Red;
                }
                GC.Collect();
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch (OverflowException)
            {
                return;
            }
            catch
            {
                return;
            }
        }

        public void GmapView_Region()
        {
            try
            {
                if (_serialPort.IsOpen == true)
                {
                    try
                    {
                        if (gps_latitude != 0 && gps_longitude != 0)
                        {
                            CanSatStatusText.Content = "In Service";
                            CanSatStatusIcon.Foreground = System.Windows.Media.Brushes.LimeGreen;
                            if (mapMarkerPayload != null)
                            {
                                Debug.WriteLine("Payload Disini!");
                                string Platitude = gps_latitude.ToString();
                                string Plongitude = gps_longitude.ToString();
                                PayloadCoordinateText.Content = Platitude + ", " + Plongitude;
                                mapMarkerPayload.Position = new PointLatLng(gps_latitude, gps_longitude);
                                mapMarkerPayload.ToolTipText = $"Payload\n" + $" Latitude : {gps_latitude}, \n" + $" Longitude : {gps_longitude}";

                                if (watcher.Position.Location.Latitude.ToString() != "NaN" && watcher.Position.Location.Longitude.ToString() != "Nan")
                                {
                                    double betweenlat = gps_latitude + watcher.Position.Location.Latitude;
                                    double betweenlng = gps_longitude + watcher.Position.Location.Longitude;
                                    GmapViewHome.Position = new PointLatLng(betweenlat / 2, betweenlng / 2);
                                    if ((string)MiniMapBtn.Content == "Started")
                                    {
                                        GmapView.Position = new PointLatLng(betweenlat / 2, betweenlng / 2);
                                    }
                                }
                                else
                                {
                                    GmapViewHome.Position = new PointLatLng(gps_latitude, gps_longitude);
                                    if ((string)MiniMapBtn.Content == "Started")
                                    {
                                        GmapView.Position = new PointLatLng(gps_latitude, gps_longitude);
                                    }
                                }
                                if ((string)MiniMapBtn.Content == "Started" && minimap_check == false)
                                {
                                    minimap_check = true;
                                    GMapOverlay markersP = new("markers");
                                    markersP.Markers.Add(mapMarkerPayload);
                                    GmapView.Overlays.Add(markersP);
                                }
                            }
                            else
                            {
                                string Platitude = gps_latitude.ToString();
                                string Plongitude = gps_longitude.ToString();
                                PayloadCoordinateText.Content = Platitude + ", " + Plongitude;
                                GMapOverlay markersP = new("markers");
                                var markerPayload = new GMapOverlay("markers");
                                mapMarkerPayload = new GMarkerGoogle(new PointLatLng(gps_latitude, gps_longitude), GMarkerGoogleType.blue);

                                markersP.Markers.Add(mapMarkerPayload);
                                GmapViewHome.Overlays.Add(markersP);
                                if ((string)MiniMapBtn.Content == "Started" && minimap_check == false)
                                {
                                    minimap_check = true;
                                    GmapView.Overlays.Add(markersP);
                                    if (watcher.Position.Location.Latitude.ToString() != "NaN" && watcher.Position.Location.Longitude.ToString() != "Nan")
                                    {
                                        double betweenlat = gps_latitude + watcher.Position.Location.Latitude;
                                        double betweenlng = gps_longitude + watcher.Position.Location.Longitude;
                                        GmapView.Position = new PointLatLng(betweenlat / 2, betweenlng / 2);
                                    }
                                    else
                                    {
                                        GmapView.Position = new PointLatLng(gps_latitude, gps_longitude);
                                    }
                                }
                            }
                        }
                        else
                        {
                            CanSatStatusText.Content = "Locking GPS....";
                            CanSatStatusIcon.Foreground = System.Windows.Media.Brushes.Blue;
                        }
                    }
                    catch (NullReferenceException)
                    {
                        return;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return;
                    }
                    catch (FormatException)
                    {
                        return;
                    }
                    catch
                    {
                        return;
                    }
                    if (watcher.Position.Location.Latitude.ToString() != "NaN" && watcher.Position.Location.Longitude.ToString() != "Nan")
                    {
                        double dLat1InRad = gcs_latitude * (Math.PI / 180);
                        double dLong1InRad = gcs_longitude * (Math.PI / 180);

                        double dLat2InRad = gps_latitude * (Math.PI / 180);
                        double dLong2InRad = gps_longitude * (Math.PI / 180);

                        double dLongitude = dLong2InRad - dLong1InRad;
                        double dLatitude = dLat2InRad - dLat1InRad;

                        // Intermediate result a.
                        double a = Math.Pow(Math.Sin(dLatitude / 2), 2) + Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);

                        // Intermediate result c (great circle distance in Radians).
                        double c = 2 * Math.Asin(Math.Sqrt(a));

                        // Perhitungan
                        const Double kEarthRadiusKms = 6371;
                        distance = kEarthRadiusKms * c * 1000;

                        GCSDistanceText.Content = String.Format("{0:0}", distance);
                    }
                }
                if (watcher.Position.Location.Latitude.ToString() != "NaN" && watcher.Position.Location.Longitude.ToString() != "Nan")
                {
                    if (mapMarkerGCS != null)
                    {
                        gcs_latitude = ((float)watcher.Position.Location.Latitude);
                        gcs_longitude = ((float)watcher.Position.Location.Longitude);
                        GCSCoordinateText.Content = Math.Round(gcs_latitude, 4).ToString() + ", " + Math.Round(gcs_longitude, 4).ToString();
                        GCSLocationStatus.Text = Math.Round(gcs_latitude, 4).ToString() + ", " + Math.Round(gcs_longitude, 4).ToString();
                        mapMarkerGCS.Position = new PointLatLng(watcher.Position.Location.Latitude, watcher.Position.Location.Longitude);
                        mapMarkerGCS.ToolTipText = $"GCS\n" + $" Latitude : {Math.Round(gcs_latitude, 4)}, \n" + $" Longitude : {Math.Round(gcs_longitude, 4)}";
                    }
                }


                if (gps_latitude == 0 && gps_longitude == 0)
                {
                    GCSDistanceText.Content = "Unknown";
                }
                GC.Collect();
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        private void WriteLogPayload()
        {
            try
            {
                writeCan.Write("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}\n"
                                , teamId, missionTime, packetCount, mode
                                , state, String.Format("{0:0.0}", altitude), String.Format("{0:0.0}", air_speed), hs_status, pc_status,
                                String.Format("{0:0.0}", temperature), String.Format("{0:0.0}", voltage), String.Format("{0:0.0}", pressure), gps_time, String.Format("{0:0.0}", gps_altitude), String.Format("{0:0.0000}", gps_latitude), String.Format("{0:0.0000}", gps_longitude),
                                gps_sats_count, String.Format("{0:0.00}", tilt_x), String.Format("{0:0.00}", tilt_y), String.Format("{0:0.00}", rot_z), cmd_echo, String.Format("{0:0.0}", heading));
                writeCan.Flush();
                DataCanSat dtp = new()
                {
                    Pteamid = teamId,
                    Pmissiontime = missionTime,
                    Ppacketcount = packetCount,
                    Pmode = mode,
                    Pstate = state,
                    Paltitude = String.Format("{0:0.0}", altitude),
                    Pair_speed = String.Format("{0:0.0}", air_speed),
                    Phs_status = hs_status,
                    Ppc_status = pc_status,
                    Ptemperature = String.Format("{0:0.0}", temperature),
                    Pvoltage = String.Format("{0:0.0}", voltage),
                    Ppressure = String.Format("{0:0.0}", pressure),
                    Pgps_time = gps_time,
                    Pgps_altitude = String.Format("{0:0.0}", gps_altitude),
                    Pgps_latitude = String.Format("{0:0.0000}", gps_latitude),
                    Pgps_longitude = String.Format("{0:0.0000}", gps_longitude),
                    Pgps_sats_count = gps_sats_count,
                    Ptilt_x = String.Format("{0:0.00}", tilt_x),
                    Ptilt_y = String.Format("{0:0.00}", tilt_y),
                    Prot_z = String.Format("{0:0.00}", rot_z),
                    Pcmd_echo = cmd_echo,
                    Pheading = String.Format("{0:0.0}", heading)
                };
                if (checkSumHasil)
                {
                    dtp.Pchecksum = "Valid";
                }
                else
                {
                    dtp.Pchecksum = "Corrupt";
                }

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    CanSatDataCsv.Items.Add(dtp);
                    if (auto_scroll)
                    {
                        CanSatDataCsv.ScrollIntoView(CanSatDataCsv.Items[CanSatDataCsv.Items.Count - 1]);
                    }
                }));
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (IndexOutOfRangeException)
            {
                return;
            }
            catch (FormatException)
            {
                return;
            }
            catch
            {
                return;
            }
        }

        private void BtnCsv_Click(object sender, EventArgs e)
        {
            if (CanSatDataCsv.Items.Count > 0)
            {
                CMDTextBox2.Text = "CanSat Data: " + CanSatDataCsv.Items.Count;
                System.Windows.Forms.SaveFileDialog sfd = new()
                {
                    Filter = "CSV (*.csv)|*.csv",
                    FileName = "Flight_" + teamId + ".csv"
                };
                bool fileError = false;
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    if (System.IO.File.Exists(sfd.FileName))
                    {

                        try
                        {
                            System.IO.File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            System.Windows.Forms.MessageBox.Show("File with this name is already exist. Please try again. " + ex.Message);
                        }
                    }
                    if (!fileError)
                    {

                        try
                        {
                            string payloadFileLogExport;
                            if (mode == 'S')
                            {
                                payloadFileLogExport = binAppPath + "\\LogData\\SIMULATION\\Flight_" + teamId + ".csv";
                            }
                            else
                            {
                                payloadFileLogExport = binAppPath + "\\LogData\\FLIGHT\\Flight_" + teamId + ".csv";
                            }

                            //System.IO.File.Copy(payloadFileLogExport, sfd.FileName);
                            CanSatDataCsv.SelectAllCells();
                            //CanSatDataCsv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
                            //var text = CanSatDataCsv.GetClipboardContent().GetText();
                            //System.IO.File.WriteAllText(payloadFileLogExport, text);
                            System.Windows.Forms.MessageBox.Show("Data Exported Successfully !!!", "Info");
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("No Record To Export !!!", "Info");
            }
        }

        public class DataCanSat
        {
            public Double Pteamid { get; set; }
            public String Pmissiontime { get; set; }
            public uint Ppacketcount { get; set; }
            public Char Pmode { get; set; }
            public String Pstate { get; set; }
            public string Paltitude { get; set; }
            public string Pair_speed { get; set; }
            public Char Phs_status { get; set; }
            public Char Ppc_status { get; set; }
            public string Ptemperature { get; set; }
            public string Ppressure { get; set; }
            public string Pvoltage { get; set; }
            public String Pgps_time { get; set; }
            public string Pgps_altitude { get; set; }
            public string Pgps_latitude { get; set; }
            public string Pgps_longitude { get; set; }
            public uint Pgps_sats_count { get; set; }
            public string Ptilt_x { get; set; }
            public string Ptilt_y { get; set; }
            public string Prot_z { get; set; }
            public String Pcmd_echo { get; set; }
            public string Pheading { get; set; }
            public String Pchecksum { get; set; }
        }

        private void Watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e) //watcher status map
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                if (watcher.Position.Location.IsUnknown)
                {
                    if (gcs_latitude == 0.0f && gcs_longitude == 0.0f)
                    {
                        GCSStatusText.Content = "Out Of Service";
                        GCSStatusIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                    }
                }
                else
                {
                    gcs_latitude = ((float)watcher.Position.Location.Latitude);
                    gcs_longitude = ((float)watcher.Position.Location.Longitude);
                    GCSStatusText.Content = "In Service";
                    GCSCoordinateText.Content = gcs_latitude.ToString().Substring(0, 7) + ", " + gcs_latitude.ToString().Substring(0, 7);
                    GCSLocationStatus.Text = gcs_latitude.ToString().Substring(0, 7) + ", " + gcs_longitude.ToString().Substring(0, 7);
                    GCSStatusIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 255, 0));
                }
            }
        }

        private void DataNavClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MainPage.SelectedIndex = 3;
        }

        private void SettingNavClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MainPage.SelectedIndex = 4;
        }

        private void MainPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ShutdownBtnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure to shutdown the GCS?", "", MessageBoxButton.OKCancel);
            switch (result)
            {
                case MessageBoxResult.OK:
                    //SoundPlayer player = new(binAppPath + "/Audio/GCSSTOP.wav");
                    //player.Play();
                    //Thread.Sleep(3000);
                    System.Windows.Application.Current.Shutdown();
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }

        private void RestartBtnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure to restart the application?", "", MessageBoxButton.OKCancel);

            switch (result)
            {
                case MessageBoxResult.OK:
                    // Menutup aplikasi saat ini
                    System.Windows.Application.Current.Shutdown();
                    SoundPlayer player = new(binAppPath + "/Audio/GCSSTOP.wav");
                    player.Play();

                    // Mendapatkan path aplikasi yang sedang berjalan
                    string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                    // Memulai aplikasi lagi
                    Process.Start(appPath);

                    // Mengakhiri proses saat ini
                    Environment.Exit(0);
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }

        private void MinimizeBtnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            // Meminimalkan jendela aplikasi
            WindowState = WindowState.Minimized;
        }

        public void GetBatteryPercent()
        {
            ManagementClass wmi = new("Win32_Battery");
            var allBatteries = wmi.GetInstances();
            int estimatedTimeRemaining, estimatedChargeRemaining;
            string final;
            foreach (var battery in allBatteries)
            {
                estimatedChargeRemaining = Convert.ToInt32(battery["EstimatedChargeRemaining"]);
                estimatedTimeRemaining = Convert.ToInt32(battery["EstimatedRunTime"]);
                x = estimatedChargeRemaining;
                y = estimatedTimeRemaining;

                BatteryStatus.Content = Convert.ToString(estimatedChargeRemaining) + "% Remaining";
            }
            if ((int)hasilRegLin > 715827)
            {
                jumlahDataRegLin = 0;
                sumX = 0;
                sumX2 = 0;
                sumY = 0;
                sumXY = 0;
                hasilRegLin = 0;
            }
            jumlahDataRegLin += 2;
            sumX += (x * 2);
            sumX2 += (x * 2) * (x * 2);
            sumY += (y * 2);
            sumXY += (x * 2) * (y * 2);
            m = (jumlahDataRegLin * sumXY - sumX * sumY) / (jumlahDataRegLin * sumX2 - sumX * sumX);
            c = (sumY - m * sumX) / jumlahDataRegLin;
            hasilRegLin = c + (m * x);
            final = Convert.ToString((int)hasilRegLin) + "" + " Minutes";

            if (y == 71582788)
            {
                var converter = new System.Windows.Media.BrushConverter();
                var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#00FF00");
                BatteryPercentage.Background = brush;
                final = "On Charging";
            }
            else if (y < 30)
            {
                var converter = new System.Windows.Media.BrushConverter();
                var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFF00");
                BatteryPercentage.Background = brush;
            }
            else if (y < 10)
            {
                var converter = new System.Windows.Media.BrushConverter();
                var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#FF0000");
                BatteryPercentage.Background = brush;
            }

            BatteryLaptopRemaining.Text = final;
            BatteryPercentage.Height = x / 2;

            if (jumlahDataRegLin >= 1000)
            {
                jumlahDataRegLin = 0;
                sumX = 0;
                sumX2 = 0;
                sumY = 0;
                sumXY = 0;
            }
            GC.Collect();
        }

        private async void GetPerformanceIndicator()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(2000);
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            Process currentProc = Process.GetCurrentProcess();
            long memoryUsed = currentProc.PrivateMemorySize64;
            CPUStatus.Text = (int)(cpuUsageTotal * 100) + " %";
            RAMStatus.Text = (memoryUsed / 1000000) + " MB";
            GC.Collect();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // CheckSerialPort();
            GetBatteryPercent();
            GetPerformanceIndicator();
        }

        private void CheckSerialPort()
        {
            if (!_serialPort.IsOpen)
            {
                if (!SerialPort.GetPortNames().Contains("COM15"))
                {
                    PortStatus.Content = "Disconnected";
                    TransmissionStatus.Text = "Disconnected";
                    //SerialDataStatus.Content = "Idle";
                    PortStatusPane.Background = System.Windows.Media.Brushes.Red;
                    ConnectPortBtn.Visibility = System.Windows.Visibility.Visible;
                    DisconnectPortBtn.Visibility = System.Windows.Visibility.Hidden;
                    CMDTextBox1.IsReadOnly = true;
                    ClearRefreshListPort();
                }
            }
        }

        private void CommandToCanSat()
        {
            if (CMDTextBox1.GetLineText(0) == "" && lineCommand == 0)
            {
                CMDTextBox2.Text = "Enter a command first lah brooo, plislaaa....";
            }
            else
            {
                string line = CMDTextBox1.GetLineText(0);
                if (line == "CMD,2032,CAL")
                {
                    try
                    {
                        string cmd = "CMD,2032,CAL";

                        StartTimer(cmd);


                        CMDTextBox2.Text += "\r\n" + "Payload is calibrating...." + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "CAL";

                        SoftwareStateLabel.Text = "MISSION_READY";

                        MaxAirSpeedLabel.Text = String.Format("{0:0.0}", 0);
                        MaxAltitudeLabel.Text = String.Format("{0:0.0}", 0);
                        MaxVoltageLabel.Text = String.Format("{0:0.0}", 0);
                        MaxTemperatureLabel.Text = String.Format("{0:0.0}", 0);
                        MaxPressureLabel.Text = String.Format("{0:0.0}", 0);
                        MaxPacketCountLabel.Text = String.Format("{0:0000}", 0);
                        max_max_air_speed = 0;
                        max_max_altitude = 0;
                        max_max_packetCount = 0;
                        min_min_pressure = 0;
                        max_max_temperature = 0;
                        max_max_volt = 0;


                        SolidColorBrush white = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF");

                        ParachuteReleasedPane.Background = merahdark;
                        ParachuteReleased.Foreground = white;

                        HeatShieldReleasedPane.Background = merahdark;
                        HeatShieldReleased.Foreground = white;


                        SoundPlayer player = new(binAppPath + "/Audio/CAL.wav");
                        player.Play();
                        CMDTextBox1.Text = "CMD,2032,CR";
                        CMDShortcut.SelectedIndex = -1;
                        openHeatShieldSimulation = false;
                        countOpenHeatShield = 0;
                    }
                    catch
                    {
                        return;
                    }


                }
                else if (line == "CMD,2032,CX,ON")
                {
                    try
                    {
                        string cmd = "CMD,2032,CX,ON";
                        StartTimer(cmd);



                        CMDTextBox2.Text += "\r\n" + "Activating payload telemetry......" + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "CXON";
                        SoundPlayer player = new(binAppPath + "/Audio/PXON.wav");
                        player.Play();
                        CMDTextBox1.Clear();
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,CR")
                {
                    try
                    {
                        string cmd = "CMD,2032,CR";
                        StartTimer(cmd);

                        CMDTextBox2.Text += "\r\n" + "CR Command is sended......" + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "CR";
                        CMDTextBox1.Text = "CMD,2032,CX,ON";
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,TC")
                {
                    try
                    {
                        string cmd = "CMD,2032,TC";
                        StartTimer(cmd);

                        CMDTextBox2.Text += "\r\n" + "TC Command is sended......" + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "TC";
                        CMDTextBox1.Clear();
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,CX,OFF")
                {
                    try
                    {
                        string cmd = "CMD,2032,CX,OFF";

                        StartTimer(cmd);




                        CMDTextBox2.Text += "\r\n" + "Deactivating CanSat telemetry" + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "CXOFF";
                        SoundPlayer player = new(binAppPath + "/Audio/PXOFF.wav");
                        player.Play();
                        timergraph.Stop();
                        //missionTimeOld = null;
                        CMDTextBox1.Clear();
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line.Contains("CMD,2032,ST,"))
                {
                    try
                    {
                        string lineSplit = line.Split(',')[3];
                        string pattern = @"^([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$";
                        if (lineSplit == "GPS")
                        {
                            try
                            {
                                string cmd = "CMD,2032,ST,GPS";

                                StartTimer(cmd);

                                CMDTextBox2.Text += "\r\n" + "Setting CanSat time by GPS....." + "\r\n";
                                CMDTextBox2.ScrollToEnd();

                                //SerialDataStatus.Content = "STGPS";
                                SoundPlayer player = new(binAppPath + "/Audio/STGPS.wav");
                                player.Play();
                                CMDTextBox1.Clear();
                                CMDShortcut.SelectedIndex = -1;
                            }
                            catch
                            {
                                return;
                            }

                        }
                        else if (Regex.IsMatch(lineSplit, pattern))
                        {
                            try
                            {
                                string cmd = "CMD,2032,ST," + lineSplit;

                                StartTimer(cmd);



                                CMDTextBox2.Text += "\r\n" + "Set time CanSat by UTC...." + "\r\n";
                                CMDTextBox2.ScrollToEnd();

                                //SerialDataStatus.Content = "ST" + lineSplit;
                                SoundPlayer player = new(binAppPath + "/Audio/STUTC.wav");
                                player.Play();
                                CMDTextBox1.Clear();
                                CMDShortcut.SelectedIndex = -1;
                            }
                            catch
                            {
                                return;
                            }
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,SIM,ENABLE")
                {
                    try
                    {
                        string cmd = "CMD,2032,SIM,ENABLE";

                        StartTimer(cmd);

                        SimulationStatus.Content = "ENABLE";
                        SimulationStatus.Foreground = kuning;
                        SimulationStatusPane.Background = System.Windows.Media.Brushes.Orange;


                        CMDTextBox2.Text += "\r\n" + "Enable CanSat simulation mode...." + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "SIMENABLE";
                        SoundPlayer player = new(binAppPath + "/Audio/SIMENABLE.wav");
                        player.Play();
                        CMDTextBox1.Text = "CMD,2032,SIM,ACTIVATE";
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,SIM,DISABLE")
                {
                    try
                    {
                        string cmd = "CMD,2032,SIM,DISABLE";

                        StartTimer(cmd);



                        //SerialDataStatus.Content = "SIMDISABLE";
                        CMDTextBox2.Text += "\r\n" + "Disable CanSat simulation mode...." + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        SimulationStatus.Content = "DISABLE";
                        SimulationStatus.Foreground = System.Windows.Media.Brushes.Red;

                        SimulationStatusPane.Background = System.Windows.Media.Brushes.Red;

                        //FlightOnOffSwitch.IsChecked = true;
                        CanSatMode.Content = "F";
                        CanSatMode.Foreground = System.Windows.Media.Brushes.Black;
                        CanSatModePane.Background = System.Windows.Media.Brushes.LimeGreen;

                        timerSimulation.Stop();
                        timerCSV = 0;

                        SoundPlayer player = new(binAppPath + "/Audio/SIMDISABLE.wav");
                        player.Play();
                        CMDTextBox1.Text = "CMD,2032,CAL";
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,SIM,ACTIVATE")
                {
                    try
                    {


                        if ((string)SimulationStatus.Content != "DISABLE")
                        {
                            string cmd = "CMD,2032,SIM,ACTIVATE";
                            StartTimer(cmd);
                            CMDTextBox2.Text += "\r\n" + "Activating CanSat simulation......" + "\r\n"; //+ Emoji.Use.Construction
                            CMDTextBox2.ScrollToEnd();

                            //FlightOnOffSwitch.IsChecked = false;
                            CanSatMode.Content = "S";
                            CanSatMode.Foreground = System.Windows.Media.Brushes.White;
                            CanSatModePane.Background = System.Windows.Media.Brushes.Red;

                            SimulationStatus.Content = "ACTIVE";
                            SimulationStatus.Foreground = System.Windows.Media.Brushes.LimeGreen;
                            SimulationStatusPane.Background = System.Windows.Media.Brushes.LimeGreen;
                            //SerialDataStatus.Content = "SIMACTIVATE";
                            //SoundPlayer player = new(binAppPath + "/Audio/SIMACTIVATE.wav");
                            //player.Play();
                            CMDTextBox1.Text = "CMD,2032,SIMP,PRESSURE";
                            CMDShortcut.SelectedIndex = -1;
                        }
                        else
                        {
                            CMDTextBox2.Text += "\r\n" + "Please enable simulation first!" + "\r\n"; //Emoji.Use.Warning +
                            CMDTextBox2.ScrollToEnd();
                            CMDTextBox1.Text = "CMD,2032,SIM,ENABLE";
                            CMDShortcut.SelectedIndex = -1;
                        }

                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line.Contains("CMD,2032,SIMP,"))
                {
                    string lineSplit = line.Split(',')[3];
                    if ((string)SimulationStatus.Content != "ACTIVATE" && (string)CanSatMode.Content == "F")
                    {
                        CMDTextBox2.Text += "\r\n" + "Enable and Activate simulation mode to use this command!" + "\r\n"; //+ Emoji.Use.Loudspeaker
                        CMDTextBox2.ScrollToEnd();
                    }
                    else if (lineSplit == "PRESSURE")
                    {

                        try
                        {

                            if (openFileDialog == null)
                            {
                                CMDTextBox2.Text += "\r\n" + "Please import simulation data....." + "\r\n"; //+ Emoji.Use.Loudspeaker
                                CMDTextBox2.ScrollToEnd();
                            }
                            else
                            {
                                if (openHeatShieldSimulation)
                                {
                                    timerCSV = 0;
                                    CMDTextBox2.Text += "\r\n" + "Pressure Simulation Activated....." + "\r\n"; //+ Emoji.Use.Open_File_Folder
                                    CMDTextBox2.ScrollToEnd();

                                    SendCSV();

                                    SoundPlayer player = new(binAppPath + "/Audio/SIMP.wav");
                                    player.Play();
                                    CMDTextBox1.Clear();
                                    CMDShortcut.SelectedIndex = -1;
                                    openHeatShieldSimulation = false;
                                }
                                else
                                {
                                    CMDTextBox2.Text += "\r\n" + "Please open the heat shield first!" + "\r\n"; //+ Emoji.Use.Warning
                                    CMDTextBox2.ScrollToEnd();
                                    CMDTextBox1.Text = "CMD,2032,BK,5";
                                    CMDShortcut.SelectedIndex = -1;
                                }
                            }
                        }
                        catch
                        {
                            return;
                        }
                    }
                    else if (int.TryParse(lineSplit, out _))
                    {
                        string cmd = "CMD,2032,SIMP," + lineSplit;

                        StartTimer(cmd);



                        CMDTextBox2.Text += "\r\n" + "PRESSURE " + lineSplit + " is being sended....." + "\r\n"; //+ Emoji.Use.Pencil 
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "SIMP" + lineSplit;
                        //SoundPlayer player = new(binAppPath + "/Audio/SIMP.wav");
                        //player.Play();
                        CMDTextBox1.Clear();
                        CMDShortcut.SelectedIndex = -1;
                    }
                    else
                    {
                        CMDTextBox2.Text += "\r\n" + "Please enter the correct SIMP command!" + "\r\n"; //+Emoji.Use.Loudspeaker
                        CMDTextBox2.ScrollToEnd();
                    }
                }
                else if (line == "CMD,2032,BCN,ON")
                {
                    try
                    {
                        string cmd = "CMD,2032,BCN,ON";
                        StartTimer(cmd);



                        CMDTextBox2.Text += "\r\n" + "Activating Audio Beacon......" + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "CXON";
                        //SoundPlayer player = new(binAppPath + "/Audio/PXON.wav");
                        //player.Play();
                        CMDTextBox1.Clear();
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,BCN,OFF")
                {
                    try
                    {
                        string cmd = "CMD,2032,BCN,OFF";
                        StartTimer(cmd);



                        CMDTextBox2.Text += "\r\n" + "Deactivating Audio Beacon......" + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = "CXON";
                        //SoundPlayer player = new(binAppPath + "/Audio/PXON.wav");
                        //player.Play();
                        CMDTextBox1.Clear();
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }

                else if (line.Contains("CMD,2032,BK"))
                {
                    try
                    {
                        string[] lineCountStr = line.Split(',');
                        int lineCount = lineCountStr.Length;
                        if (lineCount < 4)
                        {
                            CMDTextBox2.Text += "\r\n" + "Please set a value for BK command " + lineCount + "\r\n";
                            CMDTextBox2.ScrollToEnd();
                        }
                        else
                        {
                            string lineSplit = line.Split(',')[3];
                            if (int.TryParse(lineSplit, out _))
                            {
                                Regex regex = new Regex(@"^(?:[1-9]|[1-5][0-5]|55)$");
                                if (regex.IsMatch(lineSplit))
                                {
                                    Debug.WriteLine("Match found!");
                                    string cmd = "CMD,2032,BK," + lineSplit;

                                    _serialPort.Write(cmd + "\r");
                                    CMDTextBox2.Text += "\r\n" + "BK command with value " + lineSplit + " is sended!" + "\r\n";   //Emoji.Use.Construction +
                                    //SerialDataStatus.Content = "BK" + lineSplit;
                                    CMDTextBox2.ScrollToEnd();
                                    CMDTextBox1.Clear();
                                    if (line == "CMD,2032,BK,5")
                                    {
                                        if (countOpenHeatShield == 0)
                                        {
                                            CMDTextBox1.Text = "CMD,2032,BK,5";
                                            countOpenHeatShield += 5;
                                        }
                                        else if (countOpenHeatShield == 5)
                                        {
                                            CMDTextBox1.Text = "CMD,2032,BK,3";
                                            countOpenHeatShield += 5;
                                        }
                                    }
                                    else if (line == "CMD,2032,BK,3")
                                    {
                                        countOpenHeatShield += 3;
                                    }
                                    if ((string)SimulationStatus.Content == "ACTIVE" == false && countOpenHeatShield == 13)
                                    {
                                        openHeatShieldSimulation = true;
                                        CMDTextBox1.Text = "CMD,2032,SIMP,PRESSURE";
                                        countOpenHeatShield = 0;
                                    }
                                    openHeatShieldSimulation = true;
                                }
                                else
                                {
                                    Debug.WriteLine("Match not found.");
                                    CMDTextBox2.Text += "\r\n" + "BK value is from range 1-55" + "\r\n";         //Emoji.Use.Construction 
                                    CMDTextBox2.ScrollToEnd();
                                }
                            }
                            else
                            {
                                CMDTextBox2.Text += "\r\n" + "BK value is an integer!" + "\r\n";      //+ Emoji.Use.Construction 
                                CMDTextBox2.ScrollToEnd();
                            }


                        }
                    }
                    catch
                    {
                        return;
                    }
                }
                else if (line == "CMD,2032,GB,ON")
                {
                    try
                    {
                        string cmd = "CMD,2032,GB,ON";

                        StartTimer(cmd);



                        CMDTextBox2.Text += "\r\n" + "Gimbal Testing....." + "\r\n";   //Emoji.Use.Construction +
                        CMDTextBox2.ScrollToEnd();
                    }
                    catch
                    {
                        return;
                    }
                }

                else if (line.Contains("CMD,2032,"))
                {
                    string lineSplit = line.Split(',')[2];
                    try
                    {
                        string words;
                        String words_status;
                        if (lineSplit == "LCK")
                        {
                            words = "Lock";
                            words_status = "LCK_MECHANISM";

                        }
                        else if (lineSplit == "HS")
                        {
                            words = "Heat Shield";
                            words_status = "HS_RELEASED";
                        }
                        else if (lineSplit == "UPR")
                        {
                            words = "Uprighting";
                            words_status = "HS_MECHANISM";
                        }
                        else
                        {
                            return;
                        }
                        string cmd = "CMD,2032," + lineSplit;

                        StartTimer(cmd);


                        CMDTextBox2.Text += "\r\n" + " " + words + " Mechanism Activated" + "\r\n";
                        CMDTextBox2.ScrollToEnd();

                        //SerialDataStatus.Content = words_status;
                        //SoundPlayer player = new(binAppPath + "/Audio/MECHANISM.wav");
                        //player.Play();
                        CMDTextBox1.Clear();
                        CMDShortcut.SelectedIndex = -1;
                    }
                    catch
                    {
                        return;
                    }
                }


                else
                {
                    CMDTextBox2.Text += "\r\n" + "----+The Command You entered Is Not Recognized.+----" + "\r\n"; // + Emoji.Use.Information_Source 
                    CMDTextBox2.ScrollToEnd();

                    //SoundPlayer player = new(binAppPath + "/Audio/TRY.wav");
                    //player.Play();
                    CMDTextBox1.Clear();
                }
            }
        }

        private void StartTimer(string cmd)
        {
            _timerCommand = new System.Threading.Timer(SendCommand, cmd, 0, 50);
            _isTimerRunning = true;
        }

        private void StopTimer()
        {
            _isTimerRunning = false;
            _timerCommand.Dispose();
            _timerCommand = null;    //yumnacapekpusingdahlahmendingtiduraja
        }

        private void ImportImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "CSV Files (*.csv)|*.csv|TXT Files(*.txt)|*.txt|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                csvTextBox.Text = openFileDialog.FileName;
                this.Dispatcher.Invoke(() =>
                {
                    BindData(csvTextBox.Text);
                });

            }
        }

        private void MapStreetClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (GmapViewHome.MapProvider == null)
            {
                GmapViewHome.MapProvider = GoogleMapProvider.Instance;
            }
            else
            {
                if (GmapViewHome.MapProvider != GoogleMapProvider.Instance)
                {
                    GmapViewHome.MapProvider = GoogleMapProvider.Instance;
                }
            }
        }

        private void MapSatelliteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (GmapViewHome.MapProvider == null)
            {
                GmapViewHome.MapProvider = GoogleSatelliteMapProvider.Instance;
            }
            else
            {
                if (GmapViewHome.MapProvider != GoogleSatelliteMapProvider.Instance)
                {
                    GmapViewHome.MapProvider = GoogleSatelliteMapProvider.Instance;
                }
            }
        }

        private void MapTerrainClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (GmapViewHome.MapProvider == null)
            {
                GmapViewHome.MapProvider = GoogleTerrainMapProvider.Instance;
            }
            else
            {
                if (GmapViewHome.MapProvider != GoogleTerrainMapProvider.Instance)
                {
                    GmapViewHome.MapProvider = GoogleTerrainMapProvider.Instance;
                }
            }
        }

        private void MiniMapActivate(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((string)MiniMapBtn.Content == "Started")
            {
                //MiniMapStatus.Content = "Not Started";
                MiniMapBtn.Content = "Start";
            }
            else
            {
                //MiniMapStatus.Content = "Started";
                MiniMapBtn.Content = "Started";
                GmapView_Load();
                MiniMapBtn.IsEnabled = false;
            }
        }

        private void CMDSendButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_serialPort.IsOpen == true)
            {

                try
                {
                    CommandToCanSat();
                }
                catch
                {
                    return;
                }
            }
            else
            {
                try
                {
                    //SoundPlayer player = new(binAppPath + "/Audio/CONNECTTO.wav");
                    CMDTextBox2.Text += "\r\n" + "Connect To Serial Port First!" + "\r\n"; //+ Emoji.Use.Information_Source 
                    CMDTextBox1.Clear();
                }
                catch
                {
                    return;
                }
            }
        }

        private void TimerSimulation_Tick(object sender, EventArgs e)
        {
            if (Col4.Count == timerCSV)
            {
                CMDTextBox2.Text = " Simulation is over! \r\n";   //Emoji.Use.Robot_Face
                timerCSV = 0;
                timerSimulation.Stop();
                CMDTextBox1.Text = "CMD,2032,SIM,DISABLE";
                Debug.WriteLine("Timersimulation is stopped, is it? (if true it false) " + timerSimulation.IsEnabled);
            }
            else
            {
                try
                {
                    if (!_serialPort.IsOpen)
                    {
                        // Port is not open, so it is likely not connected
                        Debug.WriteLine("Serial port is not connected.");
                        CMDTextBox2.Text = " Simulation is over!"; //Emoji.Use.Robot_Face +
                        timerCSV = 0;
                        timerSimulation.Stop();
                        Debug.WriteLine("Timersimulation is stopped, is it? (if true it false) " + timerSimulation.IsEnabled);
                        //PortStatus.Content = "Disconnected From Serial Port";
                        TransmissionStatus.Text = "Disconnected";
                        //SerialDataStatus.Content = "Idle";
                        PortStatusPane.Background = System.Windows.Media.Brushes.Red;
                        ConnectPortBtn.Visibility = System.Windows.Visibility.Visible;
                        DisconnectPortBtn.Visibility = System.Windows.Visibility.Hidden;
                        CMDTextBox1.IsReadOnly = true;
                        ClearRefreshListPort();
                    }
                    else
                    {
                        Debug.WriteLine("Timersimulation is still running, is it? " + timerSimulation.IsEnabled + " " + timerCSV);
                        CMDTextBox2.Text = " Simulation data count: " + Col4.Count + " timerCsv: " + timerCSV;   //Emoji.Use.Robot_Face + 
                        string cmd = "CMD,2032,SIMP," + Col4[timerCSV] + "\r";

                        //Debug.WriteLine("Test outside " + timerCSV);
                        _serialPort.Write(cmd);
                        //SerialDataStatus.Content = "SIMP" + Col4[timerCSV];
                        SerialControlTextBox.Text += " CMD,2032,SIMP," + Col4[timerCSV] + "\n"; //Emoji.Use.Pencil +
                    }

                }
                catch (Exception)
                {
                    return;
                }
                Debug.WriteLine("Test outside " + timerCSV);
            }

            timerCSV++;
        }

        private void ThreeDModelActivate(object sender, System.Windows.RoutedEventArgs e)
        {
            if ((string)ThreeDModelBtn.Content == "Started")
            {
                model.Content = null;
                //ThreeDModelStatus.Content = "Not Started";
                ThreeDModelBtn.Content = "Start";
            }
            else
            {
                fileobj = System.AppDomain.CurrentDomain.BaseDirectory + "/Assets/3D/Probe_Stowed.obj";
                //ThreeDModelStatus.Content = "Started";
                ThreeDModelBtn.Content = "Started";
                HelixViewport3D_Load();
            }
        }

        private void SendCSV()
        {
            try
            {
                Col4.Clear();
                var filePath = openFileDialog.FileName;

                using StreamReader reader = new(filePath);
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    while (!reader.EndOfStream)
                    {
                        string data = reader.ReadLine();

                        if (!data.Contains(@"#"))
                        {
                            var values = data.Split(',');

                            if (!values.Any(s => s == ""))
                            {
                                string field = values[3];
                                Col4.Add(field);
                            }
                        }
                    }
                    timerSimulation.Start();
                    GC.Collect();
                }
            }
            catch
            {
                return;
            }
        }

        private void SendCommand(object state)
        {
            if (!_isTimerRunning)
            {
                return;
            }
            else if (_counterCommand < 1)
            {
                _serialPort.Write((string)state + "\r");
                Debug.WriteLine("Counter Test Baru: {0} {1}", _counterCommand, (string)state);
                _counterCommand++;
            }
            else
            {
                _isTimerRunning = false;
                _timerCommand.Change(Timeout.Infinite, Timeout.Infinite);
                StopTimer();
                _counterCommand = 0;
            }
        }

        private void counterCommandCMDText()
        {
            CMDTextBox2.Text = "Counter command: " + _counterCommand;
        }

        public void CMDTextBox_KeyPress(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (_serialPort.IsOpen == true)
            {

                try
                {
                    if (e.Key == Key.Return)
                    {
                        e.Handled = true;
                        CommandToCanSat();
                    }
                }
                catch
                {
                    return;
                }
            }
            else
            {
                try
                {
                    if (e.Key == Key.Return)
                    {
                        e.Handled = true;
                        //SoundPlayer player = new(binAppPath + "/Audio/CONNECTTO.wav");
                        CMDTextBox2.Text += "\r\n" + "Connect To Serial Port First!" + "\r\n"; //+ Emoji.Use.Information_Source
                        CMDTextBox1.Clear();
                    }
                }
                catch
                {
                    return;
                }
            }
        }

        private void CMDShortcutChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CMDShortcut.SelectedItem != null)
            {
                string command = CMDShortcut.SelectedItem.ToString();
                CMDShortcut.SelectedItem = null;
                command = command.Split(' ')[1];
                switch (command)
                {
                    case "CAL":
                        CMDTextBox1.Text = "CMD,2032,CAL";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "CXON":
                        CMDTextBox1.Text = "CMD,2032,CX,ON";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "CXOFF":
                        CMDTextBox1.Text = "CMD,2032,CX,OFF";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "ST":
                        CMDTextBox1.Text = "CMD,2032,ST,";
                        break;
                    case "SIMENABLE":
                        CMDTextBox1.Text = "CMD,2032,SIM,ENABLE";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "SIMDISABLE":
                        CMDTextBox1.Text = "CMD,2032,SIM,DISABLE";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "SIMACTIVATE":
                        CMDTextBox1.Text = "CMD,2032,SIM,ACTIVATE";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "SIMP":
                        CMDTextBox1.Text = "CMD,2032,SIMP,PRESSURE";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "BCNON":
                        CMDTextBox1.Text = "CMD,2032,BCN,ON";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "BCNOFF":
                        CMDTextBox1.Text = "CMD,2032,BCN,OFF";
                        //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                        break;
                    case "BK":
                        CMDTextBox1.Text = "CMD,2032,BK,1-55";
                        break;
                    //case "GCSRESET":
                    //    CMDTextBox1.Text = "CMD,GCS,RESET";
                    //    break;
                    //case "GCSTEST":
                    //    CMDTextBox1.Text = "CMD,GCS,TESTMODE";
                    //    break;
                    //case "CR":
                    //    CMDTextBox1.Text = "CMD,2032,CR";
                    //    //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    //    break;
                    //case "TC":
                    //    CMDTextBox1.Text = "CMD,2032,TC";
                    //    //CMDSendButton.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                    //    break;
                    default:
                        break;
                }


            }
        }

        private void AutoScrollActivate(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AutoScrollBtn.IsChecked == true)
            {
                auto_scroll = true;
            }
            else
            {
                auto_scroll = false;
            }
        }

        private void calibrateGCS()
        {

        }
    }
}


