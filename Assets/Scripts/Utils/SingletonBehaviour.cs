using UnityEngine;

namespace CodeBlaze.Vloxy.Demo.Utils {
    
    [DefaultExecutionOrder(-100)]
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T> {

        public static T Current { get; private set; }

        [SerializeField] private bool _isPersistant;

        protected virtual void Initialize() { }

        private void Awake() {
            if (!Current) {
                Current = this as T;
                
                if (_isPersistant) DontDestroyOnLoad(gameObject);
                
                Initialize();
            }
            else if (!_isPersistant) Destroy(gameObject);
        }

    }
    
}