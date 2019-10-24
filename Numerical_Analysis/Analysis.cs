using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Numerical_Analysis;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using Timer = System.Windows.Forms.Timer;
using System.Timers;
using Chart = Telerik.Charting.Chart;

namespace WindowsFormsApplication1
{
    public partial class Analysis : Telerik.WinControls.UI.RadForm
    {

        private int k = 1;
        private static int Elements = 0;
        private static int Counter = 0;
        private string Method = "";
        private static string Result = "";
        private string MM = "";
        private double prev = 0, h = 0, a = 0, b = 0;
        private Series A = new Series() {ChartType = SeriesChartType.Point};
        private Series B = new Series() { ChartType = SeriesChartType.Spline };
        private List<double> X = new List<double>();
        private List<double> Y = new List<double>();
        private string[] Spl;
        private bool finish = false, Done = false;
        private Thread t1;

        public Analysis()
        {
            InitializeComponent();
            chart1.Series.Add(A);
            chart1.Series.Add(B);
            this.radButton_clear.Enabled = false;
            radTextBox_x.Enabled = false;
            radTextBox3.Hide();
            radTextBox1_y.Enabled = false;
            radButton_next3.Enabled = false;
            radButton_Done.Enabled = false;
            radLabel4.Hide();
            radButton2.Hide();
            chart1.Hide();
            webBrowser1.Hide();
            radLabel_fun1.Hide();
            radButton1.Hide();
            radButton_result.Hide();
            radTextBox_get_x.Hide();
            radTextBox_result.Hide();
            radPanel1.Hide();
            radPanel2.Hide();
            radTextBox1.Enabled = false;
            radTextBox2.Enabled = false;
            radDropDownList1.Items.Add("General Method");
            radDropDownList1.Items.Add("Laghrange");
            radDropDownList1.Items.Add("Spline");
            radDropDownList1.Items.Add("Smallest Squares");
            radDropDownList1.Items.Add("Progressive Neutun Gregory");

            radDropDownList2.Items.Add("Rec integration");
            radDropDownList2.Items.Add("Trapezoid integration");
            radDropDownList2.Items.Add("Simpson integration");

            this.chart1.ChartAreas[0].AxisX.Crossing = 0;
            this.chart1.ChartAreas[0].AxisY.Crossing = 0;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
        }

        ///
        /// 
        ///  Interpolation
        /// 
        /// 
        public double General(double x = 0)
        {

            double[,] op_array = new double[X.Count, Y.Count];
            double[] sol_array = new double[X.Count]; 

            for (int i = 0; i < X.Count; i++)
                for (int j = 0; j < X.Count; j++) 
                    op_array[i, j] = System.Math.Pow(X[i], j);
            
            double W = determine(op_array);
            for (int i = 0; i < Y.Count; i++)
                sol_array[i] = (double)(determine(replace(op_array, i, Y)) / W);
            
            double res = this.Poly(sol_array, x);
            if (Result.ToString() == "0")
                MessageBox.Show("error input");
            
            return res;
        }

