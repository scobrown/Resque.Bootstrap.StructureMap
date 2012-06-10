using System.Reflection;
using Resque.FailureBackend;
using StructureMap;

namespace Resque.Bootstrap.StructureMap
{
    public static class Bootstrap
    {
        public static ConfigurationExpression DefaultSetup(this ConfigurationExpression cfg)
        {
            cfg.For<IFailureService>().Use<FailureService>();
            cfg.For<IBackendFactory>().Use<RedisBackendFactory>();
            return cfg;
        }
        public static void WithJobCreator<T>(this ConfigurationExpression cfg) where T : ITask
        {
            cfg.For<IJobCreator>().Use<JobCreator<T>>();
        }
        public static void WithReflectionJobCreator(this ConfigurationExpression cfg, Assembly assembly = null)
        {
            cfg.For<IJobCreator>().Use<ReflectionJobCreator>()
                .Ctor<Assembly>().Is(assembly);
        }
    }
}
