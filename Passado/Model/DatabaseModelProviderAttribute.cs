using System;
using System.Collections.Generic;
using System.Text;

namespace Passado.Model
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DatabaseModelProviderAttribute : Attribute
    {
    }
}