        private double Laghrange(double x = 0)
        {
            double res = 0;
            double L = 1;

            for (int j = 0; j < Elements; j++)
            {
                double base1 = 1, Up = 1;
                if (Y[j] != 0)
                {

                    if (Y[j] != 1)
                        Result += Math.Round(Y[j], 5).ToString() + "*";
                    Result += "(";
                    string temp = "";
                    for (int i = 0; i < Elements; i++)
                    {
                        if (i != j)
                        {
                            L *= (x - X[i]) / (X[j] - X[i]);
                            if (X[j] - X[i] == 0)
                            {
                                MessageBox.Show("Error ... X[" + j.ToString() + "] - X[" + i.ToString() +
                                                "] = 0. Try again.");
                                X.Clear();
                                Y.Clear();
                                Counter = 0;
                                Elements = 0;
                                prev = 0;

                            }
                            else
                            {
                                base1 *= (X[j] - X[i]);
                                Up *= (x - X[i]);
                                if (X[i] > 0)
                                    temp += "(x-" + Math.Round(X[i], 5).ToString() + ")";
                                else if (X[i] < 0)
                                    temp += "(x+" + Math.Round((-1 * X[i]), 5).ToString() + ")";
                                else
                                    temp += "x";
                            }
                        }
                    }
                    Result += temp + "/" + Math.Round(base1, 5).ToString() + ")";
                    res += (Y[j] * L);
                    L = 1;
                    if (j != Elements - 1 && Y[j + 1] != 0)
                    {
                        Result += "+";
                    }
                }
            }
            return res;
        }
       private double Neutun(double x)
        {
            List<List<double>> Delta = new List<List<double>>();
            double h = X[1] - X[0];
            double result = 0;
            double p = Math.Round((x - X[0]) / h, 6);
            for (int i = 0; i < Y.Count; i++)
            {
                Delta.Add(new List<double>());
                for (int j = 0; j < Y.Count - i; j++)
                {
                    Delta[i].Add(Math.Round(get_y_res(Y, i, j), 6));
                }
            }
            for (int i = 0; i < Delta.Count - 1; i++)
            {
                result += Math.Round(Delta[i][0] * Cp(p, i), 6);
            }
            return result;
        }

        private double SmallestSquares(double x = 0)
        {
            double res = 0, a0, a1;
            for (int i = 0; i < 2; i++)
            {
                double yi = 0, xi = 0, xi2 = 0, xiyi = 0;
                for (int j = 0; j < Elements; j++)
                {
                    yi += Y[j];
                    xi += X[j];
                    xi2 += (X[j]*X[j]);
                    xiyi += (X[j]*Y[j]);

                }

                a1 = (Elements*xiyi - yi*xi)/(xi2*Elements - xi*xi);
                a0 = (yi - a1*xi)/Elements;
                double[] a = new double[2];
                a[0] = a0;
                a[1] = a1;
                this.Poly(a);
                res = a0 + a1*x;

            }
            return res;

        }


        private double Spline(double x = 0)
        {
            double res = 0;
            Spl = new string[Elements + 2];
            for (int i = 0; i < Elements - 1; i++)
            {
                if (x <= X[i + 1] && X[i] <= x)
                    res = Y[i] + (double)((Y[i + 1] - Y[i]) / (X[i + 1] - X[i])) * (x - X[i]);
                Spl[i] = Math.Round(Y[i], 5).ToString() + "+" +
                         Math.Round((Y[i + 1] - Y[i] / X[i + 1] - X[i]), 5).ToString() + "*(x-" + X[i].ToString()+")";
            }
            return res;
        }



        ///
        /// 
        ///  Interpolation
        /// 
        /// 

        ///
        /// 
        ///  Intergration
        /// 
        /// 
        private double rec_integration()
        {
            int n = Y.Count - 1 ;
            double res = (b - a);
            res /= n;
            double sum = 0;
            for (int i = 0; i < n ; i++)
                sum += Y[i];


                return res*sum;
        }

        private double trapezoid_integration()
        {
            int n = Y.Count - 1;
            double res = (b - a);
            res /= 2;

            double sum = Y[0]+Y[n];
            for (int i = 1; i < n; i++)
                sum += (2*Y[i]);
     
            return sum*res;
        }

        private double simpson_integration()
        {
            int n = Y.Count - 1;
            double res = (b - a);
            res /= n*3;
            double sum = Y[0] + Y[n];

            for (int i = 1; i < n; i++)
            {
                if (i%2 != 0) sum += (4*Y[i]);
                else sum += (2*Y[i]);
            }

            return res*sum;
        }

        ///
        /// 
        ///  Intergration
        /// 
        /// 
     
