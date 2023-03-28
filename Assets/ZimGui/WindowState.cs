using System;

namespace ZimGui {
   
    public readonly struct WindowState:IDisposable {
        public readonly bool IsActive;
        public WindowState(bool isActive) {
            IsActive = isActive;
        }
        
        public void Dispose() {
           if(IsActive)IM.EndWindow();
        }

        public static bool operator true(WindowState state) => state.IsActive;

        public static bool operator false(WindowState state) => !state.IsActive ;

        public Enumerator GetEnumerator() { 
            return new Enumerator(this);
        }
      
        public struct Enumerator:IDisposable {
            public bool DoOnce;
            public bool  IsActive;

            public Enumerator(WindowState state) {
                DoOnce = state.IsActive;
                IsActive = state.IsActive;
            }
            public bool Current => true;
            public bool MoveNext() {
                if (!DoOnce) return false;
                DoOnce = false;
                return true;
            }
            public void Dispose() {
                if(IsActive)IM.EndWindow();
            }
        }
    }
    
}