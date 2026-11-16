# ⚡ ByteStore

**ByteStore** es una aplicación web de comercio electrónico orientada a la venta de componentes de computación, periféricos y productos de tecnología. Fue desarrollada como trabajo práctico académico para la materia **Programación en Nuevas Tecnologías 1 (PNT1)**.

La aplicación permite explorar un catálogo, buscar y filtrar productos, administrar un carrito de compras y registrar pedidos. También incluye autenticación de usuarios y un área de administración para gestionar productos, stock y ventas.

## 🛠️ Tecnologías utilizadas

- **ASP.NET Core MVC (.NET 10)** y **C#** para la lógica del servidor y la organización de la aplicación.
- **Entity Framework Core 10** para el acceso a datos y las migraciones.
- **SQL Server / LocalDB** como base de datos de desarrollo.
- **ASP.NET Core Identity** para el registro, inicio de sesión y control de acceso por roles.
- **Razor**, **HTML**, **CSS**, **JavaScript** y **Bootstrap 5** para la interfaz de usuario.

## 🛍️ Funcionalidades

### Clientes y visitantes

- Visualización del catálogo de productos con fotografías, precios y stock.
- Búsqueda por nombre y filtros por categoría y marca.
- Acceso a la ficha de detalle de cada producto.
- Carrito de compras con opciones para agregar, modificar cantidades y quitar productos.
- Registro e inicio de sesión mediante ventanas modales.
- Confirmación de correo **simulada** con fines académicos.
- Confirmación de pedidos mediante un **formulario de pago simulado**: no se procesan cobros reales ni se almacenan datos de tarjetas.
- Consulta del historial de pedidos y sus estados.

### Administradores

- Acceso a funciones administrativas protegido mediante el rol `Administrador`.
- Alta, edición, desactivación y reactivación de productos.
- Gestión de stock.
- Eliminación de productos cuando no tienen registros de detalle de pedido asociados.
- Consulta de pedidos y de sus productos, cantidades e importes.
- Actualización del estado de los pedidos.

## 🏗️ Arquitectura del proyecto

ByteStore sigue el patrón **Modelo–Vista–Controlador (MVC)**:

- **Models:** representan las entidades y los datos del negocio.
- **Views:** contienen las interfaces creadas con Razor y Bootstrap.
- **Controllers:** reciben las solicitudes, coordinan las operaciones y devuelven las vistas o respuestas correspondientes.
- **Data:** contiene el contexto de Entity Framework Core y la configuración de acceso a la base de datos.
- **Areas/Identity:** reúne las páginas y funcionalidades relacionadas con ASP.NET Core Identity.
- **wwwroot:** almacena CSS, JavaScript, logos y fotografías de productos.
- **ScriptsSQL:** incluye un script para cargar datos del catálogo en una base de datos preparada.

## 🗄️ Base de datos

La aplicación utiliza Entity Framework Core con SQL Server. Entre las entidades principales se encuentran:

| Entidad | Función |
|---|---|
| `Producto` | Información comercial, precio, stock y estado de publicación. |
| `Categoria` | Clasificación de productos. |
| `Marca` | Marca asociada a cada producto. |
| `Pedido` | Compra realizada por un usuario, total y estado. |
| `DetallePedido` | Productos, cantidades y precios unitarios de cada pedido. |

Los usuarios, roles y credenciales se administran mediante las tablas de **ASP.NET Core Identity**.

El catálogo utilizado durante el desarrollo cuenta con **44 productos**, cada uno asociado a una fotografía local dentro de `wwwroot/images/productos/`.

## 🔄 ¿Cómo funciona una compra?

1. El usuario explora el catálogo y consulta los detalles de los productos.
2. Agrega artículos al carrito y ajusta las cantidades.
3. Inicia sesión para confirmar la compra.
4. El sistema verifica la disponibilidad de los productos.
5. El usuario completa un formulario de pago de prueba.
6. Se registra el pedido con sus detalles, se actualiza el stock y se vacía el carrito.
7. El cliente puede consultar el pedido desde **Mis pedidos** y el administrador puede actualizar su estado.

> **Importante:** el sistema de pago es una simulación académica. No se realizan transacciones bancarias reales.

## ▶️ Ejecución local

**Requisitos:**

- SDK de **.NET 10**.
- SQL Server LocalDB o una instancia compatible de SQL Server.
- Visual Studio con las herramientas para desarrollo ASP.NET Core, o un entorno equivalente.

**Pasos generales:**

1. Clonar el repositorio:

   ```bash
   git clone https://github.com/AdrianDC3/ByteStore.git
   ```

2. Abrir `ByteStore.csproj` en Visual Studio.
3. Restaurar las dependencias NuGet y revisar la cadena `DefaultConnection` de `appsettings.json` para el entorno local.
4. Crear o actualizar la base de datos aplicando las migraciones de Entity Framework Core existentes.
5. Preparar las categorías y marcas necesarias antes de ejecutar el script de carga de productos incluido en `ScriptsSQL`.
6. Ejecutar la aplicación desde Visual Studio.

**Nota:** la base de datos local de desarrollo no forma parte del repositorio. El script SQL carga productos en una base que ya tenga creadas las tablas y los registros correspondientes de categorías y marcas.

## 🔐 Seguridad y acceso

ByteStore utiliza **ASP.NET Core Identity** para la gestión de cuentas y contraseñas. Las acciones administrativas requieren autorización por rol y el acceso a los pedidos del cliente está restringido al usuario autenticado.

Las operaciones de confirmación de pedidos verifican el stock antes de registrar la compra. Los datos ingresados en el formulario de tarjeta se utilizan únicamente para la validación de la simulación y **no se guardan**.

## 👨‍💻 Autor

**Adrian Diaz Casas**  
Proyecto académico — Programación en Nuevas Tecnologías 1 (PNT1).
