using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ByteStore.Controllers
{
    public class LoginModalController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginModalController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(
            [FromForm(Name = "Input.Email")] string? email,
            [FromForm(Name = "Input.Password")] string? password,
            [FromForm(Name = "Input.RememberMe")] bool recordar)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "Completá el correo electrónico y la contraseña."
                });
            }

            var usuario = await _userManager.FindByEmailAsync(
                email.Trim());

            if (usuario == null)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "El correo electrónico o la contraseña son incorrectos."
                });
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario,
                password,
                recordar,
                lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                return Json(new
                {
                    exito = true,
                    mensaje = "Inicio de sesión correcto."
                });
            }

            if (resultado.IsLockedOut)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "La cuenta está bloqueada temporalmente por varios intentos fallidos."
                });
            }

            if (resultado.IsNotAllowed)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "No se pudo iniciar sesión. Verificá que tu cuenta esté confirmada."
                });
            }

            if (resultado.RequiresTwoFactor)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "Esta cuenta requiere una verificación adicional. Utilizá el inicio de sesión completo."
                });
            }

            return Json(new
            {
                exito = false,
                mensaje = "El correo electrónico o la contraseña son incorrectos."
            });
        }
    }
}
