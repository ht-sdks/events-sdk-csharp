namespace Hightouch.Events.Sovran
{
    public interface IAction
    {
        IState Reduce(IState state);
    }
}
