using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Identity;
using Business.Models;
using Microsoft.Extensions.Logging;
using RamCom.Areas.Account.Controllers;
using RamCom.Interfaces;
using Microsoft.AspNetCore.Mvc;
using RamCom.Areas.Account.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RamCom.Test.Areas.Account
{
    [TestFixture]
    public class AccountControllerTests : IDisposable
    {
        private Mock<UserManager<ApplicationUser>> _userManagerMock;
        private Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private Mock<ILogger<AccountController>> _loggerMock;
        private Mock<IRegisterViewModelBuilder> _registerBuilderMock;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null
            );
            _loggerMock = new Mock<ILogger<AccountController>>();
            _registerBuilderMock = new Mock<IRegisterViewModelBuilder>();

            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _loggerMock.Object,
                _registerBuilderMock.Object
            );
        }

        [Test]
        public void Login_Get_ReturnsViewWithModel()
        {
            var result = _controller.Login();
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOf<LoginViewModel>(viewResult.Model);
        }

        [Test]
        public async Task Login_Post_InvalidModelState_ReturnsView()
        {
            _controller.ModelState.AddModelError("UserName", "Required");
            var model = new LoginViewModel();
            var result = await _controller.Login(model);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
        }

        [Test]
        public async Task Login_Post_ValidCredentials_RedirectsToHome()
        {
            var model = new LoginViewModel { UserName = "user", Password = "Password1!" };
            var user = new ApplicationUser { UserName = "user" };
            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.PasswordSignInAsync(user, model.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.Login(model);

            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("Home", redirect.ControllerName);
        }

        [Test]
        public async Task Login_Post_InvalidCredentials_ReturnsViewWithModelError()
        {
            var model = new LoginViewModel { UserName = "user", Password = "Password1!" };
            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync((ApplicationUser)null);

            var result = await _controller.Login(model);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
            Assert.IsTrue(_controller.ModelState.ContainsKey("invalid_credentials"));
        }

        [Test]
        public async Task SignOut_CallsSignOutAndRedirects()
        {
            _signInManagerMock.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            var result = await _controller.SignOut();

            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Login", redirect.ActionName);
        }

        [Test]
        public void AccessDenied_ReturnsViewWithLoginViewModel()
        {
            var result = _controller.AccessDenied();
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOf<LoginViewModel>(viewResult.Model);
        }

        [Test]
        public void Register_Get_ReturnsRegisterOrUpdateView()
        {
            var viewModel = new RegisterViewModel();
            _registerBuilderMock.Setup(x => x.BuildViewModel(null)).Returns(viewModel);

            var result = _controller.Register();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("RegisterOrUpdate", viewResult.ViewName);
            Assert.AreEqual(viewModel, viewResult.Model);
        }

        [Test]
        public async Task UpdateDetails_UserExists_ReturnsRegisterOrUpdateView()
        {
            var user = new ApplicationUser
            {
                UserName = "user",
                FirstName = "First",
                LastName = "Last",
                Email = "email@test.com",
                PhoneNumber = "1234567890",
                Role = "Admin"
            };
            var viewModel = new RegisterViewModel();

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
 
            _registerBuilderMock.Setup(x => x.BuildViewModel(It.IsAny<RegisterViewModel>())).Returns(viewModel);

            var result = await _controller.UpdateDetails();

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual("RegisterOrUpdate", viewResult.ViewName);
            Assert.AreEqual(viewModel, viewResult.Model);
        }

        [Test]
        public async Task UpdateDetails_UserNotFound_RedirectsToSignOut()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null);
           

            var result = await _controller.UpdateDetails();

            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("SignOut", redirect.ActionName);
        }

        [Test]
        public async Task Register_Post_CreateUser_Success_RedirectsToLogin()
        {
            var model = new RegisterViewModel
            {
                UserName = "user",
                Email = "email@test.com",
                FirstName = "First",
                LastName = "Last",
                PhoneNumber = "1234567890",
                Role = "Admin",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                IsUpdateMode = false
            };
            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((ApplicationUser)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), model.Role))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(model);

            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Login", redirect.ActionName);
        }

        [Test]
        public async Task Register_Post_UpdateUser_Success_RedirectsToHome()
        {
            var model = new RegisterViewModel
            {
                UserName = "user",
                FirstName = "First",
                LastName = "Last",
                PhoneNumber = "1234567890",
                Role = "Admin",
                Password = "Password1!",
                ConfirmPassword = "Password1!",
                IsUpdateMode = true,
                CurrentPassword = "OldPassword1!"
            };
            var user = new ApplicationUser { UserName = "user" };
            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.ChangePasswordAsync(user, model.CurrentPassword, model.Password))
                .ReturnsAsync(IdentityResult.Success);
            _signInManagerMock.Setup(x => x.RefreshSignInAsync(user)).Returns(Task.CompletedTask);

            var result = await _controller.Register(model);

            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("Home", redirect.ControllerName);
        }

        [Test]
        public async Task Register_Post_InvalidModelState_ReturnsView()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var model = new RegisterViewModel();
            _registerBuilderMock.Setup(x => x.BuildViewModel(model)).Returns(model);

            var result = await _controller.Register(model);

            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.AreEqual(model, viewResult.Model);
        }

        [TearDown]
        public void Dispose()
        {
            _controller?.Dispose();
        }

    }
}