using System.Threading.Tasks;

namespace Winget.Creator.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);

        Task HandleAsync(object args);
    }
}
