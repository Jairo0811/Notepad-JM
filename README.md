# 📝 Notepad JM

**Notepad JM** es un editor de texto moderno para Windows, desarrollado con **C#**, **WPF** y **.NET 10**.

El proyecto nació como una tarea de la asignatura **Diseño Centrado en el Usuario (DCU)** en ITLA y fue completamente reescrito para convertirlo en una aplicación de escritorio moderna, mantenible y apta para portafolio.

## ✨ Funcionalidades

- Edición mediante múltiples pestañas.
- Nuevo, abrir, guardar, guardar como y guardar todo.
- Guardado seguro mediante archivo temporal.
- Confirmación de cambios antes de cerrar.
- Apertura de múltiples archivos.
- Arrastrar y soltar archivos sobre la ventana.
- Deshacer, rehacer, cortar, copiar, pegar y seleccionar todo.
- Buscar, reemplazar y reemplazar todas las coincidencias.
- Inserción rápida de fecha y hora.
- Ajuste de línea configurable.
- Zoom entre 8 y 48 puntos.
- Temas claro y oscuro.
- Corrector ortográfico integrado.
- Contador de líneas, palabras y caracteres.
- Barra de estado con ruta y nivel de zoom.
- Archivos recientes persistidos localmente.
- Recuperación automática de documentos sin guardar.
- Compatibilidad con TXT, Markdown, JSON, XML, HTML, CSS, JavaScript, TypeScript, C#, Java, Python y SQL.
- Atajos de teclado para las operaciones principales.

## 🧱 Arquitectura

La solución utiliza una arquitectura WPF modular y pragmática:

```text
Notepad-JM/
├── src/
│   └── NotepadJM/
│       ├── Models/
│       ├── App.xaml
│       ├── FindReplaceWindow.xaml
│       ├── MainWindow.xaml
│       └── NotepadJM.csproj
├── NotepadJM.sln
└── README.md
```

La lógica de documentos se encapsula en `DocumentTab`, mientras que las interacciones específicas de la interfaz se mantienen en las ventanas WPF. Esta separación evita sobrearquitectura sin sacrificar mantenibilidad.

## 🛠️ Tecnologías

- C#
- .NET 10
- WPF
- XAML
- System.Text.Json

## ▶️ Ejecución

### Requisitos

- Windows 10 u 11.
- .NET 10 SDK.
- Visual Studio 2026 o una versión compatible con .NET 10 y WPF.

### Desde la terminal

```powershell
git clone https://github.com/Jairo0811/Notepad-JM.git
cd Notepad-JM
dotnet restore
dotnet run --project .\src\NotepadJM\NotepadJM.csproj
```

### Compilar

```powershell
dotnet build .\NotepadJM.sln
```

### Publicar una versión portable

```powershell
dotnet publish .\src\NotepadJM\NotepadJM.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

## ⌨️ Atajos principales

| Acción | Atajo |
|---|---|
| Nuevo documento | `Ctrl + N` |
| Abrir | `Ctrl + O` |
| Guardar | `Ctrl + S` |
| Guardar como | `Ctrl + Shift + S` |
| Cerrar pestaña | `Ctrl + W` |
| Buscar y reemplazar | `Ctrl + H` |
| Insertar fecha y hora | `F5` |
| Acercar | `Ctrl + +` |
| Alejar | `Ctrl + -` |
| Restablecer zoom | `Ctrl + 0` |

## 🗂️ Datos locales

Notepad JM guarda sus preferencias y archivos de recuperación en:

```text
%LOCALAPPDATA%\NotepadJM
```

Los documentos recuperables se eliminan después de guardarse correctamente o cerrar la aplicación de forma segura.

## 🧭 Historia

La primera versión fue creada como una práctica académica básica con Windows Forms. La versión 2.0 reemplaza completamente esa implementación por una aplicación WPF moderna, conservando el propósito original del proyecto y demostrando su evolución técnica.

## 👨‍💻 Autor

**Francis Jairo Matías Rosario**  
Tecnólogo en Desarrollo de Software e Ingeniero de Software en formación.

## 📄 Licencia

Este proyecto puede utilizarse con fines educativos y de portafolio. Se recomienda agregar una licencia formal antes de distribuirlo comercialmente.
