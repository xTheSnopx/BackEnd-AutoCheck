# BackEnd AutoCheckAML

API RESTful en **ASP.NET Core** para gestión de formularios, autenticación JWT y exportación a Excel.

## 📁 Estructura del Proyecto

```
AutoCheckAML.Api/
├── Controllers/          # Controladores de la API
│   ├── AuthController.cs
│   └── FormSubmissionsController.cs
├── Models/              # Modelos de dominio
│   ├── User.cs
│   └── FormSubmission.cs
├── DTOs/                # Data Transfer Objects
│   ├── AuthDTOs.cs
│   └── FormDTOs.cs
├── Services/            # Servicios de negocio
│   ├── AuthService.cs
│   ├── FormService.cs
│   └── ExportService.cs
├── Data/                # Contexto y migraciones
│   └── AutoCheckAMLContext.cs
├── Program.cs           # Configuración de la aplicación
├── appsettings.json     # Configuración
└── API_DOCUMENTATION.md # Documentación completa de la API
```

## 🚀 Instalación y Ejecución

### Requisitos
- **.NET 8.0+** o superior
- Visual Studio Code o Visual Studio

### Pasos

1. **Navegar a la carpeta del proyecto**
```bash
cd BackEnd-AutoCheck/AutoCheckAML.Api
```

2. **Restaurar dependencias**
```bash
dotnet restore
```

3. **Compilar el proyecto**
```bash
dotnet build
```

4. **Ejecutar la aplicación**
```bash
dotnet run
```

El servidor estará disponible en:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:7087`

## 🔑 Credenciales de Prueba

Usuario por defecto creado en la primera ejecución:
```
Username: admin
Password: admin123
Email: admin@autocheck.com
```

## 📚 Endpoints Principales

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar nuevo usuario

### Formularios (Requiere token JWT)
- `POST /api/formsubmissions/submit` - Enviar formulario
- `GET /api/formsubmissions/all` - Obtener todos los formularios
- `GET /api/formsubmissions/{id}` - Obtener formulario específico
- `POST /api/formsubmissions/search` - Buscar con filtros
- `PUT /api/formsubmissions/{id}/status` - Actualizar estado
- `DELETE /api/formsubmissions/{id}` - Eliminar formulario
- `GET /api/formsubmissions/export/excel` - Exportar a Excel

## 🔐 Autenticación

Todos los endpoints protegidos requieren incluir el token JWT en el header:

```
Authorization: Bearer <tu_token_aqui>
```

El token se obtiene al hacer login y expira en 24 horas.

## 💾 Base de Datos

- **Motor**: SQLite
- **Archivo**: `autocheckaml.db`
- **Ubicación**: Se crea automáticamente en la raíz del proyecto

### Tablas
- **Users** - Usuarios registrados
- **FormSubmissions** - Formularios enviados

La base de datos se crea automáticamente en la primera ejecución.

## ⚙️ Configuración

Editar `appsettings.json` para cambiar:

```json
{
  "Jwt": {
    "Secret": "cambiar-esto-en-produccion",
    "Issuer": "AutoCheckAML",
    "Audience": "AutoCheckAMLClients",
    "ExpirationHours": 24
  }
}
```

⚠️ **IMPORTANTE**: Cambiar la `Secret` en producción por una clave segura de al menos 32 caracteres.

## 📦 Dependencias

- **Entity Framework Core** - ORM para base de datos
- **Microsoft.AspNetCore.Authentication.JwtBearer** - Autenticación JWT
- **ClosedXML** - Exportación a Excel
- **BCrypt.Net-Next** - Hash de contraseñas
- **SQLite** - Base de datos

## 🧪 Pruebas

### Con Postman/Thunder Client

1. Hacer login para obtener token
2. Copiar el token de la respuesta
3. Incluirlo en el header `Authorization: Bearer <token>`
4. Llamar a los endpoints protegidos

### Ejemplo de Login
```bash
curl -X POST https://localhost:7087/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

## 📝 Notas Importantes

- ✅ Las contraseñas se almacenan hasheadas con BCrypt
- ✅ CORS está habilitado para cualquier origen
- ✅ Los errores devuelven mensajes descriptivos
- ✅ La paginación está implementada en búsquedas

## 🛠️ Comandos Útiles

```bash
# Ver versión de .NET
dotnet --version

# Crear migraciones (si se modifican modelos)
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Limpiar build
dotnet clean

# Ejecutar con configuración de Release
dotnet run --configuration Release
```

## 📖 Documentación Completa

Para documentación detallada de todos los endpoints, ver `API_DOCUMENTATION.md` en esta carpeta.

## 🤝 Soporte

Para problemas o preguntas, contactar al equipo de desarrollo.

## 📄 Licencia

Todos los derechos reservados © 2024 AutoCheckAML
