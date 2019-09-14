using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlVJ
{
  public class TestUART
  {
    public TestUART(string _commandUART, string _textUART)
    {
      CommandUART = _commandUART;
      TextUART = _textUART;
    }

    public string CommandUART { get; set; }

    public string TextUART { get; set; }
  }
}
