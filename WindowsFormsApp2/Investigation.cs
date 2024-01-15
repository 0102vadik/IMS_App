using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class Investigation
    {
        private double _kohrenCrit;
        private double _fisherCrit;
        private double[] _studentCrit;
        public double[] regArray = new double[4];
        public Investigation(
        double lymdaStart,
        double lymdaEnd,
        double muStart,
        double muEnd)
        {
            StartInvestigation(lymdaStart, lymdaEnd, muStart, muEnd);
        }
        private void StartInvestigation(
        double lymdaStart,
        double lymdaEnd,
        double muStart,
        double muEnd)
        {
            int m = 2;
            double[,] matModel = new double[4, 6];
            double[] SuArray = new double[4];

            Smo[] array = { new Smo { Lymda = lymdaStart, MuPhaseOne = muStart, MuPhaseThree=muStart },
                new Smo { Lymda = lymdaStart, MuPhaseOne = muEnd, MuPhaseThree = muEnd },
                new Smo { Lymda = lymdaEnd, MuPhaseOne = muStart, MuPhaseThree = muStart },
                new Smo { Lymda = lymdaEnd, MuPhaseOne = muEnd, MuPhaseThree = muEnd }
            };
            InitializeMatModel(ref matModel);
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 3; j < 5; j++)
                {
                    array[i].StartSimulation(1000);
                    double rejected = array[i].GetRejectedRequests();
                    double serviced = array[i].GetServicedRequests();
                    matModel[i, j] = rejected / (rejected + serviced) * 100;
                }
                matModel[i, 5] = (matModel[i, 3] + matModel[i, 4]) / 2;
            }
            StringBuilder res1 = new StringBuilder();
            FileStream fileStr = null;
            try
            {
                fileStr = new FileStream("yn.txt", FileMode.Create);
                using (StreamWriter sw = new StreamWriter(fileStr))
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        for (int j = 3; j < 5; j++)
                        {
                            res1.Append(matModel[i, j] + ", ");
                        }
                        res1.Append("Average value is " + matModel[i, 5] + " ");
                        res1.AppendLine(" ");
                    }
                    sw.WriteLine(res1.ToString());
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
            for (int i = 0; i < array.Length; i++)
            {
                double tmp1 = Math.Pow(matModel[i, 5] - matModel[i, 3], 2);
                double tmp2 = Math.Pow(matModel[i, 5] - matModel[i, 4], 2);
                SuArray[i] = 1 / (m - 1) * (Math.Pow(matModel[i, 5] - matModel[i, 3], 2) + Math.Pow(matModel[i, 5] - matModel[i, 4], 2));
            }
            double Sm = GetMaxValue(SuArray);
            double SumSu = GetSum(SuArray);
            double Sigma = Sm / SumSu;


            regArray[0] = GetSum(new double[] { matModel[0, 5], matModel[1, 5], matModel[2, 5], matModel[3, 5] }) / 4;
            regArray[1] = GetSum(new double[] {
            matModel[0, 5] * matModel[0, 0], matModel[1, 5] * matModel[1, 0],
                            matModel[2, 5] * matModel[2, 0], matModel[3, 5] * matModel[3, 0] }) / 4;
            regArray[2] = GetSum(new double[] {
            matModel[0, 5] * matModel[0, 1], matModel[1, 5] * matModel[1, 1],
                            matModel[2, 5] * matModel[2, 1], matModel[3, 5] * matModel[3, 1] }) / 4;
            regArray[3] = GetSum(new double[] {
            matModel[0, 5] * matModel[0, 2], matModel[1, 5] * matModel[1, 2],
                            matModel[2, 5] * matModel[2, 2], matModel[3, 5] * matModel[3, 2] }) / 4;

            double Sy = SumSu / SuArray.Length;
            double Sa = Sy / (m * 4);
            double[] tArray = new double[4];
            for (int i = 0; i < tArray.Length; i++)
            {
                tArray[i] = Math.Abs(regArray[i]) / Sa;
            }
            _studentCrit = tArray;
            double[] deltaA = new double[_studentCrit.Length];

            double studentTableCriteria = 2.78;
            for (int i = 0; i < _studentCrit.Length; i++)
            {
                if (_studentCrit[i] < studentTableCriteria)
                {
                    _studentCrit[i] = 0;
                    deltaA[i] = _studentCrit[i] * Sa;
                }
            }
            double tmp = GetSadk(matModel);
            double Sadk = GetSadk(matModel) / Sy;

            _kohrenCrit = Sigma;
            _fisherCrit = Math.Pow(GetSadk(matModel), 2) / Math.Pow(Sy, 2);

        }
        public double GetKohrenCriterion()
        {
            return _kohrenCrit;
        }
        public double[] GetStudentCriterionArray()
        {
            return _studentCrit;
        }
        public double GetFisherCriterion()
        {
            return _fisherCrit;
        }
        void WithdrawArray(double[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                Console.Write(" " + (i + 1));
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    Console.Write("      " + array[i, j]);
                }
                Console.WriteLine();
            }
        }
        double GetMaxValue(double[] array)
        {
            double max = array[0];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    max = array[i];
                }
            }
            return max;
        }
        double GetSum(double[] array)
        {
            double sum = 0;
            for (int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return sum;
        }
        void InitializeMatModel(ref double[,] regArray)
        {
            regArray[0, 0] = 1;
            regArray[0, 1] = -1;
            regArray[0, 2] = -1;
            regArray[1, 0] = -1;
            regArray[1, 1] = -1;
            regArray[1, 2] = 1;
            regArray[2, 0] = 1;
            regArray[2, 1] = 1;
            regArray[2, 2] = 1;
            regArray[3, 0] = -1;
            regArray[3, 1] = 1;
            regArray[3, 2] = -1;
        }

        double GetSadk(double[,] matModel)
        {
            double sum = 0;
            for (int i = 0; i < 3; i++)
            {
                sum += Math.Pow(matModel[i, 5] - matModel[i, 3], 2);
            }
            return sum / (4 - 1);
        }

    }
}
