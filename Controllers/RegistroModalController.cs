using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ByteStore.Controllers
{
    public class RegistroModalController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        private const string UsuarioPendiente =
            "RegistroModal_UsuarioPendiente";

        public RegistroModalController(
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            RegistroModalViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    exito = false,
                    mensaje = string.Join(" ", errores)
                });
            }

            string correo = modelo.Email.Trim();

            var usuarioExistente =
                await _userManager.FindByEmailAsync(correo);

            if (usuarioExistente != null)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "Ya existe una cuenta con ese correo electrónico."
                });
            }

            var usuario = new IdentityUser
            {
                UserName = correo,
                Email = correo,
                EmailConfirmed = false
            };

            var resultado = await _userManager.CreateAsync(
                usuario,
                modelo.Password);

            if (!resultado.Succeeded)
            {
                var errores = resultado.Errors
                    .Select(e => e.Description);

                return Json(new
                {
                    exito = false,
                    mensaje = string.Join(" ", errores)
                });
            }

            // Guardamos temporalmente el usuario pendiente
            // en la sesión de este navegador.
            HttpContext.Session.SetString(
                UsuarioPendiente,
                usuario.Id);

            return Json(new
            {
                exito = true,
                mensaje = "Cuenta creada correctamente. Confirmá tu correo para continuar."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCorreo()
        {
            string? usuarioId = HttpContext.Session.GetString(
                UsuarioPendiente);

            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "No hay un registro pendiente de confirmación."
                });
            }

            var usuario = await _userManager.FindByIdAsync(
                usuarioId);

            if (usuario == null)
            {
                HttpContext.Session.Remove(UsuarioPendiente);

                return Json(new
                {
                    exito = false,
                    mensaje = "No se encontró la cuenta registrada."
                });
            }

            if (await _userManager.IsEmailConfirmedAsync(usuario))
            {
                HttpContext.Session.Remove(UsuarioPendiente);

                return Json(new
                {
                    exito = true,
                    mensaje = "La cuenta ya estaba confirmada."
                });
            }

            // Confirmación simulada:
            // generamos y utilizamos el token de Identity
            // sin enviar ningún correo electrónico.
            string token =
                await _userManager.GenerateEmailConfirmationTokenAsync(
                    usuario);

            var resultado = await _userManager.ConfirmEmailAsync(
                usuario,
                token);

            if (!resultado.Succeeded)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "No se pudo confirmar la cuenta."
                });
            }

            HttpContext.Session.Remove(UsuarioPendiente);

            return Json(new
            {
                exito = true,
                mensaje = "Correo confirmado correctamente. Ya podés iniciar sesión."
            });
        }
    }

    public class RegistroModalViewModel
    {
        [Required(ErrorMessage = "Ingresá tu correo electrónico.")]
        [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresá una contraseña.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmá tu contraseña.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

