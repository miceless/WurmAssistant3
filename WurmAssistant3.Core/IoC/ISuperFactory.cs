﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AldursLab.WurmAssistant3.Core.IoC
{
    public interface ISuperFactory
    {
        /// <summary>
        /// Returns an instance of T from Kernel, based on binding configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>();

        /// <summary>
        /// Returns an instance of T from Kernel, based on binding configuration, with specific constructor arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>(params Ninject.Parameters.IParameter[] parameters);
    }
}
