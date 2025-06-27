using Business.Interfaces;
using RamCom.Areas.Account.Models;
using RamCom.Interfaces;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RamCom.Areas.Account.ModelBuilder
{
    public class RegisterViewModelBuilder : IRegisterViewModelBuilder
    {
        private readonly IXmlCacheHelper _xmlCacheHelper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _config;

        public RegisterViewModelBuilder(IXmlCacheHelper xmlCacheHelper, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _xmlCacheHelper = xmlCacheHelper;
            _webHostEnvironment = webHostEnvironment;
            _config = config;
        }

        public RegisterViewModel BuildViewModel(RegisterViewModel model = null)
        {
           if(model == null)
            {
                model = new RegisterViewModel();
            }

            model.UserRoles = getRoles;

            return model;
        }

        private List<SelectListItem> getRoles
        {
            get
            {
                var rolesXmlPath = Path.Combine(_webHostEnvironment.WebRootPath, _config["XmlFiles:UserRole"] ?? string.Empty);
                XDocument document = _xmlCacheHelper.RetriveXmlValues("UserRoles", rolesXmlPath);
                return document.Descendants("Role").Select(x => new SelectListItem
                {
                    Text = x.Value,
                    Value = x.Value

                }).ToList();
            }
        }
    }
}
