using System;

namespace ZimGui.Text {
    public static class StringEx {
        public static unsafe bool Equals(this string text, ReadOnlySpan<char> span) {
            if (text.Length != span.Length) return false;
            int width = sizeof(nuint) / sizeof(char);
            var count = text.Length / width;
            var rem = text.Length % width;
            fixed (char* ptr1 = text) {
                fixed(char*ptr2=span)
                {
                    var lPtr1 = (nuint*)ptr1;
                    var lPtr2 = (nuint*)ptr2;
                    for (int i = 0; i < count; i++)
                    {
                        if (lPtr1[i] != lPtr2[i])
                        {
                            return false;
                        }
                    }
                    for (int i = 0; i < rem; i++)
                    {
                        if ((ptr1)[count * width + i] != (ptr2)[count * width + i])
                        {
                            return false;
                        }
                    }
                    
                }
            }

            return true;
        } 
        public static  unsafe bool Equals(this ReadOnlySpan<char> left, ReadOnlySpan<char> right) {
            if (left.Length != right.Length) return false;
            int width = sizeof(nuint) / sizeof(char);
            var count = left.Length / width;
            var rem = left.Length % width;
            fixed (char* ptr1 = left) {
                fixed(char*ptr2=right) {
                    var lPtr1 = (nuint*)ptr1;
                    var lPtr2 = (nuint*)ptr2;
                    for (int i = 0; i < count; i++) {
                        if (lPtr1[i] != lPtr2[i]) return false;
                    }
                    if (rem == 0) return true;
                    for (int i = 0; i < rem; i++)
                    {
                        if ((ptr1)[count * width + i] != (ptr2)[count * width + i])
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        
       public static  unsafe int GetExHashCode(this string t) {
            var length = t.Length;
            switch (length) {
                case 0: return 0;
                case 1: return (((5381 << 5) + 5381) ^ t[0]) * 1566083941;
                case 2:
                    var hash = ((5381 << 5) + 5381) ^ t[0];
                    return ((hash << 5) + hash) ^ t[1] * 1566083941;
                case 3:
                    hash = ((5381 << 5) + 5381) ^ t[0];
                    hash = ((hash << 5) + hash) ^ t[1];
                    return ((hash << 5) + hash) ^ t[2] * 1566083941;
                default:
                    fixed (char* ptr = t) {
                        hash = 5381;
                        var s = (int*) ptr;
                        var end = s + (length + 1) / 2;
                        do {
                            hash = ((hash << 5) + hash) ^ s[0];
                        } while (end > ++s);
                        return (hash * 1566083941);
                    }
            }
        }
        public static  unsafe long GetExHashCode64(this string t) {
            var length = t.Length;
            switch (length) {
                case 0: return 0;
                case 1: return ((177573L) ^ t[0]) * 1566083941L;
                case 2:
                    var hash = (177573L) ^ t[0];
                    return ((hash << 5) + hash) ^ t[1] * 1566083941L;
                case 3:
                    hash = (177573L) ^ t[0];
                    hash = ((hash << 5) + hash) ^ t[1];
                    return ((hash << 5) + hash) ^ t[2] * 1566083941L;
                default:
                    fixed (char* ptr = t) {
                        hash = 5381L;
                        var s = (int*) ptr;
                        var end = s + (length + 1) / 2;
                        do {
                            hash = ((hash << 5) + hash) ^ s[0];
                        } while (end > ++s);
                        return (hash * 1566083941L);
                    }
            }
        }

       
       public static  unsafe int GetExHashCode(this ReadOnlySpan<char> t) {
            var length = t.Length;
            switch (length) {
                case 0: return 0;
                case 1: return (177573 ^ t[0]) * 1566083941;
                case 2:
                    var hash = ( 177573) ^ t[0];
                    return ((hash << 5) + hash) ^ t[1] * 1566083941;
                case 3:
                    hash =  177573^ t[0];
                    hash = ((hash << 5) + hash) ^ t[1];
                    return ((hash << 5) + hash) ^ t[2] * 1566083941;
                default:
                    fixed (char* ptr = t) {
                        hash = 5381;
                        var s = (int*) ptr;
                        var end = s + length / 2;
                        do {
                            hash = ((hash << 5) + hash) ^ s[0];
                        } while (end > ++s);
                        if (length % 2 != 0) {
                            hash = ((hash << 5) + hash) ^ (s[0] & 0xFFFF);
                        }
                        return (hash * 1566083941);
                    }
            }
        }
    }
}