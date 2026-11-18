document.addEventListener("DOMContentLoaded", () => {

    const numero = document.getElementById("numeroTarjeta");
    const vencimiento = document.getElementById("vencimientoTarjeta");
    const formulario = document.getElementById("formPagoSimulado");

    // ==========================================
    // FORMATEAR NUMERO DE TARJETA
    // ==========================================

    if (numero instanceof HTMLInputElement) {

        numero.addEventListener("input", () => {

            const digitos = numero.value
                .replace(/\D/g, "")
                .slice(0, 16);

            numero.value = digitos
                .replace(/(\d{4})(?=\d)/g, "$1 ");

        });

    }

    // ==========================================
    // FORMATEAR VENCIMIENTO
    // ==========================================

    if (vencimiento instanceof HTMLInputElement) {

        vencimiento.addEventListener("input", () => {

            const digitos = vencimiento.value
                .replace(/\D/g, "")
                .slice(0, 4);

            vencimiento.value = digitos.length > 2
                ? digitos.slice(0, 2) + "/" + digitos.slice(2)
                : digitos;

        });

    }

    // ==========================================
    // CONFIRMAR COMPRA
    // ==========================================

    if (!(formulario instanceof HTMLFormElement)) {
        return;
    }

    formulario.addEventListener("submit", async (evento) => {

        evento.preventDefault();

        const boton = document.getElementById("botonConfirmarPago");
        const mensaje = document.getElementById("mensajePago");
        const contenido = document.getElementById("contenidoCheckout");
        const resultado = document.getElementById("resultadoCompra");
        const numeroPedido = document.getElementById("numeroPedidoConfirmado");

        if (!(boton instanceof HTMLButtonElement) ||
            !(mensaje instanceof HTMLElement) ||
            !(contenido instanceof HTMLElement) ||
            !(resultado instanceof HTMLElement) ||
            !(numeroPedido instanceof HTMLElement)) {
            return;
        }

        if (!formulario.reportValidity()) {
            return;
        }

        mensaje.textContent = "";
        mensaje.classList.add("d-none");

        boton.disabled = true;
        boton.textContent = "Procesando compra...";

        let compraConfirmada = false;

        try {

            const respuesta = await fetch(formulario.action, {
                method: "POST",
                body: new FormData(formulario),
                credentials: "same-origin",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (respuesta.redirected) {
                window.location.href = respuesta.url;
                return;
            }

            const datos = await respuesta.json();

            if (!respuesta.ok || datos.exito !== true) {

                mensaje.textContent =
                    typeof datos.error === "string"
                        ? datos.error
                        : "No se pudo confirmar la compra.";

                mensaje.classList.remove("d-none");
                return;
            }

            compraConfirmada = true;

            // LIMPIAR EL FORMULARIO
            formulario.reset();

            // MOSTRAR CONFIRMACION
            contenido.classList.add("d-none");

            numeroPedido.textContent =
                "Número de pedido: #" + datos.pedidoId;

            resultado.classList.remove("d-none");

            // ACTUALIZAR CONTADOR DEL CARRITO
            const contador = document.getElementById("contadorCarrito");

            if (contador instanceof HTMLElement) {
                contador.textContent = "0";
                contador.classList.add("d-none");
            }

            window.scrollTo({
                top: 0,
                behavior: "smooth"
            });

        }
        catch (error) {

            mensaje.textContent =
                "Ocurrió un problema al procesar la compra. " +
                "Revisá Mis pedidos antes de volver a intentar.";

            mensaje.classList.remove("d-none");

        }
        finally {

            if (!compraConfirmada) {
                boton.disabled = false;
                boton.textContent = "Confirmar pago";
            }

        }

    });

});