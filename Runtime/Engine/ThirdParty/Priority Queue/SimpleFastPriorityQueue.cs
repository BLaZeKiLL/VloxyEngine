using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Priority_Queue {

    /// <summary>
    /// A simplified priority queue implementation.  Is stable, auto-resizes, and thread-safe, at the cost of being slightly slower than
    /// FastPriorityQueue
    /// Methods tagged as O(1) or O(log n) are assuming there are no duplicates.  Duplicates may increase the algorithmic complexity.
    /// </summary>
    /// <typeparam name="TItem">The type to enqueue</typeparam>
    /// <typeparam name="TPriority">The priority-type to use for nodes.  Must extend IComparable&lt;TPriority&gt;</typeparam>
    public class SimpleFastPriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority> {

        private class SimpleNode : GenericPriorityQueueNode<TPriority> {

            public TItem Data { get; private set; }

            public SimpleNode(TItem data) {
                Data = data;
            }

        }

        private const int INITIAL_QUEUE_SIZE = 10;
        private readonly GenericPriorityQueue<SimpleNode, TPriority> _queue;
        private readonly Dictionary<TItem, IList<SimpleNode>> _itemToNodesCache;
        private readonly IList<SimpleNode> _nullNodesCache;

        #region Constructors

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        public SimpleFastPriorityQueue() : this(Comparer<TPriority>.Default, EqualityComparer<TItem>.Default) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparer used to compare TPriority values.  Defaults to Comparer&lt;TPriority&gt;.default</param>
        public SimpleFastPriorityQueue(IComparer<TPriority> priorityComparer) : this(priorityComparer.Compare,
            EqualityComparer<TItem>.Default) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparison function to use to compare TPriority values</param>
        public SimpleFastPriorityQueue(Comparison<TPriority> priorityComparer) : this(priorityComparer,
            EqualityComparer<TItem>.Default) { }

        /// <summary>
        /// Instantiate a new Priority Queue       
        /// </summary>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimpleFastPriorityQueue(IEqualityComparer<TItem> itemEquality) : this(Comparer<TPriority>.Default,
            itemEquality) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparer used to compare TPriority values.  Defaults to Comparer&lt;TPriority&gt;.default</param>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimpleFastPriorityQueue(IComparer<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) :
            this(priorityComparer.Compare, itemEquality) { }

        /// <summary>
        /// Instantiate a new Priority Queue
        /// </summary>
        /// <param name="priorityComparer">The comparison function to use to compare TPriority values</param>
        /// <param name="itemEquality">The equality comparison function to use to compare TItem values</param>
        public SimpleFastPriorityQueue(Comparison<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) {
            _queue = new GenericPriorityQueue<SimpleNode, TPriority>(INITIAL_QUEUE_SIZE, priorityComparer);
            _itemToNodesCache = new Dictionary<TItem, IList<SimpleNode>>(itemEquality);
            _nullNodesCache = new List<SimpleNode>();
        }

        #endregion

        /// <summary>
        /// Given an item of type T, returns the existing SimpleNode in the queue
        /// </summary>
        private SimpleNode GetExistingNode(TItem item) {
            if (item == null) {
                return _nullNodesCache.Count > 0 ? _nullNodesCache[0] : null;
            }

            IList<SimpleNode> nodes;
            if (!_itemToNodesCache.TryGetValue(item, out nodes)) {
                return null;
            }

            return nodes[0];
        }

        /// <summary>
        /// Adds an item to the Node-cache to allow for many methods to be O(1) or O(log n)
        /// </summary>
        private void AddToNodeCache(SimpleNode node) {
            if (node.Data == null) {
                _nullNodesCache.Add(node);
                return;
            }

            IList<SimpleNode> nodes;
            if (!_itemToNodesCache.TryGetValue(node.Data, out nodes)) {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[node.Data] = nodes;
            }

            nodes.Add(node);
        }

        /// <summary>
        /// Removes an item to the Node-cache to allow for many methods to be O(1) or O(log n) (assuming no duplicates)
        /// </summary>
        private void RemoveFromNodeCache(SimpleNode node) {
            if (node.Data == null) {
                _nullNodesCache.Remove(node);
                return;
            }

            IList<SimpleNode> nodes;
            if (!_itemToNodesCache.TryGetValue(node.Data, out nodes)) {
                return;
            }

            nodes.Remove(node);
            if (nodes.Count == 0) {
                _itemToNodesCache.Remove(node.Data);
            }
        }

        /// <summary>
        /// Returns the number of nodes in the queue.
        /// O(1)
        /// </summary>
        public int Count {
            get { return _queue.Count; }
        }

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// Throws an exception when the queue is empty.
        /// O(1)
        /// </summary>
        public TItem First {
            get {
                if (_queue.Count <= 0) {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }

                return _queue.First.Data;
            }
        }

        /// <summary>
        /// Removes every node from the queue.
        /// O(n)
        /// </summary>
        public void Clear() {
            _queue.Clear();
            _itemToNodesCache.Clear();
            _nullNodesCache.Clear();
        }

        /// <summary>
        /// Returns whether the given item is in the queue.
        /// O(1)
        /// </summary>
        public bool Contains(TItem item) {
            return item == null ? _nullNodesCache.Count > 0 : _itemToNodesCache.ContainsKey(item);
        }

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
        /// If queue is empty, throws an exception
        /// O(log n)
        /// </summary>
        public TItem Dequeue() {
            if (_queue.Count <= 0) {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }

            SimpleNode node = _queue.Dequeue();
            RemoveFromNodeCache(node);
            return node.Data;
        }

        /// <summary>
        /// Enqueue the item with the given priority, without calling lock(_queue) or AddToNodeCache(node)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private SimpleNode EnqueueNoLockOrCache(TItem item, TPriority priority) {
            SimpleNode node = new SimpleNode(item);
            if (_queue.Count == _queue.MaxSize) {
                _queue.Resize(_queue.MaxSize * 2 + 1);
            }

            _queue.Enqueue(node, priority);
            return node;
        }

        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// This queue automatically resizes itself, so there's no concern of the queue becoming 'full'.
        /// Duplicates and null-values are allowed.
        /// O(log n)
        /// </summary>
        public void Enqueue(TItem item, TPriority priority) {
            IList<SimpleNode> nodes;
            if (item == null) {
                nodes = _nullNodesCache;
            } else if (!_itemToNodesCache.TryGetValue(item, out nodes)) {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[item] = nodes;
            }

            SimpleNode node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
        }

        /// <summary>
        /// Enqueue a node to the priority queue if it doesn't already exist.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// This queue automatically resizes itself, so there's no concern of the queue becoming 'full'.  Null values are allowed.
        /// Returns true if the node was successfully enqueued; false if it already exists.
        /// O(log n)
        /// </summary>
        public bool EnqueueWithoutDuplicates(TItem item, TPriority priority) {
            IList<SimpleNode> nodes;
            if (item == null) {
                if (_nullNodesCache.Count > 0) {
                    return false;
                }

                nodes = _nullNodesCache;
            } else if (_itemToNodesCache.ContainsKey(item)) {
                return false;
            } else {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[item] = nodes;
            }

            SimpleNode node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
            return true;
        }

        /// <summary>
        /// Removes an item from the queue.  The item does not need to be the head of the queue.  
        /// If the item is not in the queue, an exception is thrown.  If unsure, check Contains() first.
        /// If multiple copies of the item are enqueued, only the first one is removed. 
        /// O(log n)
        /// </summary>
        public void Remove(TItem item) {
            SimpleNode removeMe;
            IList<SimpleNode> nodes;
            if (item == null) {
                if (_nullNodesCache.Count == 0) {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " +
                                                        item);
                }

                removeMe = _nullNodesCache[0];
                nodes = _nullNodesCache;
            } else {
                if (!_itemToNodesCache.TryGetValue(item, out nodes)) {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " +
                                                        item);
                }

                removeMe = nodes[0];
                if (nodes.Count == 1) {
                    _itemToNodesCache.Remove(item);
                }
            }

            _queue.Remove(removeMe);
            nodes.Remove(removeMe);
        }

        /// <summary>
        /// Call this method to change the priority of an item.
        /// Calling this method on a item not in the queue will throw an exception.
        /// If the item is enqueued multiple times, only the first one will be updated.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to update all of them, please wrap your items in a wrapper class so they can be distinguished).
        /// O(log n)
        /// </summary>
        public void UpdatePriority(TItem item, TPriority priority) {
            SimpleNode updateMe = GetExistingNode(item);
            if (updateMe == null) {
                throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " +
                                                    item);
            }

            _queue.UpdatePriority(updateMe, priority);
        }

        /// <summary>
        /// Returns the priority of the given item.
        /// Calling this method on a item not in the queue will throw an exception.
        /// If the item is enqueued multiple times, only the priority of the first will be returned.
        /// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
        /// to query all their priorities, please wrap your items in a wrapper class so they can be distinguished).
        /// O(1)
        /// </summary>
        public TPriority GetPriority(TItem item) {
            SimpleNode findMe = GetExistingNode(item);
            if (findMe == null) {
                throw new InvalidOperationException(
                    "Cannot call GetPriority() on a node which is not enqueued: " + item);
            }

            return findMe.Priority;
        }

        public IEnumerator<TItem> GetEnumerator() {
            return _queue.Select(x => x.Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool IsValidQueue() {
            // Check all items in cache are in the queue
            foreach (IList<SimpleNode> nodes in _itemToNodesCache.Values) {
                foreach (SimpleNode node in nodes) {
                    if (!_queue.Contains(node)) {
                        return false;
                    }
                }
            }

            // Check all items in queue are in cache
            foreach (SimpleNode node in _queue) {
                if (GetExistingNode(node.Data) == null) {
                    return false;
                }
            }

            // Check queue structure itself
            return _queue.IsValidQueue();
        }

    }

}