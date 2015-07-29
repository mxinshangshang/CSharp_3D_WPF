using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace Simulation
{
    
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        string name;
        string ip;
        private Thread Listen;
        private TcpListener tcpListener;
        private static string message = "";

        // Three observable data sources. Observable data source contains
        // inside ObservableCollection. Modification of collection instantly modify
        // visual representation of graph. 
        ObservableDataSource<Point> source1 = null;
        ObservableDataSource<Point> source2 = null;
        ObservableDataSource<Point> source3 = null;

        public Window1()
        {
            InitializeComponent();
        }

        private void Simulation()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            // load spim-generated data from embedded resource file
            const string spimDataName = "Simulation.Repressilator.txt";
            using (Stream spimStream = executingAssembly.GetManifestResourceStream(spimDataName))
            {
                using (StreamReader r = new StreamReader(spimStream))
                {
                    string line = r.ReadLine();
                    while (!r.EndOfStream)
                    {
                        line = r.ReadLine();
                        string[] values = line.Split(',');

                        double x = Double.Parse(values[0], culture);
                        double y1 = Double.Parse(values[1], culture);
                        double y2 = Double.Parse(values[2], culture);
                        double y3 = Double.Parse(values[3], culture);

                        Point p1 = new Point(x, y1);
                        Point p2 = new Point(x, y2);
                        Point p3 = new Point(x, y3);

                        source1.AppendAsync(Dispatcher, p1);
                        source2.AppendAsync(Dispatcher, p2);
                        source3.AppendAsync(Dispatcher, p3);

                        Thread.Sleep(10); // Long-long time for computations...
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create first source
            source1 = new ObservableDataSource<Point>();
            // Set identity mapping of point in collection to point on plot
            source1.SetXYMapping(p => p);

            // Create second source
            source2 = new ObservableDataSource<Point>();
            // Set identity mapping of point in collection to point on plot
            source2.SetXYMapping(p => p);

            // Create third source
            source3 = new ObservableDataSource<Point>();
            // Set identity mapping of point in collection to point on plot
            source3.SetXYMapping(p => p);

            // Add all three graphs. Colors are not specified and chosen random
            plotter.AddLineGraph(source1, 2, "Data row 1");
            plotter.AddLineGraph(source2, 2, "Data row 2");
            plotter.AddLineGraph(source3, 2, "Data row 3");

            // Start computation process in second thread
            //Thread simThread = new Thread(new ThreadStart(Simulation));
            //simThread.IsBackground = true;
            //simThread.Start();
        }

        private void StartListen()
        {
            string line;
            CultureInfo culture = CultureInfo.InvariantCulture;
            //SetTextCallback settextbox = new SetTextCallback(SetText);
            byte[] buffer = new byte[8192];
            message = "";
            tcpListener = new TcpListener(IPAddress.Any, 8500);//tcpListener = new TcpListener(ipLocalEndPoint);//
            tcpListener.Start();
            while (true)
            {
                TcpClient tcpclient = tcpListener.AcceptTcpClient();
                NetworkStream streamToClient = tcpclient.GetStream();
                int bytesRead = streamToClient.Read(buffer, 0, 8192);
                message = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                line = message;

                this.textBox3.Text += Environment.NewLine + line + "\r\n";//"\r\n";// "\r\n" + text;
                this.textBox3.SelectionStart = this.textBox1.Text.Length;

                string[] values = line.Split(',');

                double x = Double.Parse(values[0], culture);
                double y1 = Double.Parse(values[1], culture);
                double y2 = Double.Parse(values[2], culture);
                double y3 = Double.Parse(values[3], culture);

                Point p1 = new Point(x, y1);
                Point p2 = new Point(x, y2);
                Point p3 = new Point(x, y3);

                source1.AppendAsync(Dispatcher, p1);
                source2.AppendAsync(Dispatcher, p2);
                source3.AppendAsync(Dispatcher, p3);

                Thread.Sleep(10); // Long-long time for computations...
            }
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Listen = new Thread(new ThreadStart(this.StartListen));
            Listen.IsBackground = true;
            Listen.Start();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                // load spim-generated data from embedded resource file
                const string spimDataName = "Simulation.Repressilator.txt";
                using (Stream spimStream = executingAssembly.GetManifestResourceStream(spimDataName))
                {
                    using (StreamReader r = new StreamReader(spimStream))
                    {
                        string line = r.ReadLine();
                        while (!r.EndOfStream)
                        {
                            TcpClient client = new TcpClient();
                            IPAddress ip1 = IPAddress.Parse(textBox1.Text.Trim());//{ 111, 186, 100, 46 }
                            client.Connect(ip1, 8500);
                            Stream streamToServer = client.GetStream();	// 获取连接至远程的流
                            line = r.ReadLine();
                            byte[] buffer = Encoding.Unicode.GetBytes(line);
                            streamToServer.Write(buffer, 0, buffer.Length);
                            streamToServer.Flush();
                            streamToServer.Close();
                            client.Close();
                            Thread.Sleep(10); // Long-long time for computations...
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
