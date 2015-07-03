using System;
using System.IO;
using System.Reflection;

namespace Xunit
{
    internal class RemoteAppDomainManager : IDisposable
    {
        public RemoteAppDomainManager(string assemblyFileName, string configFileName, bool shadowCopy, string shadowCopyFolder)
        {
            Guard.ArgumentNotNullOrEmpty("assemblyFileName", assemblyFileName);

            assemblyFileName = Path.GetFullPath(assemblyFileName);
            Guard.FileExists("assemblyFileName", assemblyFileName);

            if (configFileName == null)
                configFileName = GetDefaultConfigFile(assemblyFileName);

            if (configFileName != null)
                configFileName = Path.GetFullPath(configFileName);

            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;
        }

        public string AssemblyFileName { get; private set; }

        public string ConfigFileName { get; private set; }

        public TObject CreateObject<TObject>(AssemblyName assemblyName, string typeName, params object[] args)
        {
            try
            {
#if DNXCORE50
                return (TObject)Activator.CreateInstance(Assembly.Load(assemblyName).GetType(typeName), args);
#else
                return (TObject)Activator.CreateInstance(AppDomain.CurrentDomain, assemblyName.FullName, typeName, false, BindingFlags.Default, null, args, null, null).Unwrap();
#endif
            }
            catch (TargetInvocationException ex)
            {
                ex.InnerException.RethrowWithNoStackTraceLoss();
                return default(TObject);
            }
        }

        public virtual void Dispose() { }

        static string GetDefaultConfigFile(string assemblyFile)
        {
            string configFilename = assemblyFile + ".config";

            if (File.Exists(configFilename))
                return configFilename;

            return null;
        }
    }
}