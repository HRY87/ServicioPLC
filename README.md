# Servicio PLC - Worker Service .NET 8.0

Servicio de Windows para lectura automática de PLCs industriales usando **protocolo TCP/IP personalizado** y almacenamiento en SQL Server.

## 🎯 Características

- ✅ **Worker Service .NET 8.0** (no usa .NET Framework)
- ✅ **Protocolo TCP/IP personalizado** (no Modbus)
- ✅ Lectura asíncrona de múltiples PLCs en paralelo
- ✅ Reconexión automática ante fallos
- ✅ Almacenamiento dual (BD local + nube)
- ✅ Sistema de eventos y logging robusto
- ✅ Configuración flexible mediante JSON
- ✅ Modo consola para desarrollo
- ✅ Resistente a fallos

## 🚀 Requisitos

- **.NET 8.0 SDK** ([Descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- **SQL Server 2019+** (o compatible)
- **Visual Studio Code** (recomendado)
- **PowerShell 5.1+** (para instalación)
- **Permisos de Administrador** (para instalar servicio)

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

1. **Editar `LectorPlcTcp.cs`** - Agregar lectura en `LeerDatosAsync()`:

```csharp
lectura.Presion = await LeerFloatAsync(112, cancellationToken) ?? 0;
```

2. **Editar `Modelos/modelos.cs`** - Agregar propiedad:

```csharp
public float Presion { get; set; }
```

3. **Editar `RepositorioDatos.cs`** - Agregar parámetro:

```csharp
comando.Parameters.AddWithValue("@Presion", lectura.Presion);
```

4. **Ejecutar en SQL Server**:

```sql
ALTER TABLE LecturasPLC ADD Presion FLOAT NULL;
```

### Agregar Más PLCs

Simplemente editar `appsettings.json`:

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
SELECT * FROM vw_UltimasLecturas;

-- Ver lecturas de un PLC específico
SELECT TOP 50 * 
FROM LecturasPLC 
WHERE PlcId = 1 
ORDER BY FechaHoraLectura DESC;

-- Ver eventos del sistema
SELECT TOP 100 * 
FROM EventosSistema 
ORDER BY FechaHora DESC;

-- Estadísticas de producción
SELECT 
    NombrePlc,
    COUNT(*) as TotalLecturas,
    AVG(KgHoraActual) as PromedioKgHora,
    MIN(FechaHoraLectura) as PrimeraLectura,
    MAX(FechaHoraLectura) as UltimaLectura
FROM LecturasPLC
WHERE FechaHoraLectura >= DATEADD(DAY, -1, GETDATE())
GROUP BY NombrePlc;

-- Limpiar datos antiguos
EXEC sp_LimpiarDatosAntiguos @DiasAntiguedad = 90;
```

## 🔄 Actualizar el Servicio

1. Detener el servicio:
```powershell
Stop-Service ServicioPLC
```

2. Reemplazar archivos en `publish\`

3. Iniciar el servicio:
```powershell
Start-Service ServicioPLC
```

O simplemente ejecutar:
```powershell
.\Scripts\Instalar-WorkerService.ps1
```

## 🎯 Diferencias con .NET Framework

| Aspecto | .NET Framework | .NET 8.0 Worker |
|---------|----------------|-----------------|
| Clase base | `ServiceBase` | `BackgroundService` |
| Instalación | `InstallUtil.exe` | `New-Service` PowerShell |
| NuGet SQL | `System.Data.SqlClient` | `Microsoft.Data.SqlClient` |
| Hosting | Manual | `IHostBuilder` |
| Logging | Manual | `ILogger` integrado |
| DI | Manual | Inyección nativa |

## 📄 Licencia

[Especifica tu licencia aquí]

## 👤 Autor

[Tu nombre/empresa]

---

**Nota Importante:** Este servicio usa protocolo TCP/IP personalizado, NO Modbus. El puerto por defecto es 8000, no 502.

**Versión:** 2.0.0 (Worker Service .NET 8.0)  
**Fecha:** Enero 2026
