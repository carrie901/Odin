﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odin.Tests
{
    [DefaultAction("DoSomething")]
    public class SubCommandController : Controller
    {
        [Action]
        public virtual void DoSomething()
        {
            
        }
    }
}
