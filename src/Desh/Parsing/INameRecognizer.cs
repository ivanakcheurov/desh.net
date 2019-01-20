using System;
using System.Collections.Generic;
using System.Text;

namespace Desh.Parsing
{
    public interface INameRecognizer
    {
        bool Recognize(string name);
    }
}
