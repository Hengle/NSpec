﻿using NSpec.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.Domain
{
    public abstract class MethodExampleBase : ExampleBase
    {
        public MethodExampleBase(MethodInfo method, string tags) 
            : base(method.Name.Replace("_", " "), tags)
        {
            this.method = method;
        }

        public override bool IsAsync
        {
            get { return method.IsAsync(); }
        }

        protected MethodInfo method;
    }
}
