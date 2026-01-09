# Servicio PLC - Worker Service .NET 10.0

Servicio de Windows para lectura automática de PLCs industriales tipo **Controlplast** usando **protocolo TCP/IP personalizado** y almacenamiento en SQL Server.

## 🎯 Características

- ✅ **Worker Service .NET 10.0** (también compilable para .NET 8.0)
- ✅ **Protocolo TCP/IP personalizado Controlplast** (no Modbus)
- ✅ Lectura asíncrona de múltiples PLCs en paralelo
- ✅ Reconexión automática ante fallos (reintentos configurables)
- ✅ Almacenamiento en SQL Server
- ✅ Sistema de eventos robusto con `GestorEventos`
- ✅ Logging detallado con `Logger` personalizado
- ✅ Configuración flexible mediante JSON (`appsettings.json`)
- ✅ Modo consola para desarrollo (detección automática)
- ✅ Tolerancia a fallos de red y BD

## 🚀 Requisitos

- **.NET 10.0 SDK** o **.NET 8.0 SDK** ([Descargar](https://dotnet.microsoft.com/download/dotnet))
- **SQL Server 2019+** (o compatible)
- **Visual Studio Code** (recomendado)
- **PowerShell 5.1+** (para instalación como servicio)
- **Permisos de Administrador** (para instalar servicio de Windows)

## 📦 Inicio Rápido

### 1. Configurar Base de Datos

Ejecutar el script SQL:

```sql
-- Abrir en SSMS y ejecutar
Scripts/plc_database_script.sql
```

O desde línea de comandos:

```powershell
sqlcmd -S localhost -i Scripts\plc_database_script.sql
```

### 2. Configurar appsettings.json

Editar el archivo `appsettings.json` con tus datos:

```json
{
  "IntervaloLecturaSegundos": 5,
  "Databases": {
    "Local": {
      "Enabled": true,
      "ConnectionString": "Server=.;Database=ProduccionLocal;Trusted_Connection=True;"
    }
  },
  "Plcs": [
    {
      "Nombre": "PLC1_Extrusora",
      "Ip": "192.168.0.10",
      "Puerto": 8000,
      "Id": 1,
      "Habilitada": true
    }
  ]
}
```

### 3. Probar en Modo Consola (Debug)

```powershell
# Ejecutar directamente
dotnet run

# O desde VS Code
Presionar F5
```

### 4. Compilar para Producción

```powershell
dotnet publish -c Release -o publish
```

### 5. Instalar como Servicio de Windows

**IMPORTANTE: Ejecutar PowerShell como Administrador**

```powershell
cd Scripts
.\Instalar-WorkerService.ps1
```

## 🔧 Gestión del Servicio

### Comandos PowerShell

```powershell
# Ver estado
Get-Service ServicioPLC

# Iniciar
Start-Service ServicioPLC

# Detener
Stop-Service ServicioPLC

# Reiniciar
Restart-Service ServicioPLC

# Ver detalles
Get-Service ServicioPLC | Format-List *
```

### Desinstalar

```powershell
cd Scripts
.\Desinstalar-WorkerService.ps1
```

## 📊 Arquitectura del Protocolo TCP/IP

El servicio usa un **protocolo TCP/IP personalizado** desarrollado específicamente para PLCs Controlplast:

### Estructura del Paquete

```
Header (38 bytes):
[0-27]  → Header estándar del protocolo
[28-29] → Tipo de memoria (0x8DFF = Datos, 0x08FF = Parámetros)
[30-33] → Dirección de memoria (3 bytes + padding)
[34-37] → Número de words a leer (2 bytes + padding)

Respuesta:
[0-32]  → Header de respuesta
[33+]   → Datos (2 bytes por word)
```

### Direcciones de Memoria Importantes

| Variable | Dirección | Tipo | Descripción |
|----------|-----------|------|-------------|
| Kg/Hora Actual | 800 | Float | Producción actual |
| Espesor Actual | 802 | Float | Espesor en mm |
| Velocidad Línea | 810 | Float | m/min |
| Estado Máquina | 30023 | Word | ON/OFF |
| Kg Producidos | 30037 | Float | Total producido |

## 🛠️ Personalización

### Agregar Nuevos Datos del PLC

1. **Editar `Modelos/DatosProduccion.cs`** - Agregar dirección y propiedad:

```csharp
public const int ADDR_NUEVA_VARIABLE = 850;
public float NuevaVariable { get; set; }
```

2. **Editar `Servicios/LectorPLC.cs`** - Agregar lectura en `LeerDatosAsync()`:

```csharp
lectura.Produccion.NuevaVariable = await LeerFloatAsync(DatosProduccion.ADDR_NUEVA_VARIABLE, TIPO_DADOS, cancellationToken) ?? 0;
```

3. **Editar `Modelos/modelos.cs`** - Agregar a la visualización en `ToString()` si es importante

4. **Editar `Servicios/BaseDatos.cs`** - Agregar parámetro en el INSERT/UPDATE:

```csharp
comando.Parameters.AddWithValue("@NuevaVariable", lectura.Produccion.NuevaVariable);
```

5. **SQL Server** - Agregar columna a la tabla:

```sql
ALTER TABLE LecturasPLC ADD NuevaVariable FLOAT NULL;
```

### Agregar Más PLCs

Simplemente editar `Configuracion/appsettings.json`:

```json
"Plcs": [
  {
    "Nombre": "PLC1_Extrusora",
    "Ip": "192.168.0.10",
    "Puerto": 8000,
    "Id": 1,
    "Habilitada": true
  },
  {
    "Nombre": "PLC2_Extrusora",
    "Ip": "192.168.0.11",
    "Puerto": 8000,
    "Id": 2,
    "Habilitada": true
  }
]
```

### Modificar Intervalo de Lecturas

Editar en `Configuracion/appsettings.json`:

```json
"IntervaloLecturaSegundos": 5,
"IntervaloReconexionSegundos": 10,
"TimeoutLecturaSegundos": 8,
"MaximoReintentos": 3
```

## 📝 Logs y Monitoreo

### Archivos de Log

```
Logs/
├── servicio_plc.log      # Log técnico detallado
├── eventos.log           # Eventos del sistema
└── *.bak                 # Backups automáticos
```

### Ver Logs en Tiempo Real

```powershell
# PowerShell
Get-Content .\Logs\servicio_plc.log -Wait -Tail 50

# CMD
tail -f .\Logs\servicio_plc.log
```

### Event Viewer de Windows

```powershell
# Abrir Event Viewer
eventvwr.msc

# Navegar a:
Windows Logs > Application > Source: ServicioPLC
```

## 🔍 Troubleshooting

### El servicio no inicia

1. Verificar logs: `Logs\servicio_plc.log`
2. Verificar Event Viewer: `eventvwr.msc`
3. Verificar conexión a BD:

```powershell
# Probar conexión
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

### No se conecta al PLC

```powershell
# Hacer ping
ping 192.168.0.10

# Probar puerto
Test-NetConnection -ComputerName 192.168.0.10 -Port 8000

# Verificar firewall
Get-NetFirewallRule | Where-Object {$_.Enabled -eq 'True'}
```

### Error de permisos en SQL Server

```sql
-- Dar permisos al usuario del servicio (LocalSystem)
USE ProduccionLocal;
GO

CREATE USER [NT AUTHORITY\SYSTEM] FOR LOGIN [NT AUTHORITY\SYSTEM];
ALTER ROLE db_datareader ADD MEMBER [NT AUTHORITY\SYSTEM];
ALTER ROLE db_datawriter ADD MEMBER [NT AUTHORITY\SYSTEM];
GO
```

### Servicio se detiene solo

Verificar:
1. Logs de aplicación
2. Event Viewer
3. Conexión a PLCs (timeout)
4. Memoria disponible del sistema

## 📊 Consultas SQL Útiles

```sql
-- Ver últimas 100 lecturas
SELECT TOP 100 * FROM LecturasPLC ORDER BY FechaHoraLectura DESC;

-- Ver lecturas de un PLC específico
SELECT TOP 50 * 
FROM LecturasPLC 
WHERE PlcId = 1 
ORDER BY FechaHoraLectura DESC;

-- Ver eventos del sistema
SELECT TOP 100 * 
FROM EventosSistema 
ORDER BY FechaHora DESC;

-- Estadísticas de producción (últimas 24 horas)
SELECT 
    PlcId,
    COUNT(*) as TotalLecturas,
    AVG(KgHoraActual) as PromedioKgHora,
    MAX(KgHoraActual) as MaxKgHora,
    MIN(FechaHoraLectura) as PrimeraLectura,
    MAX(FechaHoraLectura) as UltimaLectura
FROM LecturasPLC
WHERE FechaHoraLectura >= DATEADD(DAY, -1, GETDATE())
GROUP BY PlcId;

-- Producción por hora
SELECT 
    CONVERT(DATE, FechaHoraLectura) as Fecha,
    DATEPART(HOUR, FechaHoraLectura) as Hora,
    PlcId,
    AVG(KgHoraActual) as PromedioKgHora,
    SUM(KgProducidos) as TotalProducido
FROM LecturasPLC
GROUP BY CONVERT(DATE, FechaHoraLectura), DATEPART(HOUR, FechaHoraLectura), PlcId
ORDER BY Fecha DESC, Hora DESC;
```

## 🔄 Actualizar el Servicio

1. Detener el servicio:
```powershell
Stop-Service ServicioPLC
```

2. Compilar:
```powershell
dotnet build -c Release
```

3. Copiar archivos a carpeta de instalación (típicamente `C:\Servicios\PLCServicio\`)

4. Iniciar el servicio:
```powershell
Start-Service ServicioPLC
```

## 🎯 Flujo de Funcionamiento

1. **Inicio del servicio** → `Program.cs` crea el Host (.NET 10.0)
2. **Configuración** → `ConfiguracionManager` carga `appsettings.json`
3. **Inyección de dependencias** → Se registran Logger, GestorLecturas, etc.
4. **BackgroundService** → `WorkerPLC` inicia el bucle principal
5. **Para cada PLC**:
   - `GestorLecturas` crea instancia de `LectorPLC`
   - Conecta vía `LectorPLC.ConectarAsync()`
   - Lee datos con `LectorPLC.LeerDatosAsync()` cada N segundos
   - Guarda en BD con `BaseDatos.GuardarLecturaAsync()`
   - Reconecta automáticamente si hay error
6. **Eventos** → Se registran en `Eventos.cs` para auditoría
7. **Logs** → Se guardan en `Logs/` para debugging

## 📊 Clases Principales

### `LectorPLC` - Comunicación con PLC
- **Responsabilidad**: Implementar protocolo TCP/IP Controlplast
- **Métodos principales**:
  - `ConectarAsync()` - Establece conexión TCP
  - `LeerDatosAsync()` - Lee todos los datos de producción
  - `LeerFloatAsync()` - Lee un float (4 bytes)
  - `LeerStringAsync()` - Lee una cadena
  - `ReconectarAsync()` - Reconexión automática
  - `Desconectar()` - Cierra conexión

### `BaseDatos` - Persistencia
- **Responsabilidad**: Guardar lecturas en SQL Server
- **Métodos**:
  - `GuardarLecturaAsync()` - INSERT en tabla LecturasPLC
  - `ActualizarEstadoPlcAsync()` - UPDATE de estado

### `GestorLecturas` - Orquestación
- **Responsabilidad**: Coordinar múltiples PLCs
- **Métodos**:
  - `IniciarLecturas()` - Inicia threads para cada PLC
  - `ProcesoLecturaPLC()` - Bucle de lectura continua
  - `DetenerLecturas()` - Limpia recursos

### `Worker` - Servicio Principal
- Extiende `BackgroundService` de .NET
- Método `ExecuteAsync()` es el bucle infinito
- Se ejecuta como servicio de Windows

## 🔍 Protocolo TCP/IP Controlplast

### Estructura del Paquete

```
Solicitud (38 bytes):
[0-27]   → Header estándar del protocolo
[28-29]  → Tipo de memoria (0x8DFF = Datos, 0x08FF = Parámetros)
[30-33]  → Dirección de memoria (3 bytes + 1 padding)
[34-37]  → Número de words a leer (2 bytes + 2 padding)

Respuesta:
[0-32]   → Header de respuesta
[33+]    → Datos (2 bytes por word, little-endian)
```

### Tipos de Datos

- **Float** (32 bits): 2 words (dirección + dirección+2)
- **Word** (16 bits): 1 word
- **String** (char/2): N words

### Direcciones de Memoria (Datos Producción)

Ver `Modelos/DatosProduccion.cs` para lista completa. Ejemplos:

| Variable | Dirección | Tipo | Descripción |
|----------|-----------|------|-------------|
| KgHora Actual | 30000 | Float | Producción actual en kg/h |
| Espesor Actual | 30002 | Float | Espesor en mm |
| Velocidad Línea | 30006 | Float | Metros por minuto |
| Número OP | 30043 | String | Número de orden de producción |
| Kg Producidos | 30048 | Float | Total producido en esta OP |

**Nota**: Las direcciones son hexadecimales en el protocolo pero se usan como decimales en el código.

**Nota Importante**: 
- Este servicio usa **protocolo TCP/IP personalizado Controlplast**, NO Modbus
- El puerto por defecto es **8000**, no 502
- Target frameworks: **.NET 10.0** (primario) y **.NET 8.0** (compatible)
- Compilación para .NET 8.0: Editar `plcServicio.csproj` y cambiar `net10.0` a `net8.0`

**Versión**: 2.1.0 (Worker Service .NET 10.0)  
**Última actualización**: Enero 2026