        ///
        /// 
        /// 
        /// Helper Functions  
        /// 
        ///
        static double[,] CreateSmallerMatrix(double[,] input, int i, int j)
        {
            int order = int.Parse(System.Math.Sqrt(input.Length).ToString());
            double[,] output = new double[order - 1, order - 1];
            int x = 0, y = 0;
            for (int m = 0; m < order; m++, x++)
            {
                if (m != i)
                {
                    y = 0;
                    for (int n = 0; n < order; n++)
                    {
                        if (n != j)
                        {
                            output[x, y] = input[m, n];
                            y++;
                        }
                    }
                }
                else
                {
                    x--;
                }
            }
            return output;
        }
        static int SignOfElement(int i, int j)
        {
            if ((i + j) % 2 == 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
        public static double determine(double[,] input) //Determine of the operation matrix 
        {
            int order = int.Parse(System.Math.Sqrt(input.Length).ToString());
            if (order > 2)
            {
                double value = 0;
                for (int j = 0; j < order; j++)
                {
                    double[,] Temp = CreateSmallerMatrix(input, 0, j);
                    value = value + input[0, j] * (SignOfElement(0, j) * determine(Temp));
                }
                return value;
            }
            else if (order == 2)
            {
                return ((input[0, 0] * input[1, 1]) - (input[1, 0] * input[0, 1]));
            }
            else
            {
                return (input[0, 0]);
            }
        }
        private static double[,] replace(double[,] op_array, int col_num, List<double> y_list)
        // replace each column with constants of Inter linear equations
        {
            double[,] temp_array = new double[y_list.Count, y_list.Count];
            Array.Copy(op_array, temp_array, op_array.Length);
            for (int i = 0; i < y_list.Count; i++)
            {
                temp_array[i, col_num] = y_list[i];
            }
            return temp_array;
        }

        private double Poly(double[] sol, double x = 0)
        {
            double res = 0, temp = 1;
            Result = "0";
            for (int i = 0; i < sol.Length; i++)
            {
                if (sol[i] != 0)
                {
                    if (i == 0)
                    {
                        res += sol[i];
                        Result = Math.Round(sol[i], 5).ToString();
                    }
                    else if (i == 1)
                    {
                        res += sol[i] * x;
                        temp = x;
                        if (sol[i] == 1)
                            Result += "x";
                        else if (sol[i] == -1)
                            Result += "-x";
                        else if (sol[i] > 0)
                            Result += "+" + Math.Round(sol[i], 5).ToString() + "x";
                        else if (sol[i] < 0)
                            Result += "-" + Math.Round((-1 * sol[i]), 5).ToString() + "x";
                    }
                    else
                    {
                        temp *= x;
                        res += sol[i] * temp;
                        if (sol[i] < 0)
                        {
                            Result += "-";
                            if (sol[i] == -1)
                                Result += "x^" + i.ToString();
                            else
                                Result += Math.Round((-1 * sol[i]), 5).ToString() + "x^" + i.ToString();
                        }
                        else if (sol[i] > 0)
                        {
                            Result += "+";
                            if (sol[i] == 1)
                                Result += "x^" + i.ToString();
                            else
                                Result += Math.Round(sol[i], 5).ToString() + "x^" + i.ToString();
                        }
                    }
                }
            }
            return res;
        }
       
        private double fact(int n)
        {
            if (n <= 1) return 1;
            int result = 1;
            for (int i = 2; i <= n; i++)
            {
                result = result * i;
            }
            return result;
        }

        private double C(int n, int r)
        {
            return (fact(n))/(fact(r)*fact(n - r));
        } 

        private double Cp(double p, int k)
        {
            double res = 1;
            if (k == 0) return 1;
            else if (k == 1)
            {
                return p;
            }
            else
            {
                for (int i = 1; i <= k; i++)
                {
                    res *= p - (i - 1);
                }
                return res/fact(k);
            }
        }

        private double get_y_res(List<double> y_list, int degree, int index) //getting delta(y) ^n
        {
            double res = 0;
            for (int i = 0; i <= degree; i++)
            {
                res += (Math.Pow(-1, i))*C(degree, i)*(y_list[index + degree - i]);
            }
            return res;
        }     
        /// 
        /// 
        /// Helper Functions  
        /// 
        ///
    
        
        ///
        /// 
        /// Form Functions 
        ///
        /// 
        private void radButton_next1_Click(object sender, EventArgs e)
        {
            if (radDropDownList1.Text == "Choose a Method" && radDropDownList2.Text == "Choose a Method")
                MessageBox.Show("Choose a Method Please !");
            else
            {

                radTextBox1.Enabled = true;
                radTextBox2.Enabled = true;
                radTextBox_x.Enabled = true;
                radTextBox1_y.Enabled = true;
                radButton_next3.Enabled = true;
                radButton_Done.Enabled = true;
                radButton_clear.Enabled = false;
                radGridView1.Enabled = true;
                radDropDownList1.Enabled = false;
                if (radDropDownList1.Text == "Laghrange")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "L";
                    radButton_next1.Enabled = false;
                }
                else if (radDropDownList1.Text == "Spline")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "S";
                    radButton_next1.Enabled = false;
                }
                else if (radDropDownList1.Text == "Smallest Squares")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "SQ";
                    radButton_next1.Enabled = false;
                }

                else if (radDropDownList1.Text == "General Method")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "G";
                    radButton_next1.Enabled = false;
                }
                else if (radDropDownList1.Text == "Progressive Neutun Gregory")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "N";
                    radButton_next1.Enabled = false;
                }
                else if (radDropDownList2.Text == "Rec integration")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "RI";
                    radButton_next1.Enabled = false;
                }
                else if (radDropDownList2.Text == "Trapezoid integration")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "TI";
                    radButton_next1.Enabled = false;
                }
                 else if (radDropDownList2.Text == "Simpson integration")
                {
                    this.radButton_clear.Enabled = true;
                    this.radGridView1.Show();
                    Method = "SI";
                    radButton_next1.Enabled = false;
                }
            }
        }

        private void radButton_clear_Click(object sender, EventArgs e)
        {
            radDropDownList1.Text = "Choose a Method";
            radDropDownList2.Text = "Choose a Method";
            radPanel1.Text = "";
            radRadioButton1.IsChecked = false;
            radRadioButton2.IsChecked = false;
            k = 1;
            radTextBox3.Clear();
            radLabel4.Hide();
            radTextBox3.Hide();
            radTextBox1.Enabled = false;
            radTextBox2.Enabled = false;
            radRadioButton1.Show();
            radRadioButton2.Show();
            radButton3.Show();
            radPanel2.Hide();
            radPanel1.Hide();
            chart1.Hide();
            webBrowser1.Hide();
            radLabel_fun1.Hide();
            radButton1.Hide();
            radButton_result.Hide();
            radTextBox_get_x.Hide();
            radTextBox_result.Hide();
            radTextBox_x.Enabled = false;
            radTextBox1_y.Enabled = false;
            radButton_next3.Enabled = false;
            radButton_Done.Enabled = false;
            radDropDownList1.Enabled = true;
            this.radButton_clear.Enabled = false;
            radGridView1.Enabled = false;
            radButton2.Hide();

            if (finish)
            {
                finish = false;
            }
            radGridView1.Rows.Clear();
            radGridView1.ClearSelection();
            foreach (var series in chart1.Series)
                series.Points.Clear();
            radButton_Done.Enabled = false;
            X.Clear();
            Y.Clear();
            radLabel_x.Text = "X0";
            radLabel_y.Text = "Y0";
            radTextBox_result.Clear();
            radTextBox_get_x.Clear();
            radTextBox_x.Clear();
            radTextBox1_y.Clear();
            Method = "";
            Result = "";
            Counter = 0;
            Elements = 0;
            prev = 0;
            h = 0;
            radButton_next1.Enabled = true;
        }


        private void radButton_next3_Click(object sender, EventArgs e)
        {
            //  radGridView1.Rows[k].Cells[0].Value = radTextBox_x.Text;
            // radGridView1.Rows[k++].Cells[1].Value = radTextBox1_y.Text;
            if (MM == "DIF")
            {
                if (radTextBox_x.Text == "" || radTextBox1_y.Text == "")
                {
                    MessageBox.Show("error input.....Please Try Again");
                    radTextBox_x.Clear();
                    radTextBox1_y.Clear();
                }
                else
                {
                    radButton_Done.Enabled = true;
                    double x1 = Convert.ToDouble(radTextBox_x.Text.ToString());
                    if (Method == "N" && Counter > 1 && (x1 - prev) != h)
                    {
                        MessageBox.Show(
                            "Input error ... Neutun gregory is used only with a constant difference between x. Try again.");
                        radTextBox_x.Clear();
                        radTextBox1_y.Clear();
                    }
                    else if (Method == "S" && Counter > 1 && x1 < prev)
                    {
                        MessageBox.Show(
                            "Input error ... In Spline every x should be bigger than the previous x. Try again.");
                        radTextBox_x.Clear();
                        radTextBox1_y.Clear();


                    }
                    else
                    {
                        radGridView1.Rows.Add(radTextBox_x.Text, radTextBox1_y.Text);
                        if (Counter == 1 && Method == "N")
                            h = x1 - prev;
                        prev = x1;
                        double y1 = Convert.ToDouble(radTextBox1_y.Text.ToString());
                        Counter++;
                        X.Add(x1);
                        Y.Add(y1);
                        radTextBox_x.Clear();
                        radTextBox1_y.Clear();
                        radLabel_x.Text = "X" + Counter.ToString();
                        radLabel_y.Text = "Y" + Counter.ToString();

                    }

                }
            }
            else if (MM == "INT")
            {
                if (radTextBox_x.Text == "" || radTextBox1_y.Text == "")
                {
                    MessageBox.Show("error input.....Please Try Again");
                    radTextBox_x.Clear();
                    radTextBox1_y.Clear();
                }
                else
                {
                    radGridView1.Rows.Add(radTextBox_x.Text, radTextBox1_y.Text);
                    double x1 = Convert.ToDouble(radTextBox_x.Text.ToString());
                    double y1 = Convert.ToDouble(radTextBox1_y.Text.ToString());
                    X.Add(x1);
                    Y.Add(y1);
                    radTextBox_x.Clear();
                    radTextBox1_y.Clear();
                }
            }
        }

        public static bool IsOnline()
        {
            var pinger = new Ping();

            try
            {
                return pinger.Send("www.google.com").Status == IPStatus.Success;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (PingException)
            {
                return false;
            }
        }

        private void coolfunction(int i)
        {
            if (IsOnline())
            {
                webBrowser1.Navigate("http://latex.codecogs.com/gif.latex?%20" + radPanel1.Text);
                Done = true;
            }
            else
            {
                Done = false;
                webBrowser1.DocumentText =
                    "<html><body>No Internet Connection " + i +
                    " <br/></body></html>";
            }
        }

        private double det(double i)
        {
            if (Method == "L")
            {
                return Laghrange(i);
            }
            else if (Method == "S")
            {
                return Spline(i);

            }
            else if (Method == "SQ")
            {
                return SmallestSquares(i);
            }
            else if (Method == "G")
            {
                return General(i);

            }
            else if (Method == "N")
            {
                return Neutun(i);
            }
            return 0;
        }

        private void cooldraw()
        {
            double minx = X.Min();
            double maxx = X.Max();
            double max = Math.Max(Math.Abs(minx), Math.Abs(maxx));
            if (MM=="DIF")
            {
                for (double i = -2*max; i <= 2*max + 0.1; i += 0.01)
                {
                    chart1.Series[0].Points.Add(new DataPoint(i, det(i)));
                }
                chart1.Invoke(new Action(Refresh));
            }
            else if (MM=="INT")
            {
                for(int i=0;i<X.Count;i++)
                    chart1.Series[2].Points.Add(new DataPoint(X[i], Y[i]));
            }
        }


        private void radButton_Done_Click_1(object sender, EventArgs e)
        {
            if (X.Count == 0)
            {
                MessageBox.Show("insert a new point please ! ");
            }
            else
            {
                if (MM == "DIF")
                {
                    chart1.Size = new Size(700, 267); 
                    radTextBox3.Hide();
                    radLabel4.Hide();
                    radPanel1.Show();
                    chart1.Show();
                    webBrowser1.Show();
                    radLabel_fun1.Show();
                    radButton1.Show();
                    radButton_result.Show();
                    radTextBox_get_x.Show();
                    radTextBox_result.Show();
                    radButton2.Show();
                    radTextBox_x.Enabled = false;
                    radTextBox1_y.Enabled = false;
                    radButton_next3.Enabled = false;
                    radButton_Done.Enabled = false;
                    finish = true;
                    radButton_clear.Enabled = true;
                    Elements = Counter;
                    if (Method == "L")
                    {
                        this.Laghrange();
                        radLabel_fun1.Text = "The Laghrange Function is :";
                        radPanel1.Text = Result;

                    }
                    else if (Method == "SQ")
                    {
                        this.SmallestSquares();
                        radLabel_fun1.Text = "The Line Function is :";
                        radPanel1.Text = Result;
                    }
                    else if (Method == "G")
                    {
                        this.General();
                        radLabel_fun1.Text = "The Line Function is :";
                        radPanel1.Text = Result;
                    }
                    else if (Method == "N")
                    {
                        chart1.Size = new Size(700, 367);
                        chart1.Show();
                        radPanel1.Hide();
                        radButton2.Hide();
                    }
                    else if (Method == "S")
                    {
                        chart1.Size = new Size(700, 367);
                        chart1.Show();
                        radPanel1.Hide();
                        radButton2.Hide();
                    }
                    Done = true;
                }
                else if (MM == "INT")
                {
                    if (radTextBox1.Text == "" || radTextBox2.Text == "")
                        MessageBox.Show("insert a & b please ! ");
                    else
                    {
                        chart1.Size = new Size(700, 367);
                        radLabel4.Show();
                        chart1.Show();
                        radTextBox3.Show();
                        radTextBox_x.Enabled = false;
                        radTextBox1_y.Enabled = false;
                        radButton_next3.Enabled = false;
                        radButton_Done.Enabled = false;
                        finish = true;
                        radButton_clear.Enabled = true;
                        Elements = Counter;

                        if (Method=="RI" )
                        radTextBox3.Text = rec_integration().ToString();
                        else
                        if (Method == "TI")
                         radTextBox3.Text = trapezoid_integration().ToString();
                        else if (Method == "SI")
                        {
                            if (X.Count%2 == 0)
                            {
                                chart1.Hide();
                                radLabel4.Hide();
                                radTextBox3.Hide();
                                MessageBox.Show("error!! the number of points is even ! insert another point");
                            }
                            else
                                radTextBox3.Text = simpson_integration().ToString();
                        }
                    }
                }
                cooldraw();
                if (((MM == "DIF")&&(Method!="N"))||((MM == "DIF")&&(Method!="S")))
                {
                    t1 = new Thread(() => coolfunction(k++));
                    t1.Start();
                }
            }
          
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            t1 = new Thread(() => coolfunction(k++));
            t1.Start();

        }


        //private async void delay()
        //{
        //    await Task.Delay(100);
        //    coolfunction();

        //}
        //private static Timer timer1;
        //private  void repeat()
        //{
        //    timer1 = new Timer();
        //    timer1.Interval = 1000;
        //    timer1.Enabled = true;
        //    timer1.Tick += new System.EventHandler(OnTimerEvent);
        //}
        //public  void OnTimerEvent(object source, EventArgs e)
        //{
        //    coolfunction();
        //}


        private void radButton_result_Click(object sender, EventArgs e)
        {

            if (radTextBox_get_x.Text.Length == 0)
            {
                MessageBox.Show("Error Input ....Please try again");
                radTextBox_get_x.Clear();
            }
            else
            {
                bool ch = false;
                double x = Convert.ToDouble(radTextBox_get_x.Text);
                if (Method == "L")
                {
                    radTextBox_result.Text = Laghrange(x).ToString();
                    DataPoint r = new DataPoint(x, Laghrange(x));
                    chart1.Series[0].Points.Add(r);
                    r.MarkerSize = 5;
                    r.Color = Color.DarkGreen;
                }
                else if (Method == "S")
                {
                    radTextBox_result.Text = Laghrange(x).ToString();
                    DataPoint r = new DataPoint(x, Spline(x));
                    chart1.Series[0].Points.Add(r);
                    r.MarkerSize = 5;
                    r.Color = Color.DarkGreen;

                }
                else if (Method == "SQ")
                {

                    radTextBox_result.Text = SmallestSquares(x).ToString();
                    DataPoint r = new DataPoint(x, SmallestSquares(x));
                    chart1.Series[0].Points.Add(r);
                    r.MarkerSize = 5;
                    r.Color = Color.DarkGreen;
                }
                else if (Method == "G")
                {
                    radTextBox_result.Text = General(x).ToString();
                    DataPoint r = new DataPoint(x, General(x));
                    chart1.Series[0].Points.Add(r);
                    r.MarkerSize = 5;
                    r.Color = Color.DarkGreen;
                }
                else if (Method == "N")
                {
                    radTextBox_result.Text = Neutun(x).ToString();
                    DataPoint r = new DataPoint(x, Neutun(x));
                    chart1.Series[0].Points.Add(r);
                    r.MarkerSize = 5;
                    r.Color = Color.DarkGreen;
                }

                double y = Convert.ToDouble(radTextBox_result.Text);
                for (int i = 0; i < X.Count; i++)
                    if (x == X[i])
                    {
                        ch = true;
                        break;
                    }
                if (!ch)
                {
                       X.Add(x);
                       Y.Add(y);
                       radGridView1.Rows.Add(x.ToString(), y.ToString());
                }
            }
              
            

        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            if (!radRadioButton1.IsChecked && !radRadioButton2.IsChecked)
                MessageBox.Show("Choose a Method Please !");
            else
            {
                radRadioButton1.Hide();
                radRadioButton2.Hide();
                radButton3.Hide();
                if (radRadioButton1.IsChecked)
                {
                    MM = "DIF";
                    radDropDownList2.Hide();
                    radDropDownList1.Show();
                    radLabel1.Hide();
                    radLabel2.Hide();
                    radTextBox1.Hide();
                    radTextBox2.Hide();
                    radButton4.Hide();
                    radPanel2.Show();

                }
                else if (radRadioButton2.IsChecked)
                {
                    MM = "INT";
                    radDropDownList2.Show();
                    radDropDownList1.Hide();
                    radLabel1.Show();
                    radLabel2.Show();
                    radTextBox1.Show();
                    radTextBox2.Show();
                    radButton4.Show();
                    radTextBox2.Clear();
                    radTextBox1.Clear();
                    radPanel2.Show();

                }

            }
        }
        private void radButton4_Click_1(object sender, EventArgs e)
        {
            if (radTextBox1.Text == "" || radTextBox2.Text == "")
            {
                MessageBox.Show("error input.....Please Try Again");
                radTextBox1.Clear();
                radTextBox2.Clear();
            }
            else
            {
                a = Convert.ToDouble(radTextBox1.Text);
                b = Convert.ToDouble(radTextBox2.Text);
                radTextBox2.Enabled = false;
                radTextBox1.Enabled = false;
            }

        }

        /// 
        /// 
        /// Form Functions 
        ///
        /// 
        private void Analysis_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void radPanel_result_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radLabel2_Click(object sender, EventArgs e)
        {

        }

        private void radPanel_fun_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radLabel_fun1_Click(object sender, EventArgs e)
        {

        }

        private void radLabel_x_Click(object sender, EventArgs e)
        {

        }

        private void radGridView1_Click(object sender, EventArgs e)
        {

        }

        private void radDropDownList1_SelectedIndexChanged(object sender,
            Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {

        }

        private void radRadioButton_S_ToggleStateChanged(object sender,
            Telerik.WinControls.UI.StateChangedEventArgs args)
        {

        }
        private void radButton4_Click(object sender, EventArgs e)
        {

        }



        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void radButton5_Click(object sender, EventArgs e)
        {
            About a =new About();
            a.Show();
        }

        private void radTextBox_result_TextChanged(object sender, EventArgs e)
        {

        }
    }
}


