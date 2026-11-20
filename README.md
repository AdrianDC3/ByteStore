
# ⚡ ByteStore

**ByteStore** es una aplicación web de comercio electrónico orientada a la venta de componentes de computación, periféricos y productos de tecnología.

Fue desarrollada como trabajo práctico académico para la materia **Programación en Nuevas Tecnologías 1 (PNT1)**.

La aplicación permite explorar un catálogo de productos, realizar búsquedas, administrar un carrito de compras y registrar pedidos mediante un proceso de pago simulado.

También incluye autenticación de usuarios, autorización por roles y un área administrativa para gestionar productos, stock y ventas.

## 🛠️ Tecnologías utilizadas

- **ASP.NET Core MVC (.NET 10)** y **C#** para el desarrollo del backend.
- **Entity Framework Core 10** para el acceso a datos y la gestión de migraciones.
- **SQL Server** como sistema de gestión de base de datos.
- **SQL Server LocalDB** para el entorno de desarrollo.
- **ASP.NET Core Identity** para la autenticación y autorización de usuarios.
- **Razor y HTML** para la construcción de las vistas.
- **CSS y Bootstrap 5** para el diseño y la adaptación a distintos dispositivos.
- **JavaScript** para las interacciones dinámicas de la interfaz.

## 🛍️ Funcionalidades

### Clientes y visitantes

- Visualización del catálogo de productos con fotografías, precios y disponibilidad.
- Búsqueda de productos por nombre.
- Filtrado de productos por categoría y marca.
- Consulta de información detallada de cada producto.
- Carrito de compras lateral que permite agregar productos, modificar cantidades y eliminarlos.
- Registro e inicio de sesión mediante ventanas modales.
- Confirmación de correo electrónico simulada con fines académicos.
- Finalización de compras mediante un formulario de pago simulado.
- Consulta del historial de pedidos.
- Visualización del estado actualizado de cada pedido.

### Administradores

- Acceso a funcionalidades protegidas mediante el rol `Administrador`.
- Consulta del listado de productos.
- Búsqueda y filtrado de productos por nombre, categoría y marca.
- Alta de nuevos productos.
- Modificación de productos existentes.
- Gestión de precios y stock.
- Activación y desactivación de productos.
- Eliminación definitiva de productos sin ventas registradas.
- Consulta de pedidos realizados por los clientes.
- Visualización de productos, cantidades, importes y datos identificatorios del cliente.
- Modificación del estado de los pedidos.

## 🏗️ Arquitectura del proyecto

ByteStore utiliza el patrón de arquitectura **Modelo–Vista–Controlador (MVC)**, separando las responsabilidades de presentación, procesamiento de solicitudes y acceso a datos.

### Organización principal

- **Models:** contiene las entidades utilizadas por el sistema, como productos, categorías, marcas y pedidos.

- **Views:** contiene las interfaces desarrolladas mediante Razor y Bootstrap.

- **Controllers:** recibe las solicitudes HTTP, coordina las operaciones correspondientes y devuelve vistas o respuestas.

- **Services:** concentra operaciones y reglas de negocio relacionadas con productos, carrito de compras y pedidos.

- **Data:** contiene `ApplicationDbContext`, responsable de la integración con Entity Framework Core y la base de datos, además de las migraciones.

- **Areas/Identity:** contiene las páginas y funcionalidades de ASP.NET Core Identity utilizadas para administrar las cuentas.

- **wwwroot:** almacena los archivos estáticos de la aplicación, incluyendo CSS, JavaScript, fotografías y logos.

- **ScriptsSQL:** contiene scripts utilizados para preparar y cargar datos del catálogo.

### Organización del frontend

La interfaz utiliza vistas Razor y componentes parciales reutilizables para evitar duplicación de código.

Los archivos se encuentran organizados según su responsabilidad:

- **Razor/HTML:** estructura y contenido de las páginas.
- **CSS:** estilos visuales personalizados.
- **JavaScript:** interacciones dinámicas y comunicación con los controladores.
- **Bootstrap 5:** componentes visuales y diseño responsive.

Entre las funcionalidades dinámicas se encuentran el carrito lateral, los formularios modales de autenticación y la actualización de cantidades del carrito.

## 🗄️ Base de datos

ByteStore utiliza **Entity Framework Core con SQL Server** para almacenar y administrar la información.

Las principales entidades del sistema son:

| Entidad | Función |
|---|---|
| `Producto` | Almacena nombre, descripción, precio, stock y estado de publicación. |
| `Categoria` | Permite clasificar los productos. |
| `Marca` | Representa la marca de cada producto. |
| `Pedido` | Registra las compras realizadas, su fecha, importe total y estado. |
| `DetallePedido` | Almacena los productos, cantidades y precios unitarios correspondientes a cada pedido. |

La información de usuarios, contraseñas y roles se administra mediante las tablas proporcionadas por **ASP.NET Core Identity**.

### Catálogo

El catálogo inicial utilizado durante el desarrollo cuenta con **44 productos**, organizados en diferentes categorías.

Cada producto dispone de una fotografía almacenada localmente en:

`wwwroot/images/productos/`

