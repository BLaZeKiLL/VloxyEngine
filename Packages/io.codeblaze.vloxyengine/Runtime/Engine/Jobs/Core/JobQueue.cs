using Priority_Queue;

namespace CodeBlaze.Vloxy.Engine.Jobs.Core {

    public class JobQueue<T> {

        private SimplePriorityQueue<T> _ClaimQueue;
        private SimplePriorityQueue<T> _ReclaimQueue;

        public JobQueue() {
            _ClaimQueue = new SimplePriorityQueue<T>();
            _ReclaimQueue = new SimplePriorityQueue<T>();
        }

        /// <summary>
        /// Schedule something for computation
        /// </summary>
        public void Schedule() { }
        
        /// <summary>
        /// Take items for queue and send for processing
        /// </summary>
        public void Process() { }
        
        /// <summary>
        /// item has been processed
        /// </summary>
        public void Complete() { }
        
        /// <summary>
        /// 
        /// </summary>
        public void Clean() { }

    }

}