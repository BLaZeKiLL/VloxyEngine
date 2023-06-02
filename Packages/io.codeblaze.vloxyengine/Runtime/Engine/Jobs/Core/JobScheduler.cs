using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CodeBlaze.Vloxy.Engine.Jobs.Core {

    public abstract class JobScheduler {

        private Queue<long> _Timings;
        private Stopwatch _Watch;
        private int _Records;

        protected JobScheduler(int records = 16) {
            _Records = records;
            _Watch = new Stopwatch();
            _Timings = new Queue<long>(_Records);
        }

        public float AvgTime => (float) _Timings.Sum() / 10;

        protected void StartRecord() {
            _Watch.Restart();
        }
        
        protected void StopRecord() {
            _Watch.Stop();
            var ms = _Watch.ElapsedMilliseconds;
            
            if (_Timings.Count <= _Records) _Timings.Enqueue(ms);
            else {
                _Timings.Dequeue();
                _Timings.Enqueue(ms);
            }
        }
        
    }

}