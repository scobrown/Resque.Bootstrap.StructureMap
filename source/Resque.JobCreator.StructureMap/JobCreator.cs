using System;
using StructureMap;

namespace Resque.Bootstrap.StructureMap
{
    public class JobCreator<T> : IJobCreator where T : ITask
    {

        public IJob CreateJob(IFailureService failureService, Worker worker, QueuedItem deserializedObject, string queue)
        {
            return new Job(failureService, worker, deserializedObject, queue);
        }

        public class Job : IJob
        {
            public IFailureService FailureService { get; private set; }
            public QueuedItem Payload { get; private set; }
            public string Queue { get; private set; }
            public IWorker Worker { get; private set; }

            public Job(IFailureService failureService, IWorker worker, QueuedItem item,
                       string queue)
            {
                FailureService = failureService;
                Worker = worker;
                Payload = item;
                Queue = queue;
            }

            public void Perform()
            {
                var task = ObjectFactory.GetNamedInstance<T>(Payload.@class);
                task.Perform(Payload.args);
            }

            public void Failed(Exception exception)
            {
                FailureService.Create(Payload, exception, Worker, Queue);
            }

        }

    }
}
