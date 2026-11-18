
document.addEventListener("change", (evento) => {

    const campo = evento.target;

    if (!(campo instanceof HTMLInputElement)) {
        return;
    }

    if (!campo.matches(
        '#carritoLateral form[action$="/Carrito/Actualizar"] input[name="cantidad"]'
    )) {
        return;
    }

    if (!campo.checkValidity()) {
        campo.reportValidity();
        return;
    }

    if (campo.form instanceof HTMLFormElement) {
        campo.form.requestSubmit();
    }
});


document.addEventListener("DOMContentLoaded", () => {

    const panel = document.getElementById("carritoLateral");
    const contenido = document.getElementById("contenidoCarrito");
    const contador = document.getElementById("contadorCarrito");

    if (!(panel instanceof HTMLElement) ||
        !(contenido instanceof HTMLElement)) {
        return;
    }

    const carritoLateral =
        bootstrap.Offcanvas.getOrCreateInstance(panel);

    let verificandoCheckout = false;

    // =====================================================
    // ACTUALIZAR CONTADOR DEL CARRITO
    // =====================================================

    async function actualizarContadorCarrito() {

        if (!(contador instanceof HTMLElement)) {
            return;
        }

        try {

            const respuesta = await fetch("/Carrito/Cantidad", {
                credentials: "same-origin"
            });

            if (!respuesta.ok) {
                throw new Error(
                    "No se pudo consultar la cantidad del carrito."
                );
            }

            const datos = await respuesta.json();
            const cantidad = Number(datos.cantidad) || 0;

            contador.textContent = String(cantidad);

            contador.classList.toggle(
                "d-none",
                cantidad === 0
            );

        }
        catch (error) {

            console.error(
                "Error al actualizar el contador:",
                error
            );

        }
    }

    // =====================================================
    // CARGAR CONTENIDO DEL CARRITO
    // =====================================================

    async function cargarCarrito() {

        const respuesta = await fetch("/Carrito/Contenido", {
            credentials: "same-origin"
        });

        if (!respuesta.ok) {
            throw new Error(
                "No se pudo cargar el carrito."
            );
        }

        contenido.innerHTML = await respuesta.text();

        await actualizarContadorCarrito();
    }

    // =====================================================
    // ENVIAR FORMULARIOS DEL CARRITO
    // =====================================================

    async function enviarFormulario(formulario) {

        const respuesta = await fetch(formulario.action, {
            method: "POST",
            body: new FormData(formulario),
            credentials: "same-origin"
        });

        if (!respuesta.ok) {

            let mensaje = "Ocurrió un error.";

            try {

                const datos = await respuesta.json();

                mensaje = datos.error || mensaje;

            }
            catch {
                // La respuesta no contiene un error JSON.
            }

            throw new Error(mensaje);
        }

        return respuesta;
    }

    // =====================================================
    // ABRIR MODAL DE LOGIN
    // =====================================================

    function abrirModalLogin() {

        const modalLogin = document.getElementById("modalLogin");

        if (!(modalLogin instanceof HTMLElement)) {
            throw new Error(
                "No se encontró el modal de inicio de sesión."
            );
        }

        const modal =
            bootstrap.Modal.getOrCreateInstance(modalLogin);

        // Esperamos a que termine de cerrarse el carrito
        // para evitar superponer dos componentes de Bootstrap.

        if (panel.classList.contains("show")) {

            panel.addEventListener(
                "hidden.bs.offcanvas",
                () => {
                    modal.show();
                },
                { once: true }
            );

            carritoLateral.hide();

        }
        else {

            modal.show();

        }
    }

    // =====================================================
    // VERIFICAR AUTENTICACION PARA EL CHECKOUT
    // =====================================================

    async function irAlCheckout() {

        if (verificandoCheckout) {
            return;
        }

        verificandoCheckout = true;

        try {

            // Consultamos la pagina protegida del checkout.
            // Identity determina si la sesion es valida.
            //
            // No guardamos credenciales en JavaScript,
            // ni reemplazamos la autorizacion del servidor.

            const respuesta = await fetch("/Pedidos/Checkout", {
                method: "GET",
                credentials: "same-origin",
                redirect: "follow",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const urlFinal = new URL(respuesta.url);

            const rutaFinal = urlFinal.pathname.toLowerCase();

            // Identity puede redirigir al login predeterminado
            // cuando un visitante no esta autenticado.

            const redirigidoAlLogin =
                rutaFinal.includes("/identity/account/login") ||
                rutaFinal === "/account/login";

            const requiereAutenticacion =
                redirigidoAlLogin ||
                respuesta.status === 401;

            if (requiereAutenticacion) {

                // En lugar de mostrar la pagina antigua,
                // abrimos el modal que ya tiene ByteStore.

                abrirModalLogin();
                return;
            }

            if (!respuesta.ok) {

                throw new Error(
                    "No se pudo acceder a la pantalla de compra."
                );

            }

            // El servidor permitio acceder al checkout.
            // Navegamos normalmente a la pantalla de pago.

            carritoLateral.hide();

            window.location.href = "/Pedidos/Checkout";

        }
        catch (error) {

            console.error(
                "Error al comprobar el acceso al checkout:",
                error
            );

            alert(
                error instanceof Error
                    ? error.message
                    : "No se pudo continuar con la compra."
            );

        }
        finally {

            verificandoCheckout = false;

        }
    }

    // =====================================================
    // ABRIR CARRITO LATERAL
    // =====================================================

    panel.addEventListener("show.bs.offcanvas", async () => {

        try {

            await cargarCarrito();

        }
        catch (error) {

            contenido.textContent =
                error instanceof Error
                    ? error.message
                    : "No se pudo cargar el carrito.";

        }

    });

    // =====================================================
    // FINALIZAR COMPRA DESDE EL CARRITO
    // =====================================================

    document.addEventListener("submit", (evento) => {

        const formulario = evento.target;

        if (!(formulario instanceof HTMLFormElement)) {
            return;
        }

        // Solo interceptamos formularios del carrito lateral.
        // No interferimos con el formulario de pago
        // que existe dentro de Pedidos/Checkout.

        if (!panel.contains(formulario)) {
            return;
        }

        const url = new URL(formulario.action);

        if (url.pathname.toLowerCase() !== "/pedidos/confirmar") {
            return;
        }

        evento.preventDefault();

        // No confirmamos el pedido desde el carrito.
        // Primero verificamos el acceso al checkout.

        irAlCheckout();

    });

    // =====================================================
    // AGREGAR, ACTUALIZAR Y ELIMINAR PRODUCTOS
    // =====================================================

    document.addEventListener("submit", async (evento) => {

        const formulario = evento.target;

        if (!(formulario instanceof HTMLFormElement)) {
            return;
        }

        const acciones = [
            "/carrito/agregar",
            "/carrito/actualizar",
            "/carrito/eliminar"
        ];

        const url = new URL(formulario.action);
        const accionActual = url.pathname.toLowerCase();

        if (!acciones.includes(accionActual)) {
            return;
        }

        evento.preventDefault();

        const boton = formulario.querySelector(
            'button[type="submit"]'
        );

        if (boton instanceof HTMLButtonElement) {
            boton.disabled = true;
        }

        try {

            await enviarFormulario(formulario);

            await cargarCarrito();

            if (accionActual === "/carrito/agregar") {
                carritoLateral.show();
            }

        }
        catch (error) {

            alert(
                error instanceof Error
                    ? error.message
                    : "Ocurrió un error al actualizar el carrito."
            );

        }
        finally {

            if (boton instanceof HTMLButtonElement &&
                boton.isConnected) {

                boton.disabled = false;

            }

        }

    });

    // =====================================================
    // CONSULTAR CARRITO AL CARGAR LA PAGINA
    // =====================================================

    actualizarContadorCarrito();

});
