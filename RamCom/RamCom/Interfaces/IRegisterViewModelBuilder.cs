using Microsoft.AspNetCore.Mvc.Rendering;
using RamCom.Areas.Account.Models;

namespace RamCom.Interfaces
{
    public interface IRegisterViewModelBuilder
    {
        RegisterViewModel BuildViewModel(RegisterViewModel model = null);
    }
}
