using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        double lymdaStart = 2;
        double lymdaEnd = 8;
        double lymdaDelta = 0.5;
        double muStart = 0.2;
        double muEnd = 1.4;
        double muDelta = 0.1;
        private Dictionary<double, double> points;

        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "2";
            textBox2.Text = "1";
            textBox3.Text = "1.2";
            textBox4.Text = "5";
            textBox5.Text = "5";
        }

        private void MakeInvestigation(
        double lymdaStart,
        double lymdaEnd,
        double muStart,
        double muEnd,
        ref double kohren,
        ref double fisher)
        {
            Investigation investigation = new Investigation(lymdaStart, lymdaEnd, muStart, muEnd);

            kohren = investigation.GetKohrenCriterion();
            fisher = investigation.GetFisherCriterion();

            gr(points, investigation.regArray);

        }

        private void btnStartSimulation_Click(object sender, EventArgs e)
        {
            try
            {
                lymdaStart = double.Parse(textBox1.Text) * 0.5;
                lymdaEnd = double.Parse(textBox1.Text) * 1.5;
                lymdaDelta = double.Parse(textBox1.Text) / 10;
                muStart = double.Parse(textBox3.Text) * 0.5;
                muEnd = double.Parse(textBox3.Text) * 1.5;
                muDelta = double.Parse(textBox3.Text) / 10;
            }
            catch (Exception ex)
            {

            }
            Smo qms;
            try
            {
                qms = new Smo(
                    double.Parse(textBox1.Text), 
                    double.Parse(textBox4.Text),
                    double.Parse(textBox5.Text),
                    double.Parse(textBox6.Text), 
                    double.Parse(textBox8.Text), 
                    double.Parse(textBox2.Text), 
                    double.Parse(textBox3.Text), 
                    double.Parse(textBox7.Text),
                    1);
            }
            catch (Exception ex)
            {
                qms = new Smo();
            }
            qms.StartSimulation(5000);
            int n0 = qms.GetRejectedRequests();
            int n = qms.GetServicedRequests();
            double maxTime = qms.GetMaxTimeInStorageDevice();
            double averageTime = qms.GetAverageTimeInStorageDevice();
            double omission = qms.GetAbsoluteOmissionAbility();
            StringBuilder res = qms.result;
            WithdrawResult(n, n0, averageTime, maxTime, omission, res);
            BuildGraphic();

            double kohren = double.MaxValue;
            double fisher = double.MaxValue;

            while (fisher > 6.3901 || kohren > 0.6287)
            {
                MakeInvestigation(lymdaStart, lymdaEnd, muStart, muEnd, ref kohren, ref fisher);

                if (kohren > 0.6287)
                {
                    lymdaStart += lymdaDelta;
                    muEnd -= muDelta;
                }
                else
                {
                    lymdaEnd -= lymdaDelta;
                    muStart += muDelta;
                }
            }

            label29.Text = Math.Round(kohren, 4).ToString();
            label28.Text = Math.Round(fisher, 4).ToString();
            label25.Text = "0.6287"; //критерий Кохрена
            label27.Text = "6.3901"; //критерий Фишера
            if (Math.Round(kohren, 4) < 0.6287)
            {
                label31.Text = "пройдена";
            }
            else
            {
                label31.Text = "непройдена";
            }
            if (Math.Round(fisher, 4) < 6.3901)
            {
                label30.Text = "пройдена";
            }
            else
            {
                label30.Text = "непройдена";
            }

        }
        private void BuildGraphic()
        {
            graphic.ChartAreas[0].AxisY.Minimum = 0;
            graphic.ChartAreas[0].AxisY.Maximum = 100;
            graphic.ChartAreas[0].AxisY.Title = "Вероятность отклонения заявки";
            graphic.ChartAreas[0].AxisX.Title = "Интенсивность входного потока";
            this.graphic.Series.Clear();
            Series series2 = this.graphic.Series.Add("Pотк(lymda)");
            series2.ChartType = SeriesChartType.Spline;
            graphic.Series[0].BorderWidth = 2;

            int simulationTime = 10000;
            List<double> rejectedInPercent = new List<double>();
            List<double> parameters = new List<double>();
            int count = 0;
            points = new Dictionary<double, double>();
            for (double i = lymdaStart; i < lymdaEnd; i += lymdaDelta)
            {
                Smo qms = new Smo() { Lymda = i };
                qms.StartSimulation(simulationTime);
                rejectedInPercent.Add((double)qms.GetRejectedRequests() / (double)(qms.GetServicedRequests() + qms.GetRejectedRequests()) * 100);
                parameters.Add(i);
                points.Add(i, rejectedInPercent[count]);
                count++;
            }
            this.graphic.Series["Pотк(lymda)"].Points.DataBindXY(parameters, rejectedInPercent);
            parameters.Clear();
            rejectedInPercent.Clear();
            this.chart1.Series.Clear();
            chart1.ChartAreas[0].AxisY.Title = "Вероятность отклонения заявки";
            chart1.ChartAreas[0].AxisX.Title = "Интенсивность потока обслуживания";
            Series series1 = this.chart1.Series.Add("Pотк(m2)");
            series1.ChartType = SeriesChartType.Spline;
            chart1.Series[0].BorderWidth = 2;
            
            for (double i = muStart; i < muEnd; i += muDelta)
            {
                Smo qms = new Smo() { MuPhaseOne = i, MuPhaseTwoOne = i };
                qms.StartSimulation(simulationTime);
                double p = (double)qms.GetRejectedRequests() / (double)(qms.GetServicedRequests() + qms.GetRejectedRequests()) * 100;
                rejectedInPercent.Add(p);
                parameters.Add(i);
            }
            this.chart1.Series["Pотк(m2)"].Points.DataBindXY(parameters, rejectedInPercent);

        }
        private void WithdrawResult(double successRequestsCount, double rejectedRequestsCount, double averageTime, double maxTime, double omission, StringBuilder res)
        {
            double p = (double)rejectedRequestsCount / (double)(rejectedRequestsCount + successRequestsCount) * 100;
            lblAllRequests.Text = (successRequestsCount + rejectedRequestsCount).ToString();
            lblRejectedRequests.Text = rejectedRequestsCount.ToString();
            lblRejectedInPercent.Text = Math.Round(p, 2).ToString() + "%";
            lblAverageTime.Text = Math.Round(averageTime, 3).ToString();
            lblMaxTime.Text = Math.Round(maxTime, 3).ToString();
            label26.Text = Math.Round(omission, 0).ToString();
            lblAbsoluteOmission.Text = successRequestsCount.ToString();

            FileStream fileStr = null;
            try
            {
                fileStr = new FileStream("results.txt", FileMode.Create);
                using (StreamWriter sw = new StreamWriter(fileStr))
                {
                    sw.WriteLine(res.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (fileStr != null)
                    fileStr.Dispose();
            }
        }
        public void gr(Dictionary<double, double> pointsImitModel, double[] regArray)
        {
            this.chart2.Series.Clear();
            //this.chart2.Titles.Add("Cовмещенныеграфики");
            chart2.ChartAreas[0].AxisY.Minimum = 0;
            chart2.ChartAreas[0].AxisY.Maximum = 100;
            Series series2 = this.chart2.Series.Add("Potk \n(имитационная модель)");
            Series series1 = this.chart2.Series.Add("Potk \n(планирование эксперимента)");
            series1.ChartType = SeriesChartType.Spline;
            series2.ChartType = SeriesChartType.Spline;
            chart2.Series[0].BorderWidth = 2;
            chart2.Series[1].BorderWidth = 2;

            foreach (var point in pointsImitModel)
            {
                this.chart2.Series["Potk \n(имитационная модель)"].Points.AddXY(point.Key, point.Value);
            }
            double currLambda = 0.1;
            double mu1 = 0.1;

            for (double i = lymdaStart; i < lymdaEnd; i += lymdaDelta)
            {
                currLambda += 0.1;
                mu1 += muDelta;

                double Potk = regArray[0] + regArray[1] * currLambda + regArray[2] * mu1 + regArray[3] * mu1 * currLambda;
                this.chart2.Series["Potk \n(планирование эксперимента)"].Points.AddXY(i, Potk);

            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
