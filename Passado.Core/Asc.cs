using System;

namespace Passado.Core
{
    /// <summary>
    /// A placeholder type to declare that a column should be sorted in ascending order in the index.
    /// </summary>
    public class Asc
    {
        public static explicit operator Asc(bool b) { throw new NotImplementedException(); }
        public static explicit operator Asc(bool? b) { throw new NotImplementedException(); }
        public static explicit operator Asc(byte b) { throw new NotImplementedException(); }
        public static explicit operator Asc(byte? b) { throw new NotImplementedException(); }
        public static explicit operator Asc(short s) { throw new NotImplementedException(); }
        public static explicit operator Asc(short? s) { throw new NotImplementedException(); }
        public static explicit operator Asc(int i) { throw new NotImplementedException(); }
        public static explicit operator Asc(int? i) { throw new NotImplementedException(); }
        public static explicit operator Asc(long l) { throw new NotImplementedException(); }
        public static explicit operator Asc(long? l) { throw new NotImplementedException(); }
        public static explicit operator Asc(DateTime d) { throw new NotImplementedException(); }
        public static explicit operator Asc(DateTime? d) { throw new NotImplementedException(); }
        public static explicit operator Asc(string s) { throw new NotImplementedException(); }
    }
}
