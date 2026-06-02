# AutoCheckAML Backend API

API RESTful en ASP.NET Core para la gestión de formularios con autenticación JWT, base de datos SQLite y exportación a Excel.

## 🚀 Características

- ✅ **Autenticación JWT**: Sistema de login seguro con tokens JWT
- ✅ **Gestión de Usuarios**: Registro y login de usuarios
- ✅ **API REST**: Endpoints para CRUD de formularios
- ✅ **Búsqueda Avanzada**: Filtrar formularios por múltiples criterios
- ✅ **Exportación a Excel**: Descargar datos en formato XLSX
- ✅ **Validaciones**: Validaciones en servidor de todos los datos
- ✅ **Base de datos SQLite**: Almacenamiento persistente
- ✅ **CORS Habilitado**: Comunicación con Frontend

## 📋 Requisitos

- .NET 8.0 o superior
- Visual Studio Code o Visual Studio
- SQLite (incluido con .NET)

## 🔧 Instalación

### 1. Restaurar dependencias
```bash
cd AutoCheckAML.Api
dotnet restore
```

### 2. Ejecutar migraciones (si aplica)
```bash
dotnet ef database update
```

### 3. Iniciar el servidor
```bash
dotnet run
```

El servidor estará disponible en: `https://localhost:7087` (o el puerto indicado en consola)

## 📚 Endpoints de la API

### Autenticación

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "id": 1,
  "username": "admin",
  "email": "admin@autocheck.com",
  "fullName": "Administrador",
  "token": "eyJhbGc..."
}
```

#### Registro
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "user@example.com",
  "password": "password123",
  "fullName": "Nuevo Usuario"
}
```

### Formularios (Requiere autenticación con JWT)

#### Enviar Formulario
```http
POST /api/formsubmissions/submit
Authorization: Bearer <token>
Content-Type: application/json

{
  "nombre": "Juan Pérez",
  "email": "juan@example.com",
  "telefono": "+1234567890",
  "empresa": "Acme Corp",
  "asunto": "Consulta general",
  "mensaje": "Tengo una pregunta...",
  "fecha": "2024-05-27"
}
```

#### Obtener Todos los Formularios
```http
GET /api/formsubmissions/all
Authorization: Bearer <token>
```

#### Obtener Formulario por ID
```http
GET /api/formsubmissions/{id}
Authorization: Bearer <token>
```

#### Buscar Formularios
```http
POST /api/formsubmissions/search
Authorization: Bearer <token>
Content-Type: application/json

{
  "searchTerm": "nombre o email",
  "status": "Pendiente",
  "startDate": "2024-01-01",
  "endDate": "2024-12-31",
  "pageNumber": 1,
  "pageSize": 10
}
```

#### Actualizar Estado del Formulario
```http
PUT /api/formsubmissions/{id}/status
Authorization: Bearer <token>
Content-Type: application/json

{
  "status": "Revisado"
}
```

Estados válidos: `Pendiente`, `Revisado`, `Completado`

#### Eliminar Formulario
```http
DELETE /api/formsubmissions/{id}
Authorization: Bearer <token>
```

#### Exportar a Excel
```http
GET /api/formsubmissions/export/excel
Authorization: Bearer <token>
```

Devuelve un archivo XLSX con todos los formularios.

## 🔐 Autenticación

Todos los endpoints de formularios requieren autenticación. Debe incluir el token JWT en el encabezado:

```
Authorization: Bearer <token_aquí>
```

El token expira en 24 horas.

## 💾 Estructura de Base de Datos

### Tabla: Users
- `Id` (int, PK)
- `Username` (string, unique)
- `Email` (string, unique)
- `PasswordHash` (string)
- `FullName` (string)
- `IsActive` (bool)
- `CreatedAt` (datetime)
- `LastLogin` (datetime)

### Tabla: FormSubmissions
- `Id` (int, PK)
- `UserId` (int, FK)
- `Nombre` (string)
- `Email` (string)
- `Telefono` (string)
- `Empresa` (string)
- `Asunto` (string)
- `Mensaje` (string)
- `Fecha` (datetime)
- `CreatedAt` (datetime)
- `Status` (string) - Pendiente, Revisado, Completado

## ⚙️ Configuración

El archivo `appsettings.json` contiene:

```json
{
  "Jwt": {
    "Secret": "tu-clave-secreta-aqui",
    "Issuer": "AutoCheckAML",
    "Audience": "AutoCheckAMLClients",
    "ExpirationHours": 24
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=autocheckaml.db"
  }
}
```

**⚠️ IMPORTANTE:** Cambiar la `Secret` en producción por una clave segura.

## 📦 Dependencias Principales

- **Entity Framework Core**: ORM para base de datos
- **System.IdentityModel.Tokens.Jwt**: JWT generation y validation
- **ClosedXML**: Exportación a Excel
- **BCrypt.Net-Next**: Hash de contraseñas

## 🧪 Prueba la API

### Usando Postman o Thunder Client

1. **Login** para obtener token
2. Copiar el token de la respuesta
3. Usar el token en el encabezado `Authorization: Bearer <token>` para otros endpoints

### Usuarios de Prueba

Usuario por defecto:
- **Username**: `admin`
- **Password**: `admin123`

## 📝 Notas Importantes

- Las contraseñas se almacenan hasheadas con BCrypt (nunca en texto plano)
- La base de datos SQLite se crea automáticamente en la primera ejecución
- CORS está habilitado para permitir requests desde cualquier origen (configurable)
- Todos los errores devuelven mensajes descriptivos

## 🤝 Contribuciones

Este es un proyecto en desarrollo. Para cambios o mejoras, contactar al equipo de desarrollo.

## 📄 Licencia

Todos los derechos reservados © 2024 AutoCheckAML
