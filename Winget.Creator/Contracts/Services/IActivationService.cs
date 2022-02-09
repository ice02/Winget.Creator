using System.Threading.Tasks;

namespace Winget.Creator.Contracts.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
