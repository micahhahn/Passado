using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Passado.Tests
{
    public class CompilationError
    {
        public string ErrorId { get; set; }
        public string ErrorText { get; set; }
        public string[] Locations { get; set; }
    }
}
