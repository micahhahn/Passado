using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Core
{
    /// <summary>
    /// A placeholder type to declare that a column should be sorted in descending order in the index.
    /// </summary>
    public class Desc
    {
        public static explicit operator Desc(bool b) { throw new NotImplementedException(); }
        public static explicit operator Desc(bool? b) { throw new NotImplementedException(); }
        public static explicit operator Desc(byte b) { throw new NotImplementedException(); }
        public static explicit operator Desc(byte? b) { throw new NotImplementedException(); }
        public static explicit operator Desc(short s) { throw new NotImplementedException(); }
        public static explicit operator Desc(short? s) { throw new NotImplementedException(); }
        public static explicit operator Desc(int i) { throw new NotImplementedException(); }
        public static explicit operator Desc(int? i) { throw new NotImplementedException(); }
        public static explicit operator Desc(long l) { throw new NotImplementedException(); }
        public static explicit operator Desc(long? l) { throw new NotImplementedException(); }
        public static explicit operator Desc(DateTime d) { throw new NotImplementedException(); }
        public static explicit operator Desc(DateTime? d) { throw new NotImplementedException(); }
        public static explicit operator Desc(string s) { throw new NotImplementedException(); }
    }
}
