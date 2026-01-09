# Proyecto Servicio PLC

Servicio de Windows para lectura automática de datos desde PLCs industriales (Modbus) y almacenamiento en bases de datos SQL Server (local y/o nube).

## 📋 Características

- ✅ Lectura asíncrona de múltiples PLCs en paralelo
- ✅ Reconexión automática ante fallos
- ✅ Almacenamiento dual (BD local + nube)
- ✅ Sistema de eventos y logging robusto
- ✅ Configuración flexible mediante JSON
- ✅ Modo consola para desarrollo y debug
- ✅ Resistente a fallos (no se detiene si falla una BD o PLC)

## 🏗️ Estructura del Proyecto

```
ProyectoServicioPLC/
│
├── Program.cs                      # Punto de entrada
├── MiServicio.cs                   # Clase principal del servicio
│
├── Configuracion/
│   ├── appsettings.json            # Configuración del sistema
│   └── ConfiguracionManager.cs     # Gestor de configuración
│
├── Modelos/
│   └── LecturaPlc.cs               # Modelos de datos
│
├── Servicios/
│   ├── LectorPlc.cs                # Lector individual de PLC
│   ├── GestorLecturas.cs           # Coordinador de lecturas
│   └── RepositorioDatos.cs         # Persistencia en BD
│
├── Eventos/
│   └── GestorEventos.cs            # Sistema de eventos
│
├── Mensajes/
│   └── GestorMensajes.cs           # Mensajes en consola
│
├── Utilidades/
│   ├── Logger.cs                   # Sistema de logging
│   └── Extensiones.cs              # Métodos auxiliares
│
├── Instalacion/
│   └── InstaladorServicio.cs       # Instalador de servicio Windows
│
└── Scripts/
    └── CrearBaseDatos.sql          # Script de creación de BD
```

## 🚀 Inicio Rápido

### 1. Prerrequisitos

- .NET 8.0 SDK
- SQL Server 2025 (o compatible)
- Visual Studio Code
- Permisos de administrador (para instalar el servicio)

### 2. Configurar Base de Datos

Ejecutar el script SQL en SQL Server Management Studio:

```bash
sqlcmd -S localhost -i Scripts/CrearBaseDatos.sql
```

O abrir y ejecutar manualmente el archivo `Scripts/CrearBaseDatos.sql`

### 3. Configurar appsettings.json

Editar `Configuracion/appsettings.json` con tus datos:

```json
{
  "IntervaloLecturaSegundos": 5,
  "Databases": {
    "Local": {
      "Enabled": true,
      "ConnectionString": "Server=.;Database=ProduccionLocal;Trusted_Connection=True;TrustServerCertificate=True;"
    }
  },
  "Plcs": [
    {
      "Nombre": "PLC1",
      "Ip": "192.168.0.10",
      "Puerto": 502,
      "Id": 1,
      "Habilitada": true
    }
  ]
}
```

### 4. Compilar el Proyecto

```bash
dotnet build
```

### 5. Ejecutar en Modo Consola (Debug)

```bash
dotnet run
```

O desde VS Code: Presionar `F5`

### 6. Instalar como Servicio de Windows

**Ejecutar como Administrador:**

```bash
# Compilar en modo Release
dotnet publish -c Release -o publish

# Instalar el servicio
cd publish
ProyectoServicioPLC.exe /install

# Iniciar el servicio
ProyectoServicioPLC.exe /start

# Ver estado
ProyectoServicioPLC.exe /status
```

## 🔧 Configuración

### Agregar Nuevos Datos al PLC

1. **En `appsettings.json`** - Agregar el mapeo:

```json
"MapeosDatos": {
  "Presion": { "Posicion": 112, "Tipo": "Float", "Descripcion": "Presión del sistema" }
}
```

2. **En `Modelos/LecturaPlc.cs`** - Agregar la propiedad:

```csharp
public float Presion { get; set; }
```

3. **En `Servicios/RepositorioDatos.cs`** - Agregar el parámetro:

```csharp
comando.Parameters.AddWithValue("@Presion", lectura.Presion);
```

4. **En la BD** - Agregar la columna:

```sql
ALTER TABLE LecturasPLC ADD Presion FLOAT NULL;
```

### Configurar Múltiples PLCs

Simplemente agregar más entradas en el array `Plcs`:

```json
"Plcs": [
  { "Nombre": "PLC1", "Ip": "192.168.0.10", "Puerto": 502, "Id": 1, "Habilitada": true },
  { "Nombre": "PLC2", "Ip": "192.168.0.11", "Puerto": 502, "Id": 2, "Habilitada": true },
  { "Nombre": "PLC3", "Ip": "192.168.0.12", "Puerto": 502, "Id": 3, "Habilitada": false }
]
```

## 📊 Consultas Útiles SQL

### Ver últimas 100 lecturas

```sql
SELECT * FROM vw_UltimasLecturas;
```

### Ver lecturas de un PLC específico

```sql
SELECT TOP 50 * 
FROM LecturasPLC 
WHERE PlcId = 1 
ORDER BY FechaHoraLectura DESC;
```

### Ver eventos del sistema

```sql
SELECT TOP 100 * 
FROM EventosSistema 
ORDER BY FechaHora DESC;
```

### Limpiar datos antiguos (90 días)

```sql
EXEC sp_LimpiarDatosAntiguos @DiasAntiguedad = 90;
```

## 🔍 Troubleshooting

### El servicio no inicia

- Verificar permisos de administrador
- Revisar logs en `Logs/servicio_plc.log`
- Verificar configuración de BD en `appsettings.json`

### No se conecta al PLC

- Verificar IP y puerto en `appsettings.json`
- Hacer ping al PLC: `ping 192.168.0.10`
- Revisar firewall
- Ver eventos en `Logs/eventos.log`

### Error de conexión a BD

- Verificar connection string
- Verificar que SQL Server esté corriendo
- Revisar permisos de la cuenta de servicio
- El servicio continúa funcionando con la BD disponible

## 📝 Comandos del Servicio

```bash
# Instalar
ProyectoServicioPLC.exe /install

# Desinstalar
ProyectoServicioPLC.exe /uninstall

# Iniciar
ProyectoServicioPLC.exe /start
# O: net start ServicioPLC

# Detener
ProyectoServicioPLC.exe /stop
# O: net stop ServicioPLC

# Ver estado
ProyectoServicioPLC.exe /status

# Ayuda
ProyectoServicioPLC.exe /help
```

## 🛠️ Desarrollo

### Agregar Cliente Modbus Real

El proyecto incluye simulación de lectura. Para implementar Modbus real:

1. Descomentar en `.csproj`:
```xml
<PackageReference Include="NModbus4" Version="3.0.74" />
```

2. Implementar en `LectorPlc.cs` las secciones marcadas con `// TODO:`

### Arquitectura Resiliente

El servicio está diseñado para NO detenerse ante:
- ❌ Fallo de un PLC → Continúa con los demás
- ❌ Fallo de BD local → Usa BD nube
- ❌ Fallo de BD nube → Usa BD local
- ❌ Pérdida de red → Reintenta automáticamente
- ❌ Timeouts → Registra y continúa

## 📄 Licencia

[Especifica tu licencia aquí]

## 👤 Autor

[Tu nombre aquí]

---

**Nota:** Este es un proyecto base. Recuerda implementar el cliente Modbus real según tu hardware específico.