### Migraciones

La estructura de la base de datos se administra mediante las migraciones de Entity Framework Core.

Esto permite mantener sincronizadas las entidades del proyecto con las tablas de SQL Server.

## 🛒 Proceso de compra

El proceso de compra de ByteStore funciona de la siguiente manera:

1. El visitante ingresa al catálogo y explora los productos disponibles.
2. Puede buscar productos, aplicar filtros y consultar sus características.
3. Agrega artículos al carrito lateral.
4. Modifica cantidades o elimina productos según sus necesidades.
5. Para finalizar la compra, debe iniciar sesión con una cuenta registrada.
6. El sistema verifica nuevamente la disponibilidad y el stock de los productos.
7. El cliente completa un formulario con datos ficticios de una tarjeta.
8. El sistema registra el pedido y sus detalles en la base de datos.
9. Se descuenta el stock correspondiente y se vacía el carrito.
10. El cliente puede consultar la compra desde la sección **Mis pedidos**.
11. El administrador puede visualizar el pedido y modificar su estado.

### Pago simulado

El proceso de pago fue desarrollado exclusivamente para fines académicos.

El formulario permite simular una operación utilizando datos ficticios de una tarjeta.

**No se realizan cobros reales ni se almacenan los datos de las tarjetas ingresadas.**

## 🔐 Seguridad

ByteStore incorpora mecanismos de seguridad mediante **ASP.NET Core Identity** y las funcionalidades de ASP.NET Core.

### Autenticación

Los usuarios pueden registrarse, iniciar sesión y cerrar sesión.

ASP.NET Core Identity se encarga de gestionar las credenciales y el almacenamiento seguro de las contraseñas.

### Autorización

El sistema distingue dos tipos de usuarios:

- **Cliente:** puede realizar compras y consultar sus propios pedidos.
- **Administrador:** puede gestionar productos, stock y pedidos de los clientes.

Las funcionalidades administrativas están protegidas mediante autorización basada en roles.

Los usuarios registrados desde el formulario público no reciben automáticamente permisos de administración.

El rol `Administrador` debe asignarse de manera controlada.

### Validaciones y protección de datos

El proyecto incorpora:

- Validaciones de los datos ingresados en formularios.
- Protección contra solicitudes no autorizadas.
- Validación del stock disponible antes de confirmar pedidos.
- Protección antifalsificación en operaciones mediante formularios POST.
- Restricciones de acceso a funcionalidades administrativas.
- Consultas de pedidos asociadas al usuario autenticado.

### Integridad de las compras

El registro de pedidos utiliza una transacción de base de datos.

Esto permite realizar de manera conjunta el registro de la compra y la actualización del stock.

Si la operación no puede completarse correctamente, la transacción evita que los cambios queden aplicados parcialmente.

## ▶️ Ejecución local

### Requisitos

- SDK de **.NET 10**.
- Visual Studio con herramientas de desarrollo ASP.NET Core, o un entorno equivalente.
- SQL Server LocalDB o una instancia compatible de SQL Server.

### Instalación

**1. Clonar el repositorio**

```bash
git clone https://github.com/AdrianDC3/ByteStore.git
```

**2. Abrir el proyecto**

Abrir `ByteStore.csproj` desde Visual Studio.

**3. Restaurar dependencias**

Restaurar los paquetes NuGet utilizados por el proyecto.

**4. Configurar la base de datos**

Revisar la cadena de conexión `DefaultConnection` del archivo `appsettings.json`.

La cadena debe apuntar a una instancia de SQL Server disponible en el entorno local.

**5. Aplicar las migraciones**

Desde la Consola del Administrador de paquetes de Visual Studio, ejecutar:

```powershell
Update-Database
```

**6. Preparar los datos iniciales**

Cargar las categorías y marcas requeridas por el catálogo y, posteriormente, ejecutar el script de productos incluido en `ScriptsSQL`.

El script de productos requiere que las categorías y marcas correspondientes ya existan en la base de datos.

**7. Ejecutar ByteStore**

Iniciar la aplicación desde Visual Studio.

### Consideraciones

La base de datos local utilizada durante el desarrollo no está incluida directamente en el repositorio.

Las tablas se generan mediante las migraciones de Entity Framework Core.

Para utilizar las funcionalidades administrativas es necesario contar con un usuario al que se le haya asignado el rol `Administrador`.

## 📱 Diseño responsive

ByteStore utiliza Bootstrap 5 y estilos CSS personalizados para adaptar su interfaz a diferentes tamaños de pantalla.

Su diseño está basado en una combinación de tonos oscuros y verdes, con una identidad visual representada por un rayo.

La interfaz incluye:

- Barra de navegación adaptable.
- Catálogo organizado mediante tarjetas de productos.
- Fotografías de los artículos.
- Carrito lateral.
- Formularios de autenticación mediante ventanas modales.
- Paneles de administración.
- Tablas adaptables para visualizar productos y pedidos.

## 👨‍💻 Autor

**Adrián Díaz Casas**

Proyecto académico desarrollado para la materia **Programación de Nuevas Tecnologías 1 (PNT1)**.
