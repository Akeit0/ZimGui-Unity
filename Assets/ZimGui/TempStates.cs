using System;

namespace ZimGui {
    public struct DisposableIndent:IDisposable  {
        public void Dispose() {
            IMStyle.IndentLevel--;
        }
    }
    
    
    public readonly struct DisposableDivision:IDisposable {
        public readonly int LastDivisions;
        public DisposableDivision(int lastDivisions) {
            LastDivisions = lastDivisions;
        }
        
        public void Dispose() {
            IMStyle.HorizontalDivisions=LastDivisions;
            IMStyle.HorizontalDivisionsProgress=0;
        }
    }
    
    
    
}