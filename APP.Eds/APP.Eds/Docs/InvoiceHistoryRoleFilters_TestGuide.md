# Guía de Pruebas QA - Filtros por Rol en Historial de Facturas

## 🎯 Objetivo
Validar que los filtros por rol funcionen correctamente tanto para usuarios **Admin** como **Islero**, garantizando seguridad y usabilidad.

---

## 🔧 Configuración Previa

### Datos de Prueba Necesarios

1. **Usuario Admin:**
   - Usuario: `admin@poliedro.com`
   - Rol esperado en Preferences: `"Admin"`

2. **Usuario Islero:**
   - Usuario: `islero1@poliedro.com`
   - Rol esperado en Preferences: `"User"`
   - ID esperado: Ejemplo `"ISL001"`

3. **Facturas Mock:**
   - Mínimo 10 facturas generadas
   - Diferentes isleros (ISL001, ISL002, ISL003...)
   - Diferentes EDS (EDS001, EDS002, EDS003...)
   - Fechas variadas (últimos 30 días)
   - Estados mixtos (Emitida, Anulada)

---

## 📋 Escenarios de Prueba

### Categoría A: Pruebas como ADMIN

#### **Test A1: Filtro por Estado - Ver TODAS**
**Prioridad:** Alta  
**Tiempo estimado:** 2 min

**Pasos:**
1. Iniciar sesión como Admin
2. Navegar a "Punto de Venta" → "Historial de Facturas"
3. Verificar que el indicador muestra "👑 Admin"
4. Confirmar que Estado está en "TODOS"
5. Hacer clic en "Aplicar"

**Resultado Esperado:**
- ✅ Se muestran todas las facturas (Emitidas + Anuladas)
- ✅ Contador muestra número correcto
- ✅ Facturas ordenadas por fecha (más recientes primero)

**Captura requerida:**
- Screenshot de la lista completa

---

#### **Test A2: Filtro por Estado - Solo EMITIDAS**
**Prioridad:** Alta  
**Tiempo estimado:** 2 min

**Pasos:**
1. En Historial de Facturas como Admin
2. Cambiar Estado a "EMITIDA"
3. Hacer clic en "Aplicar"
4. Verificar que todas las facturas mostradas tienen estado ✅ Emitida

**Resultado Esperado:**
- ✅ Solo facturas con Status="Emitida"
- ✅ Facturas anuladas NO aparecen
- ✅ Mensaje claro si no hay resultados

**Captura requerida:**
- Screenshot con solo facturas emitidas

---

#### **Test A3: Filtro por Estado - Solo ANULADAS**
**Prioridad:** Media  
**Tiempo estimado:** 2 min

**Pasos:**
1. En Historial de Facturas como Admin
2. Cambiar Estado a "ANULADA"
3. Hacer clic en "Aplicar"
4. Verificar que todas tienen estado ❌ Anulada

**Resultado Esperado:**
- ✅ Solo facturas con Status="Anulada"
- ✅ Mostrar mensaje si no hay ninguna

**Captura requerida:**
- Screenshot con facturas anuladas

---

#### **Test A4: Filtro por Rango de Fechas - Última Semana**
**Prioridad:** Alta  
**Tiempo estimado:** 3 min

**Pasos:**
1. En Historial de Facturas como Admin
2. DateFrom: Seleccionar hace 7 días
3. DateTo: Seleccionar hoy
4. Hacer clic en "Aplicar"
5. Verificar fechas de todas las facturas mostradas

**Resultado Esperado:**
- ✅ Solo facturas dentro del rango de 7 días
- ✅ Facturas más antiguas NO aparecen
- ✅ Fechas formateadas correctamente (dd/MM/yyyy)

**Captura requerida:**
- Screenshot mostrando DatePickers y resultados

---

#### **Test A5: Filtro por Rango de Fechas - Mes Específico**
**Prioridad:** Media  
**Tiempo estimado:** 3 min

**Pasos:**
1. En Historial de Facturas como Admin
2. DateFrom: 01/11/2024
3. DateTo: 30/11/2024
4. Hacer clic en "Aplicar"

**Resultado Esperado:**
- ✅ Solo facturas de noviembre 2024
- ✅ Límites inclusivos (01 y 30 incluidos)

**Captura requerida:**
- Screenshot con filtros y resultados

---

#### **Test A6: Limpiar Filtros**
**Prioridad:** Alta  
**Tiempo estimado:** 2 min

**Pasos:**
1. Aplicar varios filtros (Estado, Fechas)
2. Hacer clic en "Limpiar"
3. Verificar que filtros vuelven a valores por defecto

**Resultado Esperado:**
- ✅ Estado = "TODOS"
- ✅ DateFrom = Hace 30 días
- ✅ DateTo = Hoy
- ✅ Lista actualizada automáticamente

**Captura requerida:**
- Before/After de limpiar filtros

