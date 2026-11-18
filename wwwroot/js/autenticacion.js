document.addEventListener("DOMContentLoaded", () => {

    // ==========================================
    // ABRIR MODALES DESDE LA NAVEGACION
    // ==========================================

    const enlacesLogin = document.querySelectorAll(
        'a[href*="/Identity/Account/Login"]'
    );

    const enlacesRegistro = document.querySelectorAll(
        'a[href*="/Identity/Account/Register"]'
    );

    enlacesLogin.forEach(enlace => {
        enlace.setAttribute("data-bs-toggle", "modal");
        enlace.setAttribute("data-bs-target", "#modalLogin");
    });

    enlacesRegistro.forEach(enlace => {
        enlace.setAttribute("data-bs-toggle", "modal");
        enlace.setAttribute("data-bs-target", "#modalRegistro");
    });


    // ==========================================
    // FUNCION AUXILIAR PARA ENVIAR FORMULARIOS
    // ==========================================

    async function enviarFormulario(formulario) {

        const respuesta = await fetch(formulario.action, {
            method: "POST",
            body: new FormData(formulario),
            credentials: "same-origin",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        });

        if (!respuesta.ok) {
            throw new Error("Error en el servidor");
        }

        return await respuesta.json();
    }


    // ==========================================
    // INICIO DE SESION
    // ==========================================

    const formularioLogin = document.getElementById("formLoginModal");
    const modalLogin = document.getElementById("modalLogin");
    const errorLogin = document.getElementById("errorLoginModal");
    const botonLogin = document.getElementById("botonLoginModal");

    if (modalLogin instanceof HTMLElement) {

        modalLogin.addEventListener("show.bs.modal", () => {

            if (errorLogin instanceof HTMLElement) {
                errorLogin.textContent = "";
                errorLogin.classList.add("d-none");
            }

        });
    }

    if (formularioLogin instanceof HTMLFormElement &&
        errorLogin instanceof HTMLElement &&
        botonLogin instanceof HTMLButtonElement) {

        formularioLogin.addEventListener("submit", async evento => {

            evento.preventDefault();

            errorLogin.textContent = "";
            errorLogin.classList.add("d-none");

            botonLogin.disabled = true;
            botonLogin.textContent = "Iniciando sesión...";

            try {

                const resultado = await enviarFormulario(formularioLogin);

                if (resultado.exito === true) {
                    window.location.reload();
                    return;
                }

                errorLogin.textContent =
                    typeof resultado.mensaje === "string"
                        ? resultado.mensaje
                        : "No se pudo iniciar sesión.";

                errorLogin.classList.remove("d-none");

            }
            catch (error) {

                errorLogin.textContent =
                    "No se pudo procesar el inicio de sesión. Intentá nuevamente.";

                errorLogin.classList.remove("d-none");

            }
            finally {

                botonLogin.disabled = false;
                botonLogin.textContent = "Iniciar sesión";

            }

        });
    }


    // ==========================================
    // REGISTRO DE USUARIO
    // ==========================================

    const formularioRegistro = document.getElementById("formRegistroModal");
    const mensajeRegistro = document.getElementById("mensajeRegistro");
    const formularioConfirmacion = document.getElementById("formConfirmarCorreo");

    if (formularioRegistro instanceof HTMLFormElement &&
        mensajeRegistro instanceof HTMLElement &&
        formularioConfirmacion instanceof HTMLFormElement) {

        formularioRegistro.addEventListener("submit", async evento => {

            evento.preventDefault();

            const boton = formularioRegistro.querySelector(
                'button[type="submit"]'
            );

            if (!(boton instanceof HTMLButtonElement)) {
                return;
            }

            mensajeRegistro.className = "alert d-none";
            mensajeRegistro.textContent = "";

            boton.disabled = true;
            boton.textContent = "Creando cuenta...";

            try {

                const resultado = await enviarFormulario(formularioRegistro);

                mensajeRegistro.textContent =
                    typeof resultado.mensaje === "string"
                        ? resultado.mensaje
                        : "No se pudo completar el registro.";

                if (resultado.exito === true) {

                    mensajeRegistro.className = "alert alert-success";

                    formularioRegistro.classList.add("d-none");
                    formularioConfirmacion.classList.remove("d-none");

                }
                else {

                    mensajeRegistro.className = "alert alert-danger";

                }

            }
            catch (error) {

                mensajeRegistro.textContent =
                    "Ocurrió un problema al registrar la cuenta.";

                mensajeRegistro.className = "alert alert-danger";

            }
            finally {

                boton.disabled = false;
                boton.textContent = "Crear cuenta";

            }

        });
    }


    // ==========================================
    // CONFIRMACION SIMULADA DEL CORREO
    // ==========================================

    const formularioConfirmar = document.getElementById("formConfirmarCorreo");
    const registroFinalizado = document.getElementById("registroFinalizado");

    if (formularioConfirmar instanceof HTMLFormElement &&
        mensajeRegistro instanceof HTMLElement &&
        registroFinalizado instanceof HTMLElement) {

        formularioConfirmar.addEventListener("submit", async evento => {

            evento.preventDefault();

            const boton = formularioConfirmar.querySelector(
                'button[type="submit"]'
            );

            if (!(boton instanceof HTMLButtonElement)) {
                return;
            }

            boton.disabled = true;
            boton.textContent = "Confirmando...";

            try {

                const resultado = await enviarFormulario(formularioConfirmar);

                mensajeRegistro.textContent =
                    typeof resultado.mensaje === "string"
                        ? resultado.mensaje
                        : "No se pudo confirmar el correo.";

                if (resultado.exito === true) {

                    mensajeRegistro.className = "alert alert-success";

                    formularioConfirmar.classList.add("d-none");
                    registroFinalizado.classList.remove("d-none");

                }
                else {

                    mensajeRegistro.className = "alert alert-danger";

                }

            }
            catch (error) {

                mensajeRegistro.textContent =
                    "Ocurrió un problema al confirmar el correo.";

                mensajeRegistro.className = "alert alert-danger";

            }
            finally {

                boton.disabled = false;
                boton.textContent = "Confirmar correo (simulación)";

            }

        });
    }

});