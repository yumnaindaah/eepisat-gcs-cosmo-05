using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
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
using System.Windows.Markup;
using System.IO;
using System.Management;

namespace GCS_EEPISAT_05__COSMO_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


        public partial class MainWindow : System.Windows.Window
        {

        Microsoft.Win32.OpenFileDialog? openFileDialog;

        // battery
        static int jumlahDataRegLin;
        static float sumX, sumY, sumX2, sumXY;
        static float x, y, m, c;
        float hasilRegLin;
        //readonly int lineCommand = 0;

        public MainWindow()
            {
                InitializeComponent();
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
            private void ShutdownBtnClick(object sender, RoutedEventArgs e)
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
            //private void RestartBtnClick(object sender, System.Windows.RoutedEventArgs e)
            //{
            //    MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure to restart the GCS?", "", MessageBoxButton.OKCancel);
            //    switch (result)
            //    {
            //        case MessageBoxResult.OK:
            //            //SoundPlayer player = new(binAppPath + "/Audio/GCSRESTART.wav");
            //            //player.Play();
            //            //Thread.Sleep(3000);
            //            Process.Start(System.Windows.Application.ResourceAssembly.Location);
            //            System.Windows.Application.Current.Shutdown();
            //            break;
            //        case MessageBoxResult.Cancel:
            //            break;
            //    }
            //}
            private void RestartBtnClick(object sender, RoutedEventArgs e)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure to restart the application?", "", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    // Menutup aplikasi saat ini
                    Application.Current.Shutdown();

                    // Mendapatkan path aplikasi yang sedang berjalan
                    string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                    // Memulai aplikasi lagi
                    Process.Start(appPath);

                    // Mengakhiri proses saat ini
                    Environment.Exit(0);
                }
            }

            private void GetBatteryPercent()
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

                    BatteryPercentage.Width = Convert.ToDouble(estimatedChargeRemaining);
                    BatteryStatus.Content = Convert.ToString(estimatedChargeRemaining) + "%" + " ";
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
                final = Convert.ToString((int)hasilRegLin) + " Minutes Remaining";


                //if (y == 71582788)
                //{
                //    var converter = new System.Windows.Media.BrushConverter();
                //    var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#00FF00");
                //    BatteryPercentage.Background = brush;
                //    final = "On Charging";
                //}
                //else if (y < 30)
                //{
                //    var converter = new System.Windows.Media.BrushConverter();
                //    var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#FFFF00");
                //    BatteryPercentage.Background = brush;
                //}
                //else if (y < 10)
                //{
                //    var converter = new System.Windows.Media.BrushConverter();
                //    var brush = (System.Windows.Media.Brush)converter.ConvertFromString("#FF0000");
                //    BatteryPercentage.Background = brush;
                //}
                BatteryStatus.Content += final;
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
        }
    }

