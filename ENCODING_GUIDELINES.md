# 📝 Guía de Codificación de Caracteres UTF-8

## 🎯 Objetivo
Esta guía documenta las mejores prácticas para evitar problemas de codificación de caracteres especiales (tildes, ñ, etc.) en el proyecto app-eds.

## 🐛 Problema Identificado
Se detectaron errores de codificación donde los caracteres especiales del español se mostraban como � (carácter de reemplazo UTF-8).

### Ejemplos del problema:
- ❌ `Categor�a Creada` → ✅ `Categoría Creada`
- ❌ `La descripci�n` → ✅ `La descripción`
- ❌ `N�mero` → ✅ `Número`

## 🔍 Causa Raíz
Los archivos fuente contenían el carácter de reemplazo UTF-8 (bytes `0xEF 0xBF 0xBD`) en lugar de los caracteres españoles correctamente codificados:
- `á` debería ser `0xC3 0xA1` (no `0xEF 0xBF 0xBD`)
- `é` debería ser `0xC3 0xA9`
- `í` debería ser `0xC3 0xAD`
- `ó` debería ser `0xC3 0xB3`
- `ú` debería ser `0xC3 0xBA`
- `ñ` debería ser `0xC3 0xB1`

## ✅ Mejores Prácticas

### 1. Configuración del Editor
Asegúrese de que su editor de código esté configurado para usar UTF-8:

#### Visual Studio
1. Abrir: `Archivo` → `Opciones Avanzadas para Guardar`
2. Seleccionar: `Unicode (UTF-8 con firma) - Página de códigos 65001`

#### Visual Studio Code
```json
{
  "files.encoding": "utf8",
  "files.autoGuessEncoding": false
}
```

#### Rider
1. Ir a: `Settings` → `Editor` → `File Encodings`
2. Establecer: `Global Encoding: UTF-8`
3. Establecer: `Project Encoding: UTF-8`

### 2. Caracteres Especiales Comunes

| Carácter | Nombre | Código UTF-8 |
|----------|--------|--------------|
| á | a con tilde | U+00E1 |
| é | e con tilde | U+00E9 |
| í | i con tilde | U+00ED |
| ó | o con tilde | U+00F3 |
| ú | u con tilde | U+00FA |
| ñ | eñe | U+00F1 |
| ü | u con diéresis | U+00FC |
| ¿ | signo de interrogación inicial | U+00BF |
| ¡ | signo de exclamación inicial | U+00A1 |
| • | viñeta | U+2022 |

### 3. Validación de Archivos

#### Verificar codificación de un archivo:
```bash
file -bi nombre_archivo.cs
```
Resultado esperado: `text/plain; charset=utf-8`

#### Buscar caracteres de reemplazo:
```bash
grep -r "�" --include="*.cs" --include="*.xaml" .
```
Resultado esperado: Sin resultados (0 coincidencias)

#### Verificar que los caracteres españoles estén correctamente codificados:
```bash
grep -r "á\|é\|í\|ó\|ú\|ñ" --include="*.cs" .
```
Resultado esperado: Debe mostrar los caracteres correctamente

### 4. Regex para Validación de Entrada

Cuando se necesite validar entrada de texto con caracteres especiales:

```csharp
// Permitir letras españolas, espacios y guiones
string pattern = @"[^a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s-]";
string cleanText = Regex.Replace(inputText, pattern, "");
```

### 5. Conversión de Archivos Corruptos

Si encuentra archivos con codificación incorrecta:

```bash
# En Linux/Mac
iconv -f ISO-8859-1 -t UTF-8 archivo_corrupto.cs > archivo_corregido.cs

# En Windows PowerShell
Get-Content archivo_corrupto.cs | Set-Content -Encoding UTF8 archivo_corregido.cs
```

## 🚨 Señales de Alerta

Busque estos síntomas de problemas de codificación:

1. **Caracteres �** en el código fuente o en la interfaz
2. **Cajas o cuadrados** en lugar de letras con tilde
3. **Caracteres extraños** como `Ã±` en lugar de `ñ`
4. **Errores de compilación** en cadenas con tildes

## 🔧 Corrección de Archivos

### Archivos Corregidos en este Fix:
- `APP.Eds/APP.Eds/Services/Court/CourtService.cs` (14 correcciones)
- `APP.Eds/APP.Eds/Converters/DecimalFormatConverter.cs` (5 correcciones)
- `APP.Eds/APP.Eds/Converters/MenuIconConverter.cs` (2 correcciones)
- `APP.Eds/APP.Eds/UsesCases/Hose/HosePostView.xaml.cs` (7 correcciones)
- `APP.Eds/APP.Eds/UsesCases/Islander/IslanderPostView.xaml.cs` (7 correcciones)
- `APP.Eds/APP.Eds/UsesCases/Court/CourtDetailPage.xaml.cs` (1 corrección)
- `APP.Eds/APP.Eds/UsesCases/Category/CategoryPostView.xaml.cs` (7 correcciones)
- `APP.Eds/APP.Eds/UsesCases/Provider/ProviderPostView.xaml.cs` (1 corrección)
- `APP.Eds/APP.Eds/Components/PopUp/Category.xaml.cs` (12 correcciones)
- `APP.Eds/APP.Eds/Components/PopUp/AddCourtExpenditure.xaml.cs` (1 corrección)
- `APP.Eds/APP.Eds/Models/Court/CourtListItemModel.cs` (2 correcciones)

**Total: 59 correcciones de caracteres en 11 archivos**

## 🧪 Pruebas

Para verificar que los cambios funcionan correctamente:

1. **Compilar el proyecto** y verificar que no hay errores de sintaxis
2. **Ejecutar la aplicación** en un dispositivo físico o emulador
3. **Probar los módulos afectados:**
   - Categorías (crear categoría con nombre "Subsidiado")
   - Mangueras (verificar mensajes de error/éxito)
   - Islero (verificar validaciones)
   - Cierre de turno (verificar mensajes)

4. **Verificar que los mensajes se muestran correctamente:**
   - Alertas de error
   - Alertas de éxito
   - Confirmaciones
   - Tooltips y etiquetas

## 📚 Referencias

- [UTF-8 en Wikipedia](https://es.wikipedia.org/wiki/UTF-8)
- [.NET String Encoding](https://docs.microsoft.com/en-us/dotnet/api/system.text.encoding)
- [MAUI Localization](https://docs.microsoft.com/en-us/dotnet/maui/fundamentals/localization)

## 📝 Checklist para Pull Requests

Antes de enviar un PR con cambios en archivos .cs o .xaml:

- [ ] Verificar que el editor está configurado para UTF-8
- [ ] Buscar caracteres � en los archivos modificados
- [ ] Verificar que todos los caracteres especiales se muestran correctamente
- [ ] Probar los cambios en un dispositivo/emulador
- [ ] Confirmar que los mensajes se muestran correctamente en la UI

---

**Última actualización:** 29/11/2025  
**Autor:** Copilot  
**Issue relacionado:** #434