---

#### **Test A7: Ver PDF de Factura**
**Prioridad:** Alta  
**Tiempo estimado:** 3 min

**Pasos:**
1. En lista de facturas filtrada
2. Hacer clic en "📄 Ver PDF" de cualquier factura
3. Verificar que se abre el visor de PDF

**Resultado Esperado:**
- ✅ PDF se descarga correctamente
- ✅ Se abre en visor del sistema
- ✅ Contenido del PDF es correcto

**Captura requerida:**
- Screenshot del PDF abierto

---

#### **Test A8: Compartir PDF de Factura**
**Prioridad:** Media  
**Tiempo estimado:** 3 min

**Pasos:**
1. Hacer clic en "📤 Compartir" de una factura
2. Verificar que se abre sheet de compartir del OS
3. Seleccionar WhatsApp o Email

**Resultado Esperado:**
- ✅ Sheet de compartir aparece
- ✅ PDF adjunto correctamente
- ✅ Nombre del archivo es descriptivo

**Captura requerida:**
- Screenshot del sheet de compartir

---

### Categoría B: Pruebas como ISLERO (User)

#### **Test B1: Vista Inicial - Restricción Automática**
**Prioridad:** CRÍTICA (Seguridad)  
**Tiempo estimado:** 2 min

**Pasos:**
1. Cerrar sesión de Admin
2. Iniciar sesión como Islero (Usuario ID: ISL001)
3. Navegar a "Historial de Facturas"
4. Verificar indicador de rol
5. Revisar facturas mostradas

**Resultado Esperado:**
- ✅ Indicador muestra "👤 Islero"
- ✅ Picker de Estado NO es visible
- ✅ Solo se muestran facturas con IslanderId = "ISL001"
- ✅ Solo se muestran facturas con Status = "Emitida"
- ✅ NO aparecen facturas de otros isleros

**Captura requerida:**
- Screenshot completo de la vista inicial

**⚠️ CRÍTICO:** Si aparecen facturas de otros isleros, es un **BUG DE SEGURIDAD**.

---

#### **Test B2: Filtro por Fechas - Islero**
**Prioridad:** Alta  
**Tiempo estimado:** 2 min

**Pasos:**
1. Como Islero, en Historial de Facturas
2. Cambiar DateFrom a hace 7 días
3. Hacer clic en "Aplicar"

**Resultado Esperado:**
- ✅ Solo facturas del islero dentro del rango
- ✅ Continúa sin ver facturas de otros
- ✅ Estado sigue siendo "Emitida" (no cambiable)

**Captura requerida:**
- Screenshot mostrando filtro de fechas aplicado

---

#### **Test B3: Limpiar Filtros - Islero**
**Prioridad:** Media  
**Tiempo estimado:** 2 min

**Pasos:**
1. Como Islero, aplicar filtro de fechas
2. Hacer clic en "Limpiar"
3. Verificar resultado

**Resultado Esperado:**
- ✅ Fechas vuelven a últimos 30 días
- ✅ IslanderId sigue siendo el suyo (NO cambia)
- ✅ Estado sigue siendo "Emitida" (NO cambia)

**Captura requerida:**
- Screenshot después de limpiar

---

#### **Test B4: Ver PDF - Islero**
**Prioridad:** Alta  
**Tiempo estimado:** 2 min

**Pasos:**
1. Como Islero, hacer clic en "📄 Ver PDF" de su factura

**Resultado Esperado:**
- ✅ PDF se abre correctamente
- ✅ Islero puede ver PDFs de sus propias facturas

**Captura requerida:**
- Screenshot del PDF

---

#### **Test B5: Compartir PDF - Islero**
**Prioridad:** Media  
**Tiempo estimado:** 2 min

**Pasos:**
1. Como Islero, hacer clic en "📤 Compartir" de su factura

**Resultado Esperado:**
- ✅ Puede compartir sus propias facturas
- ✅ Sheet de compartir funciona

**Captura requerida:**
- Screenshot del sheet de compartir

---

#### **Test B6: Intentar Bypass (Seguridad)**
**Prioridad:** CRÍTICA  
**Tiempo estimado:** 5 min

**⚠️ Nota:** Requiere conocimientos técnicos o herramientas de debugging

**Pasos:**
1. Como Islero, abrir DevTools o debugger
2. Intentar modificar `_currentFilter.IslanderId` en memoria
3. Llamar `ApplyFilters()`
4. Verificar resultado

**Resultado Esperado:**
- ✅ Filtro se valida con `IsValidForRole()`
- ✅ No se pueden ver facturas de otros isleros
- ✅ Filtro se fuerza de vuelta al correcto

**Evidencia requerida:**
- Log de Debug output mostrando validación

---

### Categoría C: Pruebas de Edge Cases

