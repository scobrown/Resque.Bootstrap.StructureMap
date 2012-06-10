namespace Resque.Bootstrap.StructureMap
{
    public interface ITask
    {
        void Perform(params string[] args);
    }
}