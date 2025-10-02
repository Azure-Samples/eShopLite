# Agente Asistente de Compras - Descripción General

## Introducción

El Agente Asistente de Compras es una solución de comercio conversacional impulsada por IA que ayuda a los usuarios a descubrir productos, obtener información detallada y gestionar su carrito de compras a través de interacciones en lenguaje natural. Construido usando el Framework de Agentes de Microsoft, demuestra cómo los agentes de IA modernos pueden mejorar la experiencia de comercio electrónico.

## ¿Qué es el Agente Asistente de Compras?

El Asistente de Compras es un agente inteligente que comprende la intención del usuario y proporciona respuestas contextuales a consultas relacionadas con compras. Puede:

- **Buscar Productos:** Encontrar productos basados en descripciones en lenguaje natural
- **Proporcionar Detalles de Productos:** Recuperar y presentar información detallada de productos
- **Gestionar el Carrito de Compras:** Agregar artículos al carrito mediante comandos conversacionales
- **Hacer Recomendaciones:** Sugerir productos basados en las preferencias y contexto del usuario

## Capacidades Clave

### Comprensión del Lenguaje Natural

El agente utiliza los modelos GPT de Azure OpenAI para comprender la intención del usuario a partir de consultas en lenguaje natural. Puede manejar:

- Consultas simples: "Muéstrame botas de senderismo"
- Solicitudes complejas: "Necesito botas de senderismo impermeables para el invierno, preferiblemente por menos de 150€"
- Preguntas de seguimiento: "Cuéntame más sobre las primeras"
- Acciones del carrito: "Agrégalas a mi carrito"

### Orquestación Multi-Herramienta

El agente coordina múltiples herramientas especializadas para cumplir con las solicitudes de los usuarios:

1. **Herramienta SearchCatalog** - Busca en la base de datos de productos
2. **Herramienta ProductDetails** - Recupera información detallada de productos
3. **Herramienta AddToCart** - Gestiona operaciones del carrito

### Respuestas Conscientes del Contexto

El agente mantiene el contexto de la conversación para proporcionar respuestas de seguimiento relevantes y manejar referencias a artículos previamente discutidos.

## Descripción General de la Arquitectura

El Asistente de Compras está construido sobre tres componentes principales:

1. **Servicio de Agente** - La lógica central del agente usando Microsoft.Agents.Client
2. **Herramientas del Agente** - Herramientas especializadas para diferentes operaciones
3. **Interfaz de Chat** - Interfaz de usuario de chat en la aplicación Store

```
┌─────────────────┐
│   Interfaz      │
│   Store         │
│  (Panel Chat)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Asistente      │
│  de Compras     │
│  Agente         │
└────────┬────────┘
         │
    ┌────┴────┐
    │ Capa de │
    │ Herram. │
    └────┬────┘
         │
         ▼
┌─────────────────┐
│  API Productos  │
│  & Base de      │
│  Datos          │
└─────────────────┘
```

## Pila Tecnológica

- **Microsoft Agent Framework** - Orquestación de agentes e integración de herramientas
- **Azure OpenAI** - Comprensión y generación de lenguaje natural
- **ASP.NET Core** - Servicios API backend
- **Blazor** - Interfaz de usuario de chat interactiva
- **.NET Aspire** - Orquestación nativa en la nube
- **SQL Server** - Datos de productos y pedidos

## Casos de Uso

### Descubrimiento de Productos

**Usuario:** "Estoy buscando zapatillas para correr en senderos"

**Agente:** Busca en el catálogo y devuelve zapatillas de trail relevantes con breves descripciones y precios.

### Información Detallada

**Usuario:** "Cuéntame más sobre las azules"

**Agente:** Recupera y presenta información detallada sobre el producto referenciado, incluyendo especificaciones, características y reseñas de clientes.

### Gestión del Carrito

**Usuario:** "Agrégalas a mi carrito"

**Agente:** Agrega el producto referenciado al carrito del usuario y confirma la acción.

## Beneficios

- **Experiencia de Usuario Mejorada:** La conversación natural es más intuitiva que la búsqueda tradicional
- **Mayor Participación:** Los usuarios pasan más tiempo explorando productos a través de la conversación
- **Mejor Descubrimiento:** La IA ayuda a los usuarios a encontrar productos que no habrían descubierto mediante búsqueda por palabras clave
- **Fricción Reducida:** Agregar artículos al carrito es más fácil a través de la conversación
- **Personalización:** El agente puede aprender las preferencias del usuario con el tiempo

## Comenzando

Para comenzar a usar el Asistente de Compras:

1. Implemente el escenario siguiendo las instrucciones del README
2. Abra la aplicación Store
3. Haga clic en el ícono de chat para abrir el asistente
4. Comience a conversar con consultas en lenguaje natural

Para instrucciones de configuración detalladas, consulte [Configuración - Desarrollo Local](30_setup_local.md) o [Configuración - Implementación en Azure](40_setup_azure.md).
