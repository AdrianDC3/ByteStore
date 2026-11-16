
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

    // ACTUALIZAR CONTADOR DEL CARRITO
    async function actualizarContadorCarrito() {

        if (!(contador instanceof HTMLElement)) {
            return;
        }

        try {

            const respuesta = await fetch("/Carrito/Cantidad");

            if (!respuesta.ok) {
                throw new Error(
                    "No se pudo consultar la cantidad del carrito."
                );
            }

            const datos = await respuesta.json();
            const cantidad = datos.cantidad;

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

    // CARGAR CONTENIDO DEL CARRITO
    async function cargarCarrito() {

        const respuesta = await fetch("/Carrito/Contenido");

        if (!respuesta.ok) {
            throw new Error(
                "No se pudo cargar el carrito."
            );
        }

        contenido.innerHTML = await respuesta.text();

        await actualizarContadorCarrito();
    }

    // ENVIAR FORMULARIOS DEL CARRITO
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

    // ABRIR CARRITO LATERAL
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

    // IR AL CHECKOUT DESDE EL CARRITO LATERAL
    document.addEventListener("submit", (evento) => {

        const formulario = evento.target;

        if (!(formulario instanceof HTMLFormElement)) {
            return;
        }

        // IMPORTANTE:
        // Solo interceptamos formularios del carrito lateral.
        // El formulario de pago del checkout queda excluido.
        if (!panel.contains(formulario)) {
            return;
        }

        const url = new URL(formulario.action);

        if (url.pathname.toLowerCase() !== "/pedidos/confirmar") {
            return;
        }

        evento.preventDefault();

        // No registramos el pedido desde el carrito.
        // Primero mostramos la pantalla de pago.
        carritoLateral.hide();

        window.location.href = "/Pedidos/Checkout";

    });

    // AGREGAR, ACTUALIZAR Y ELIMINAR PRODUCTOS
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

    // CONSULTAR LA CANTIDAD AL CARGAR CUALQUIER PAGINA
    actualizarContadorCarrito();

});