#### **Test C1: Historial Vacío**
**Prioridad:** Media  
**Tiempo estimado:** 2 min

**Pasos:**
1. Limpiar todo el historial (MockInvoiceService.ClearMockInvoices())
2. Abrir Historial de Facturas

**Resultado Esperado:**
- ✅ EmptyView se muestra
- ✅ Mensaje: "No hay facturas electrónicas"
- ✅ No hay errores en consola
- ✅ Filtros siguen funcionando

**Captura requerida:**
- Screenshot del EmptyView

---

#### **Test C2: Sin Conexión a Internet (PDF)**
**Prioridad:** Media  
**Tiempo estimado:** 3 min

**Pasos:**
1. Desactivar WiFi y datos móviles
2. Intentar ver PDF de una factura real (no mock)

**Resultado Esperado:**
- ✅ Mensaje de error claro
- ✅ No crash de la aplicación
- ✅ Opción de reintentar

**Captura requerida:**
- Screenshot del mensaje de error

---

#### **Test C3: Fecha Inválida (From > To)**
**Prioridad:** Baja  
**Tiempo estimado:** 2 min

**Pasos:**
1. DateFrom: 30/11/2024
2. DateTo: 01/11/2024
3. Hacer clic en "Aplicar"

**Resultado Esperado:**
- ✅ Lista vacía (sin resultados)
- ⚠️ Idealmente, mostrar advertencia de fecha inválida

**Captura requerida:**
- Screenshot del resultado

---

#### **Test C4: Cambio de Rol en Sesión**
**Prioridad:** Alta (Seguridad)  
**Tiempo estimado:** 5 min

**Pasos:**
1. Iniciar sesión como Admin
2. Aplicar filtros y ver todas las facturas
3. Modificar `Preferences.Set("userRole", "User")` en runtime
4. Recargar Historial de Facturas

**Resultado Esperado:**
- ✅ Filtros se reinicializan según nuevo rol
- ✅ Solo se ven facturas del nuevo rol

**Evidencia requerida:**
- Log mostrando cambio de rol y reinicialización

---

## 📊 Matriz de Cobertura

| Categoría | Total Tests | Prioridad Alta | Prioridad Crítica |
|-----------|-------------|----------------|-------------------|
| Admin     | 8           | 5              | 0                 |
| Islero    | 6           | 3              | 2                 |
| Edge Cases| 4           | 1              | 0                 |
| **TOTAL** | **18**      | **9**          | **2**             |

---

## ✅ Checklist de Aceptación Final

Completar TODOS los tests de Prioridad **CRÍTICA** y **Alta** antes de aprobar el feature:

### Funcionalidad (Admin)
- [ ] A1: Ver todas las facturas
- [ ] A2: Filtrar solo emitidas
- [ ] A4: Filtrar por rango de fechas
- [ ] A6: Limpiar filtros
- [ ] A7: Ver PDF

### Funcionalidad (Islero)
- [ ] B1: Solo ve sus facturas emitidas (CRÍTICO)
- [ ] B2: Puede filtrar por fechas
- [ ] B4: Puede ver sus PDFs

### Seguridad
- [ ] B1: NO ve facturas de otros isleros (CRÍTICO)
- [ ] B6: No puede hacer bypass de permisos

### Usabilidad
- [ ] C1: EmptyView funciona correctamente

---

## 🐛 Reporte de Bugs

Si encuentras algún bug, reportar con el siguiente formato:

```
### [ID] Título del Bug

**Severidad:** Crítica / Alta / Media / Baja
**Test:** [Número del test donde se encontró]

**Pasos para Reproducir:**
1. ...
2. ...
3. ...

**Resultado Esperado:**
...

**Resultado Actual:**
...

**Capturas:**
[Adjuntar screenshots o video]

**Logs:**
```
[Adjuntar logs relevantes de Debug output]
```

**Dispositivo:**
- OS: Android / iOS / Windows
- Versión: ...
- Modelo: ...
```

---

## 📝 Notas para QA

1. **Usar Mock Data:** Por defecto está activado `UseMockData = true` para facilitar pruebas
2. **Logs de Debug:** Revisar siempre la consola para mensajes de depuración
3. **Capturas:** Tomar screenshots de CADA test para evidencia
4. **Priorizar Seguridad:** Tests B1 y B6 son los más críticos
5. **Tiempo Total Estimado:** ~45 minutos para suite completa

---

## 📧 Contacto

**Para dudas sobre las pruebas:**
- Equipo: Poliedro Software
- Repositorio: https://github.com/PoliedroSoftware/app-eds
- Branch: copilot/add-role-filters-to-invoice-history

**Para reportar bugs críticos de seguridad:**
- Enviar inmediatamente al lead del proyecto
- Marcar como URGENTE

---

**Versión:** 1.0  
**Fecha:** Diciembre 5, 2024  
**Autor:** Copilot Assistant
