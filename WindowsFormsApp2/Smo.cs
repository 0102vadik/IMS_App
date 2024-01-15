using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    public class Smo
    {
        private const int STATE_EMPTY = 0;
        private const int STATE_WORK = 2;
        private const int COUNT_CHANNELS_1 = 1;
        private const int COUNT_CHANNELS_2 = 2;
        private const int COUNT_CHANNELS_3 = 1;
        private RequestsAccumulator _accumulator;
        private RequestsAccumulator _accumulator2;
        private RequestsAccumulator _accumulator31;
        private RequestsAccumulator _accumulator32;
        private RequestChannel[] _channelsPhase1;
        private RequestChannel[] _channelsPhase2;
        private RequestChannel[] _channelsPhase3;
        private const int _accumulatorCapasity = 0;
        private const int _accumulatorCapasity2 = 0;
        private const int _accumulatorCapasity31 = 10;
        private const int _accumulatorCapasity32 = 26;
        private double _accumulatorStayingTime = 0;
        private double _accumulatorStayingTime2 = 0;
        private double _accumulatorStayingTime31 = 2.6;
        private double _accumulatorStayingTime32 = 1;
        public StringBuilder result = new StringBuilder();
        private int _rejectedRequestsCount = 0;
        private int _servicedRequestCount = 0;
        private double _omission = 0;
        public double Lymda { get; set; } = 2; 
        public double MuPhaseOne { get; set; } = 1;
        public double MuPhaseTwoOne { get; set; } = 2.5;
        public double MuPhaseTwoTwo { get; set; } = 1;
        public double MuPhaseThree { get; set; } = 1;

        public Smo(double lymbda, double m1, double m2, double tn1, double tn2)
        {
            this.Lymda = lymbda;
            StartInic();
        }

        public Smo()
        {
            StartInic();
        }

        private void StartInic()
        {
            _accumulator = new RequestsAccumulator(_accumulatorCapasity, _accumulatorStayingTime);
            _accumulator2 = new RequestsAccumulator(_accumulatorCapasity2, _accumulatorStayingTime2);
            _accumulator31 = new RequestsAccumulator(_accumulatorCapasity31, _accumulatorStayingTime31);
            _accumulator32 = new RequestsAccumulator(_accumulatorCapasity32, _accumulatorStayingTime32);

            _channelsPhase1 = new RequestChannel[COUNT_CHANNELS_1];
            for (int i = 0; i < COUNT_CHANNELS_1; i++)
            {
                _channelsPhase1[i] = new RequestChannel();
            }
            _channelsPhase2 = new RequestChannel[COUNT_CHANNELS_2];
            for (int i = 0; i < COUNT_CHANNELS_2; i++)
            {
                _channelsPhase2[i] = new RequestChannel();
            }
            _channelsPhase3 = new RequestChannel[COUNT_CHANNELS_3];
            for (int i = 0; i < COUNT_CHANNELS_3; i++)
            {
                _channelsPhase3[i] = new RequestChannel();
            }
        }

        public void StartSimulation(double endSimulationTime)
        {
            double checkTime = 0;
            double systemTime = 0;
            while (systemTime <= endSimulationTime)
            {
                systemTime += 0.01;
                // _storageDevicePhaseOne.TimedOutRequests(systemTime);
                if (systemTime >= checkTime)
                {
                    CheckStorageDeviceAndPhases(systemTime);
                    PullRequest(systemTime);
                    checkTime += CalculateIputStreamRequestTime();
                }
            }
            _omission = (double)(_rejectedRequestsCount + _servicedRequestCount) / systemTime;
        }
        private void PullRequest(double systemTime)
        {
            bool isPhaseOneUsable = false;
            for (int i = 0; i < _channelsPhase1.Length; i++)
            {
                if (_channelsPhase1[i].State == STATE_EMPTY)
                {
                    double time = CalculatePhasesChannelsServiceTime(MuPhaseOne, 1);
                    _channelsPhase1[i].ServiceTime = systemTime + time;
                    _channelsPhase1[i].State = STATE_WORK;
                    isPhaseOneUsable = true;
                    result.AppendLine("Заявка обрабатывается в фазе 1 каналом №" + (i + 1) + " .Времязаявки " + time);
                    break;
                }
            }
            if (!isPhaseOneUsable)
            {
                if (_accumulator.IsStorDeviceAvailable())
                {
                    _accumulator.AddRequest(systemTime);
                    result.AppendLine("Заявкадобавленавнакопитель (Фаза 1). Емкостьнакопителя " + _accumulator.Count());
                }
                else
                {
                    _rejectedRequestsCount++;
                    result.AppendLine("Заявкаотклонена");
                }
            }
            for (int i = 0; i < _channelsPhase2.Length; i++)
            {
                if (_accumulator2.Count() > 0)
                {
                    if (_channelsPhase2[i].State == STATE_EMPTY)
                    {
                        double calculatePhasesChannelsServiceTime = 0;
                        if (i == 0) calculatePhasesChannelsServiceTime = CalculatePhasesChannelsServiceTime(MuPhaseTwoOne, 2);
                        else calculatePhasesChannelsServiceTime = CalculatePhasesChannelsServiceTime(MuPhaseTwoTwo, 3);
                        _channelsPhase2[i].ServiceTime = systemTime + calculatePhasesChannelsServiceTime;
                        _channelsPhase2[i].State = STATE_WORK;
                        _accumulator2.DeleteRequest(systemTime);
                        break;
                    }
                }
            }
            for (int i = 0; i < _channelsPhase3.Length; i++)
            {
                if (_accumulator31.Count() > 0)
                {
                    if (_channelsPhase3[i].State == STATE_EMPTY)
                    {
                        _channelsPhase3[i].ServiceTime = systemTime + CalculatePhasesChannelsServiceTime(MuPhaseThree, 4);
                        _channelsPhase3[i].State = STATE_WORK;
                        _accumulator31.DeleteRequest(systemTime);
                        break;
                    }
                }
            }
            for (int i = 0; i < _channelsPhase3.Length; i++)
            {
                if (_accumulator32.Count() > 0)
                {
                    if (_channelsPhase3[i].State == STATE_EMPTY)
                    {
                        _channelsPhase3[i].ServiceTime = systemTime + CalculatePhasesChannelsServiceTime(MuPhaseThree, 4);
                        _channelsPhase3[i].State = STATE_WORK;
                        _accumulator32.DeleteRequest(systemTime);
                        break;
                    }
                }
            }
        }
        private void CheckStorageDeviceAndPhases(double systemTime)
        {
            //проверка 3-ей фазы
            for (int i = 0; i < _channelsPhase3.Length; i++)
            {
                if (_channelsPhase3[i].State == STATE_WORK)
                {
                    if (_channelsPhase3[i].ServiceTime <= systemTime)
                    {
                        _servicedRequestCount++;
                        result.AppendLine("Заявкаобработана в фазе 3 каналом № " + (i + 1));
                        _channelsPhase3[i].State = STATE_EMPTY;
                        CheckStorageDeviceAndPhases(systemTime);
                    }
                }
            }
            //проверка 2-ой фазы первого канала
            for (int i = 0; i < 1; i++)
            {
                if (_channelsPhase2[i].State != STATE_EMPTY)
                {
                    if (_channelsPhase2[i].ServiceTime <= systemTime)
                    {
                        int notEmptyChanelsCount = 0;
                        for (int j = 0; j < _channelsPhase3.Length; j++)
                        {
                            if (_channelsPhase3[j].State == STATE_EMPTY)
                            {
                                double time = 0;
                                if (i == 0) time = CalculatePhasesChannelsServiceTime(MuPhaseTwoOne, 2);
                                else time = CalculatePhasesChannelsServiceTime(MuPhaseTwoTwo, 3);
                                _channelsPhase3[j].ServiceTime = systemTime + time;
                                _channelsPhase3[j].State = STATE_WORK;
                                _channelsPhase2[i].State = STATE_EMPTY;
                                result.AppendLine("Заявка обрабатывается в фазе 3 каналом №" + (j + 1) + " .Время заявки " + time);
                                CheckStorageDeviceAndPhases(systemTime);
                                break;
                            }
                            else
                            {
                                notEmptyChanelsCount++;
                                continue;
                            }
                        }
                        if (notEmptyChanelsCount == _channelsPhase3.Length)
                        {
                            if (_accumulator31.IsStorDeviceAvailable())
                            {
                                _accumulator31.AddRequest(systemTime);
                                result.AppendLine("Заявка добавлена в накопитель (Фаза 3.1). Емкость накопителя " + _accumulator31.Count());
                            }
                            else
                            {
                                _rejectedRequestsCount++;
                                result.AppendLine("Заявка отклонена");
                            }
                        }
                    }
                }
            }
            //проверка 2-ой фазы второго канала
            for (int i = 1; i < 2; i++)
            {
                if (_channelsPhase2[i].State != STATE_EMPTY)
                {
                    if (_channelsPhase2[i].ServiceTime <= systemTime)
                    {
                        int notEmptyChanelsCount = 0;
                        for (int j = 0; j < _channelsPhase3.Length; j++)
                        {
                            if (_channelsPhase3[j].State == STATE_EMPTY)
                            {
                                double time = CalculatePhasesChannelsServiceTime(MuPhaseOne, 1);
                                _channelsPhase3[j].ServiceTime = systemTime + time;
                                _channelsPhase3[j].State = STATE_WORK;
                                _channelsPhase2[i].State = STATE_EMPTY;
                                result.AppendLine("Заявка обрабатывается в фазе 3 каналом №" + (j + 1) + " .Время заявки " + time);
                                CheckStorageDeviceAndPhases(systemTime);
                                break;
                            }
                            else
                            {
                                notEmptyChanelsCount++;
                                continue;
                            }
                        }
                        if (notEmptyChanelsCount == _channelsPhase3.Length)
                        {
                            if (_accumulator32.IsStorDeviceAvailable())
                            {
                                _accumulator32.AddRequest(systemTime);
                                result.AppendLine("Заявка добавлена в накопитель (Фаза 3.2). Емкостьнакопителя " + _accumulator32.Count());
                            }
                            else
                            {
                                _rejectedRequestsCount++;
                                result.AppendLine("Заявка отклонена");
                            }
                        }
                    }
                }
            }
            //проверка 1-ой фазы и блокировка каналов
            for (int i = 0; i < _channelsPhase1.Length; i++)
            {
                if (_channelsPhase1[i].State != STATE_EMPTY)
                {
                    if (_channelsPhase1[i].ServiceTime <= systemTime)
                    {
                        int notEmptyChanelsCount = 0;
                        for (int j = 0; j < _channelsPhase2.Length; j++)
                        {
                            if (_channelsPhase2[j].State == STATE_EMPTY)
                            {
                                double time = CalculatePhasesChannelsServiceTime(MuPhaseOne, 2);
                                _channelsPhase2[j].ServiceTime = systemTime + time;
                                _channelsPhase2[j].State = STATE_WORK;
                                _channelsPhase1[i].State = STATE_EMPTY;
                                result.AppendLine("Заявка обрабатывается в фазе 2 каналом №" + (j + 1) + " .Время заявки " + time);
                                CheckStorageDeviceAndPhases(systemTime);
                                break;
                            }
                            else
                            {
                                notEmptyChanelsCount++;
                                continue;
                            }
                        }
                        if (notEmptyChanelsCount == _channelsPhase2.Length)
                        {
                            if (_accumulator2.IsStorDeviceAvailable())
                            {
                                _accumulator2.AddRequest(systemTime);
                                result.AppendLine("Заявка добавлена в накопитель (Фаза 2). Емкостьнакопителя " + _accumulator2.Count());
                            }
                            else
                            {
                                _rejectedRequestsCount++;
                                result.AppendLine("Заявкаотклонена");
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < _channelsPhase1.Length; i++)
            {
                if (_accumulator.Count() > 0)
                {
                    if (_channelsPhase1[i].State == STATE_EMPTY)
                    {
                        _channelsPhase1[i].ServiceTime = systemTime + CalculatePhasesChannelsServiceTime(MuPhaseOne, 1);
                        _channelsPhase1[i].State = STATE_WORK;
                        _accumulator.DeleteRequest(systemTime);
                        break;
                    }
                }
            }
        }
        private double CalculateIputStreamRequestTime()
        {
            return -Math.Log(StaticRandom.NextDouble()) / Lymda; //показательный для входа
        }
        private double CalculatePhasesChannelsServiceTime(double mu, int isVariant)
        {
            switch (isVariant)
            {
                case 1: return -Math.Log(StaticRandom.NextDouble()) / Lymda; //показательный (экспоненциальный) для фазы 1
                case 2: return StaticRandom.NextDouble() / mu; // равномерно фазы 2.1
                case 3: return -Math.Log(StaticRandom.NextDouble()) / Lymda; //показательный (экспоненциальный) для фазы 2.2
                case 4: return 1 / (mu * Math.Sqrt(2 * Math.PI)) * Math.Exp(-Math.Pow(StaticRandom.NextDouble() - 0.5, 2) / (2 * Math.Pow(mu, 2)));  //нормальный для фазы 3
            }
            return 0;
        }
        public double GetMaxTimeInStorageDevice()
        {
            return Math.Max(_accumulator.GetMaxTimeIn(), _accumulator2.GetMaxTimeIn());
        }
        public double GetAverageTimeInStorageDevice()
        {
            return (_accumulator.GetAverageTimeIn() + _accumulator.GetAverageTimeIn()) / 2;
        }
        public int GetRejectedRequests()
        {
            return _rejectedRequestsCount;
        }
        public int GetServicedRequests()
        {
            return _servicedRequestCount;
        }
        public double GetAbsoluteOmissionAbility()
        {
            return _omission;
        }
    }

}
