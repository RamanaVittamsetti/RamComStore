using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RamCom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("ManageProducts")]
    [Authorize(Roles ="Admin")]
    public class ProductManagementController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
