# Agente Asistente de Compras - Guía de Configuración Local

Esta guía le acompaña en la configuración del escenario Agente Asistente de Compras en su máquina de desarrollo local.

## Requisitos Previos

Antes de comenzar, asegúrese de tener instalado lo siguiente:

### Software Requerido

1. **.NET 8 SDK o posterior**
   - Descargar desde [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
   - Verificar instalación: `dotnet --version`

2. **Docker Desktop** o **Podman**
   - Docker Desktop: [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
   - Podman: [https://podman.io/](https://podman.io/)
   - Requerido para ejecutar el contenedor SQL Server

3. **Workload .NET Aspire**
   ```bash
   dotnet workload install aspire
   ```

4. **Git**
   - Descargar desde [https://git-scm.com/downloads](https://git-scm.com/downloads)

### Herramientas Recomendadas

- **Visual Studio 2022** (17.8 o posterior) o **Visual Studio Code** con C# Dev Kit
- **Azure Developer CLI (azd)** - Para implementación en Azure
  ```bash
  # Windows (PowerShell)
  winget install microsoft.azd
  
  # macOS
  brew tap azure/azd && brew install azd
  
  # Linux
  curl -fsSL https://aka.ms/install-azd.sh | bash
  ```

### Recursos de Azure

Necesitará acceso a:

- **Servicio Azure OpenAI** con:
  - Implementación `gpt-4o-mini`
  - Implementación `text-embedding-ada-002`
- **Suscripción Azure** con permisos apropiados

## Paso 1: Clonar el Repositorio

```bash
git clone https://github.com/Azure-Samples/eShopLite.git
cd eShopLite/scenarios/13-AgentFramework
```

## Paso 2: Configurar Azure OpenAI

### Opción A: Uso de un Recurso Azure OpenAI Existente

Si tiene un recurso Azure OpenAI existente:

1. Navegue al proyecto ShoppingAssistantAgent:
   ```bash
   cd src/ShoppingAssistantAgent
   ```

2. Inicialice los secretos de usuario:
   ```bash
   dotnet user-secrets init
   ```

3. Defina su configuración Azure OpenAI:
   ```bash
   dotnet user-secrets set "OpenAI:Endpoint" "https://tu-recurso.openai.azure.com/"
   dotnet user-secrets set "OpenAI:ApiKey" "tu-clave-api-aqui"
   dotnet user-secrets set "OpenAI:DeploymentName" "gpt-4o-mini"
   ```

4. Configure también la API Productos:
   ```bash
   cd ../Products
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://tu-recurso.openai.azure.com/;Key=tu-clave-api-aqui;"
   dotnet user-secrets set "AI_ChatDeploymentName" "gpt-4o-mini"
   dotnet user-secrets set "AI_embeddingsDeploymentName" "text-embedding-ada-002"
   ```

### Opción B: Dejar que Aspire Cree los Recursos

Si no tiene recursos Azure OpenAI, Aspire puede crearlos por usted:

1. Asegúrese de estar conectado a Azure:
   ```bash
   azd auth login
   ```

2. Omita la configuración de secretos - Aspire aprovisionará recursos en el primer lanzamiento

## Paso 3: Construir la Solución

Navegue al directorio src y construya:

```bash
cd src
dotnet restore
dotnet build
```

Verifique que la construcción se complete sin errores.

## Paso 4: Iniciar la Aplicación

### Uso de .NET Aspire AppHost

1. Navegue al proyecto AppHost:
   ```bash
   cd eShopAppHost
   ```

2. Ejecute la aplicación:
   ```bash
   dotnet run
   ```

3. El Panel Aspire se abrirá automáticamente en su navegador, mostrando:
   - Todos los servicios en ejecución
   - Puntos de acceso de servicios
   - Registros y trazas
   - Estado de recursos

### Uso de Visual Studio

1. Abra `src/eShopLite-AgentFramework.sln` en Visual Studio

2. Establezca `eShopAppHost` como proyecto de inicio

3. Presione F5 para ejecutar

### Uso de Visual Studio Code

1. Abra la carpeta `scenarios/13-AgentFramework` en VS Code

2. Abra la Paleta de Comandos (Ctrl+Shift+P / Cmd+Shift+P)

3. Seleccione `.NET: Run Aspire App`

4. Elija `eShopAppHost`

## Paso 5: Acceder a la Aplicación

Una vez que la aplicación inicie, verá una salida similar a:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Puntos de Acceso

- **Interfaz Store**: `https://localhost:7001`
  - Aplicación eCommerce principal
  - Interfaz de chat del Asistente de Compras

- **Panel Aspire**: `https://localhost:17001`
  - Gestión de servicios
  - Registros y trazas
  - Monitoreo de recursos

- **Agente Asistente de Compras**: `https://localhost:7002`
  - Interfaz Swagger en `/swagger`
  - API Chat en `/api/agent/chat`

- **API Productos**: `https://localhost:7003`
  - Interfaz Swagger en `/swagger`
  - Puntos de acceso de productos

## Paso 6: Probar el Asistente de Compras

1. Abra la interfaz Store en su navegador

2. Busque el ícono de chat (generalmente en la esquina superior derecha o inferior derecha)

3. Haga clic para abrir el panel de chat

4. Pruebe estas consultas de ejemplo:
   - "Muéstrame botas de senderismo"
   - "Cuéntame más sobre el producto 1"
   - "Agrégalas a mi carrito"
   - "¿Qué equipo para exteriores tienen?"

## Solución de Problemas

### Problema: Puerto Ya en Uso

**Solución:** Cambie los puertos en `eShopAppHost/Properties/launchSettings.json`

### Problema: El Contenedor SQL Server No Inicia

**Solución:** 
1. Asegúrese de que Docker Desktop esté en ejecución
2. Verifique los puertos disponibles: `netstat -an | findstr :1433`
3. Intente reiniciar Docker Desktop

### Problema: Límites de Tasa de Azure OpenAI

**Solución:**
1. Verifique su cuota en el Portal de Azure
2. Reduzca las solicitudes concurrentes
3. Considere actualizar a una cuota mayor

### Problema: El Agente No Responde

**Solución:**
1. Verifique los errores en el Panel Aspire
2. Verifique la configuración de OpenAI en los secretos de usuario
3. Verifique la conectividad de red a Azure OpenAI
4. Revise los registros en Application Insights

### Problema: Errores de Construcción

**Solución:**
1. Limpiar la solución: `dotnet clean`
2. Eliminar carpetas bin/obj: `rm -rf **/bin **/obj`
3. Restaurar paquetes: `dotnet restore`
4. Reconstruir: `dotnet build`

## Pasos de Verificación

Después de la configuración, verifique:

1. ✅ Todos los servicios muestran "En ejecución" en el Panel Aspire
2. ✅ La interfaz Store se carga sin errores
3. ✅ El panel de chat se abre y acepta entradas
4. ✅ El agente responde a consultas simples
5. ✅ Los productos aparecen en los resultados de búsqueda
6. ✅ La funcionalidad de agregar al carrito funciona

## Próximos Pasos

- Revise la [Documentación de Arquitectura](20_architecture.md)
- Lea la [Guía de Usuario](50_user_guide.md) para consejos de uso
- Vea la [Guía de Administrador](60_admin_guide.md) para opciones de configuración
- Intente implementar en Azure con la [Guía de Configuración Azure](40_setup_azure.md)

## Configuración Adicional

### Habilitar Registro Detallado

Agregue a `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "ShoppingAssistantAgent": "Debug"
    }
  }
}
```

### Configurar CORS para Pruebas Locales

En `ShoppingAssistantAgent/Program.cs`, CORS ya está configurado para desarrollo local.

### Personalizar el Comportamiento del Agente

Edite las descripciones de herramientas en:
- `Tools/SearchCatalogTool.cs`
- `Tools/ProductDetailsTool.cs`
- `Tools/AddToCartTool.cs`

## Flujo de Trabajo de Desarrollo

1. Realice cambios en el código
2. Aspire reconstruirá y reiniciará automáticamente los servicios afectados
3. Actualice el navegador para ver los cambios
4. Verifique los registros y errores en el Panel Aspire
5. Itere

## Consejos de Rendimiento

- Use `dotnet build` en lugar de `dotnet run --no-build` al iterar
- Asigne recursos de Docker Desktop apropiadamente
- Monitoree el uso de tokens en Azure OpenAI para gestionar costos
- Use caché local para datos consultados frecuentemente

## Obtener Ayuda

Si encuentra problemas:

1. Consulte la sección [Solución de Problemas](#solución-de-problemas) anterior
2. Revise los registros en el Panel Aspire
3. Busque problemas existentes en GitHub
4. Cree un nuevo problema con:
   - Pasos para reproducir
   - Mensajes de error
   - Detalles del entorno
   - Capturas de pantalla del Panel Aspire
