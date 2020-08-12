﻿using Log4NetDemo.ObjectRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.Repository
{
    public interface ILoggerRepository
    {
        RendererMap RendererMap { get; }
    }
}