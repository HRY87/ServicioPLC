# 📁 Estructura Completa del Proyecto

## Árbol de Archivos

```
ProyectoServicioPLC/
│
├── 📄 ProyectoServicioPLC.csproj     # Archivo de proyecto .NET
├── 📄 README.md                       # Documentación principal
├── 📄 .gitignore                      # Archivos ignorados por Git
│
├── 📂 .vscode/                        # Configuración de Visual Studio Code
│   ├── launch.json                    # Configuración de debug
│   └── tasks.json                     # Tareas de compilación
│
├── 📂 Configuracion/                  # Archivos de configuración
│   ├── appsettings.json              # ✏️ Configuración activa (editar)
│   ├── appsettings.ejemplo.json      # Plantilla de configuración
│   └── ConfiguracionManager.cs       # Gestor de configuración
│
├── 📂 Modelos/                        # Modelos de datos
│   └── LecturaPlc.cs                 # 👉 Agregar propiedades aquí
│       ├── LecturaPlc (clase)
│       ├── PlcDato (clase)
│       └── EstadoPlc (clase)
│
├── 📂 Servicios/                      # Lógica de negocio
│   ├── LectorPlc.cs                  # 👉 Implementar Modbus real aquí
│   ├── GestorLecturas.cs             # Coordinador de lecturas
│   └── RepositorioDatos.cs           # 👉 Agregar parámetros BD aquí
│
├── 📂 Eventos/                        # Sistema de eventos
│   └── GestorEventos.cs
│       ├── TiposEvento (enum)
│       ├── GestorEventos (static)
│       └── EventoPlc (clase)
│
├── 📂 Mensajes/                       # Mensajes en consola
│   └── GestorMensajes.cs
│
├── 📂 Utilidades/                     # Funciones auxiliares
│   ├── Logger.cs                     # Sistema de logging
│   └── Extensiones.cs                # Métodos de extensión
│
├── 📂 Instalacion/                    # Instalador del servicio
│   └── InstaladorServicio.cs
│       ├── InstaladorServicio (clase)
│       └── GestorInstalacion (static)
│
├── 📂 Scripts/                        # Scripts de utilidad
│   ├── CrearBaseDatos.sql            # 👉 Agregar columnas BD aquí
│   ├── Instalar.bat                  # Instalador Windows (batch)
│   ├── Desinstalar.bat               # Desinstalador (batch)
│   └── Instalar.ps1                  # Instalador PowerShell
│
├── 📂 Logs/                           # Logs generados (auto-creado)
│   ├── servicio_plc.log              # Log principal
│   ├── eventos.log                   # Log de eventos
│   └── .gitkeep
│
├── 📄 Program.cs                      # Punto de entrada principal
└── 📄 MiServicio.cs                   # Servicio Windows
```

## 🎯 Archivos Clave para Personalizar

### 1. **appsettings.json** ⭐
**Ubicación:** `Configuracion/appsettings.json`

**Qué editar:**
- IP y puerto de tus PLCs
- String de conexión a SQL Server
- Intervalo de lectura (segundos)
- Mapeo de variables del PLC

```json
{
  "IntervaloLecturaSegundos": 5,
  "Plcs": [
    { "Ip": "192.168.0.10", ... }  // ← Cambiar IP
  ],
  "MapeosDatos": {
    "TuVariable": { "Posicion": 100, "Tipo": "Float" }  // ← Agregar variables
  }
}
```

---

### 2. **LecturaPlc.cs** 👉
**Ubicación:** `Modelos/LecturaPlc.cs`

**Qué agregar:** Nuevas propiedades para datos del PLC

```csharp
public class LecturaPlc
{
    // Propiedades existentes...
    public float KgHoraActual { get; set; }
    
    // 👉 AGREGAR AQUÍ:
    public float Presion { get; set; }
    public float Humedad { get; set; }
}
```

---

### 3. **RepositorioDatos.cs** 👉
**Ubicación:** `Servicios/RepositorioDatos.cs`

**Qué agregar:** Parámetros SQL para nuevas columnas

```csharp
// En el método EjecutarInsertAsync
comando.Parameters.AddWithValue("@KgHoraActual", lectura.KgHoraActual);

// 👉 AGREGAR AQUÍ:
comando.Parameters.AddWithValue("@Presion", lectura.Presion);
```

---

### 4. **CrearBaseDatos.sql** 👉
**Ubicación:** `Scripts/CrearBaseDatos.sql`

**Qué agregar:** Nuevas columnas en la tabla

