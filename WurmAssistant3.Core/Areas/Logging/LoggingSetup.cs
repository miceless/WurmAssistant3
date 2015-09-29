using System.IO;
using AldursLab.Essentials.Eventing;
using AldursLab.WurmAssistant3.Core.Areas.Logging.Contracts;
using AldursLab.WurmAssistant3.Core.Areas.Logging.Modules;
using AldursLab.WurmAssistant3.Core.Root.Contracts;
using Ninject;

namespace AldursLab.WurmAssistant3.Core.Areas.Logging
{
    public static class LoggingSetup
    {
        public static void Setup(IKernel kernel)
        {
            var logOutputDirFullPath = Path.Combine(kernel.Get<IWurmAssistantDataDirectory>().DirectoryPath, "Logs");
            LoggingManager manager = new LoggingManager(kernel.Get<IThreadMarshaller>());
            manager.Setup(logOutputDirFullPath);

            kernel.Bind<ILoggingConfig>().ToConstant(manager);
            kernel.Bind<ILoggerFactory>().ToConstant(manager);
            kernel.Bind<IWurmApiLoggerFactory>().ToConstant(manager);
            kernel.Bind<ILogMessageFlow>().ToConstant(manager);
            kernel.Bind<ILogMessageHandler>().ToConstant(manager);

            BindLoggerAutoResolver(kernel);
        }

        static void BindLoggerAutoResolver(IKernel kernel)
        {
            kernel.Bind<ILogger>().ToMethod(context =>
            {
                // create logger with category matching target type name
                var factory = context.Kernel.Get<ILoggerFactory>();
                var type = context.Request.Target.Member.DeclaringType;
                return factory.Create(type != null ? type.FullName : string.Empty);
            });
        }
    }
}