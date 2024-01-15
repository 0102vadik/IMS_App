using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class RequestsAccumulator
    {
        private int _capacity;
        private double _stayingTime;
        private Queue<Request> _reqList;
        private static int _successRequest = 0;
        private static double _fullRequestsTime = 0;
        private static double _maxTimeIn = 0;
        public RequestsAccumulator(int capacity, double stayingTime)
        {
            _capacity = capacity;
            _stayingTime = stayingTime;
            _reqList = new Queue<Request>();
        }
        public void TimedOutRequests(double systemTime)
        {
            if (_reqList.Count > 0)
            {
                for (int i = 0; i < _reqList.Count; i++)
                {
                    if (_reqList.Peek().TimeInAccumulator >= systemTime - _stayingTime)
                    {
                        _reqList.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        public void DeleteRequest(double systemTime)
        {
            if (_reqList.Count > 0)
            {
                _successRequest++;
                if (_maxTimeIn < systemTime - _reqList.Peek().TimeInAccumulator)
                {
                    _maxTimeIn = systemTime - _reqList.Peek().TimeInAccumulator;
                }
                _fullRequestsTime += systemTime - _reqList.Peek().TimeInAccumulator;
                _reqList.Dequeue();
            }
        }
        public void AddRequest(double systemTime)
        {
            if (IsStorDeviceAvailable())
            {
                Request request = new Request() { TimeInAccumulator = systemTime };
                _reqList.Enqueue(request);
            }
        }
        public bool IsStorDeviceAvailable()
        {
            if (_reqList.Count < _capacity)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public double GetMaxTimeIn()
        {
            return _maxTimeIn > _stayingTime ? _stayingTime : _maxTimeIn;
        }
        public double GetAverageTimeIn()
        {
            return _fullRequestsTime / _successRequest;
        }
        public int Count()
        {
            return _reqList.Count;
        }
    }

}
