using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace com.iiko.unitech
{
    internal class LogFactory
    {
        private static LogFactory instance;

        public static LogFactory Instance => instance ?? (instance = new LogFactory());

        private LogFactory()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ThreadContext.Properties["LogFileDir"] = path;
            XmlConfigurator.Configure(new Uri(path + "\\log4net.config"));
        }

        public ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }
    }
}
