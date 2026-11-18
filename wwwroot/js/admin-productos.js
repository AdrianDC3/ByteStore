document.addEventListener("DOMContentLoaded", () => {

    // ==========================================
    // FILTROS AUTOMATICOS
    // ==========================================

    const formulario = document.getElementById("formFiltros");
    const categoria = document.getElementById("categoriaId");
    const marca = document.getElementById("marcaId");

    if (formulario instanceof HTMLFormElement) {

        [categoria, marca].forEach(select => {

            if (select instanceof HTMLSelectElement) {

                select.addEventListener("change", () => {
                    formulario.requestSubmit();
                });

            }

        });

    }

    // ==========================================
    // CONFIRMACIONES DE ACCIONES
    // ==========================================

    document.querySelectorAll("form[data-confirm]")
        .forEach(formulario => {

            formulario.addEventListener("submit", evento => {

                const mensaje = formulario.dataset.confirm;

                if (mensaje && !window.confirm(mensaje)) {
                    evento.preventDefault();
                }

            });

        });

});