```sql
CREATE TABLE LecturasPLC (
    KgHoraActual FLOAT NOT NULL,
    
    -- 👉 AGREGAR AQUÍ:
    Presion FLOAT NULL,
    Humedad FLOAT NULL,
    ...
)
```

---

### 5. **LectorPlc.cs** 🔧
**Ubicación:** `Servicios/LectorPlc.cs`

**Qué implementar:** Cliente Modbus real

```csharp
// TODO: Implementar cliente Modbus real
// private ModbusClient _clienteModbus;

// Actualmente usa simulación
```

---

## 📋 Checklist de Personalización

Cuando agregues una nueva variable del PLC:

- [ ] ✏️ Agregar en `appsettings.json` → sección `MapeosDatos`
- [ ] ✏️ Agregar propiedad en `Modelos/LecturaPlc.cs`
- [ ] ✏️ Agregar parámetro en `Servicios/RepositorioDatos.cs`
- [ ] ✏️ Agregar columna SQL en `Scripts/CrearBaseDatos.sql`
- [ ] 🔄 Ejecutar script SQL o `ALTER TABLE` manual
- [ ] 🔄 Recompilar el proyecto: `dotnet build`
- [ ] 🔄 Reinstalar el servicio (o reiniciar si ya está instalado)

---

## 🚀 Flujo de Desarrollo

### Modo Desarrollo (Consola)
```bash
# Ejecutar directamente
dotnet run

# O desde VS Code
Presionar F5
```

### Compilar para Producción
```bash
dotnet publish -c Release -o publish
```

### Instalar como Servicio
```bash
# Opción 1: Script Batch (Windows)
Scripts\Instalar.bat

# Opción 2: Script PowerShell
Scripts\Instalar.ps1

# Opción 3: Manual
cd publish
ProyectoServicioPLC.exe /install
net start ServicioPLC
```

---

## 📊 Archivos Generados en Tiempo de Ejecución

```
Logs/
├── servicio_plc.log              # Log técnico del servicio
├── servicio_plc.log.*.bak        # Backups automáticos
├── eventos.log                    # Eventos del sistema
└── eventos.log.*.bak             # Backups de eventos
```

**Rotación automática:**
- Los logs rotan cuando superan 10 MB
- Se mantienen los últimos 5 backups
- Limpieza automática de archivos > 30 días

---

## 🔧 Configuración de VS Code

### launch.json
Permite debuggear presionando F5

### tasks.json
Tareas disponibles:
- `Ctrl+Shift+B` → Build
- `dotnet publish` → Publicar Release
- `dotnet watch` → Compilación automática
- `dotnet clean` → Limpiar archivos generados

---

## 📦 Dependencias NuGet

```xml
<PackageReference Include="System.Data.SqlClient" Version="4.8.6" />
<PackageReference Include="System.ServiceProcess.ServiceController" Version="8.0.0" />
```

**Por implementar:**
```xml
<!-- Descomentar cuando implementes Modbus -->
<!-- <PackageReference Include="NModbus4" Version="3.0.74" /> -->
```

---

## 🎨 Características de Diseño

### ✅ Resiliente
- No se detiene si falla un PLC
- No se detiene si falla una BD
- Reconexión automática

### ✅ Mantenible
- Configuración centralizada en JSON
- Logs detallados
- Código modular y comentado

### ✅ Eficiente
- Lecturas en paralelo (async/await)
- Task.WhenAll para múltiples PLCs
- No bloquea BD si una falla

### ✅ Monitoreable
- Eventos registrados
- Mensajes informativos en consola
- Sistema de logging robusto

---

## 📝 Notas Importantes

1. **Permisos:** El servicio corre con `LocalSystem` por defecto
2. **Puerto Modbus:** Por defecto 502 (protocolo estándar)
3. **Intervalo mínimo:** Recomendado >= 1 segundo
4. **BD Dual:** Puedes usar solo local, solo nube, o ambas
5. **Simulación:** Actualmente simula lecturas, implementar Modbus real

---

## 🆘 Solución de Problemas

### El servicio no compila
```bash
dotnet restore
dotnet clean
dotnet build
```

### Error de conexión a BD
- Verificar SQL Server corriendo
- Verificar connection string
- Verificar permisos del usuario
- Ver logs en `Logs/servicio_plc.log`

### No se conecta al PLC
- Hacer ping a la IP del PLC
- Verificar puerto 502 abierto
- Verificar firewall
- Ver logs en `Logs/eventos.log`

---

**Última actualización:** Enero 2026  
**Versión:** 1.0.0
