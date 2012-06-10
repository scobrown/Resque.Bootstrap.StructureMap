using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using StructureMap;

namespace Resque.Bootstrap.StructureMap
{
    public class ReflectionJobCreator : IJobCreator
    {
        public Assembly JobsAssembly { get; set; }

        public ReflectionJobCreator(Assembly jobsAssembly)
        {
            JobsAssembly = jobsAssembly;
        }

        public IJob CreateJob(IFailureService failureService, Worker worker, QueuedItem deserializedObject, string queue)
        {
            return new Job(this, failureService, worker, deserializedObject, queue);
        }

        public class Job : IJob
        {
            private ReflectionJobCreator Creator { get; set; }
            public IFailureService FailureService { get; private set; }
            public QueuedItem Payload { get; private set; }
            public string Queue { get; private set; }
            public IWorker Worker { get; private set; }

            public Job(ReflectionJobCreator creator, IFailureService failureService, IWorker worker, QueuedItem item,
                       string queue)
            {
                Creator = creator;
                FailureService = failureService;
                Worker = worker;
                Payload = item;
                Queue = queue;
            }

            public void Perform()
            {

                Type type = null;
                if (Creator.JobsAssembly == null)
                    type = ObjectFactory.Model.PluginTypes
                        .Select(t => t.PluginType)
                        .Union(ObjectFactory.Model.PluginTypes.SelectMany(t => t.Instances.Select(x => x.ConcreteType)))
                        .First(t => t.Name.Equals(Payload.@class, StringComparison.CurrentCultureIgnoreCase));
                else
                    type = Creator.JobsAssembly.GetType(Payload.@class, false, true) ??
                            Creator.JobsAssembly.GetLoadedModules(false)
                                .SelectMany(x =>
                                            x.FindTypes((t, o) =>
                                                        t.Name.Equals(Payload.@class,
                                                                      StringComparison.CurrentCultureIgnoreCase),
                                                        null))
                                .First();
                var task = ObjectFactory.GetInstance(type);
                Delegate.CreateDelegate(typeof(Action<string[]>), task, "Perform").DynamicInvoke(new[] {Payload.args});
                return;
                var methodCall = Expression.Call(Expression.Constant(task),
                                "Perform",
                                null,
                                Expression.Constant(Payload.args));

                var lambda = Expression.Lambda<Action>(methodCall, null).Compile();
                lambda();
            }

            public void Failed(Exception exception)
            {
                FailureService.Create(Payload, exception, Worker, Queue);
            }

        }

    }
}
