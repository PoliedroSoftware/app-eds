# Guía de Pruebas QA - Aplicación EDS

## 📋 Descripción General
Esta guía proporciona instrucciones paso a paso para realizar pruebas de control de calidad (QA) en la aplicación de gestión de estaciones de servicio (EDS). Incluye escenarios de prueba para diferentes tipos de usuarios y funcionalidades administrativas clave.

## 👥 Usuarios de Prueba

La aplicación cuenta con los siguientes usuarios de prueba configurados:

| Usuario   | Contraseña | Rol          | Descripción                                    |
|-----------|------------|--------------|------------------------------------------------|
| admin     | admin      | Administrador| Usuario administrador principal                |
| medellin  | 1234       | Administrador| Usuario administrador alternativo              |
| islander  | islander   | Islero       | Usuario operador (rol limitado)                |
| apolo     | 123456     | Usuario      | Usuario de prueba adicional                    |

---

## 🔐 Pruebas de Inicio de Sesión

### Escenario 1: Login como Administrador Principal

**Usuario:** `admin`  
**Contraseña:** `admin`

#### Pasos:
1. Abrir la aplicación EDS en el dispositivo móvil o emulador
2. Verificar que aparezca la pantalla de inicio de sesión
3. En el campo **"Usuario"**, ingresar: `admin`
4. En el campo **"Contraseña"**, ingresar: `admin`
5. Presionar el botón **"Iniciar Sesión"** o **"Login"**
6. Esperar a que la aplicación procese las credenciales

#### Resultado Esperado:
- ✅ La aplicación debe iniciar sesión exitosamente
- ✅ Se debe mostrar el menú principal con todas las opciones administrativas
- ✅ El rol debe ser "Admin" (verificable en el perfil o configuración)
- ✅ Deben estar visibles todas las categorías del menú:
  - Administración
  - Configuración Inicial
  - Punto de Venta
  - Listado de Pagos QR
  - Power BI Dashboard
  - Control IoT
  - Dispensadores y mangueras
  - Compras y productos
  - Tanques y compartimentos
  - EDS y otros
  - Inventario

---

### Escenario 2: Login como Administrador Alternativo (Medellín)

**Usuario:** `medellin`  
**Contraseña:** `1234`

#### Pasos:
1. Si hay una sesión activa, cerrar sesión primero
2. En la pantalla de inicio de sesión, ingresar:
   - Usuario: `medellin`
   - Contraseña: `1234`
3. Presionar el botón de inicio de sesión
4. Esperar la validación

#### Resultado Esperado:
- ✅ Acceso exitoso al sistema
- ✅ Permisos de administrador completos
- ✅ Acceso a todas las funcionalidades administrativas
- ✅ Menú idéntico al del usuario admin

---

### Escenario 3: Login como Islero (Operador)

**Usuario:** `islander`  
**Contraseña:** `islander`

#### Pasos:
1. Cerrar sesión si hay una activa
2. En la pantalla de login:
   - Usuario: `islander`
   - Contraseña: `islander`
3. Iniciar sesión

#### Resultado Esperado:
- ✅ Login exitoso
- ✅ El rol debe ser "User" o "Islero"
- ✅ El menú debe estar vacío o con opciones muy limitadas
- ✅ NO debe tener acceso a opciones administrativas
- ✅ Solo puede realizar operaciones básicas asignadas a isleros

**Nota:** El rol de islero tiene permisos limitados intencionalmente para operaciones de estación únicamente.

---

### Escenario 4: Login como Usuario Apolo

**Usuario:** `apolo`  
**Contraseña:** `123456`

#### Pasos:
1. Cerrar cualquier sesión activa
2. Ingresar credenciales:
   - Usuario: `apolo`
   - Contraseña: `123456`
3. Presionar botón de login

#### Resultado Esperado:
- ✅ Acceso al sistema confirmado
- ✅ Verificar el rol asignado en el perfil
- ✅ Validar permisos correspondientes al rol
- ✅ Confirmar acceso a las funcionalidades permitidas

---

## 💰 Menú Administrativo Especial: EL CORTE

El **"Corte"** es una funcionalidad administrativa crítica que permite realizar el cierre de caja y gestión financiera de la estación de servicio.

### Acceso al Módulo de Corte

#### Prerrequisitos:
- ⚠️ **Debe tener permisos de Administrador**
- ⚠️ Iniciar sesión con usuario `admin/admin` o `medellin/1234`

#### Pasos para Acceder:
1. Iniciar sesión como administrador
2. En el menú principal, localizar la categoría **"Administración"** (icono ⚙️)
3. Hacer clic en la categoría "Administración"
4. Se desplegará un submenú con las siguientes opciones:
   - **Corte** 💰
   - Negocio 🏢
   - EDS 🏪
   - Caja Fuerte 💼
   - Teléfonos 📱
5. Seleccionar **"Corte"**
6. Esperar a que cargue la vista del módulo de Corte

#### Resultado Esperado:
- ✅ Se debe abrir la pantalla de gestión de Corte
- ✅ La interfaz debe mostrar opciones para realizar el corte de caja

---

## 📊 Pruebas Detalladas del Módulo de Corte

### Flujo Completo del Corte de Caja

Esta es una prueba integral que simula un cierre de caja completo con todas sus operaciones.

#### Paso 1: Configuración Inicial del Corte

**Objetivo:** Seleccionar el negocio, EDS e islero para el corte

**Acciones:**
1. En la pantalla de Corte, hacer clic en el botón **"Nuevo Corte"** o similar
2. Esperar 5 segundos a que cargue el formulario
3. Hacer clic en el selector **"Negocio"**
4. Seleccionar un negocio de la lista
5. Esperar 3 segundos
6. Hacer clic en el selector **"EDS"** 
7. Seleccionar una estación de servicio (EDS)
8. Esperar 2 segundos
9. Hacer clic en el selector **"Islero"**
10. Seleccionar un islero de la lista
11. Esperar 3 segundos

**Resultado Esperado:**
- ✅ Todos los selectores deben cargar opciones correctamente
- ✅ Las selecciones deben guardarse
- ✅ No debe haber errores en la carga de datos

---

#### Paso 2: Registrar Nueva Venta

**Objetivo:** Agregar una venta al corte

**Acciones:**
1. Desplazarse hacia abajo si es necesario (scroll)
2. Hacer clic en el botón **"Nueva Venta"** o **"Agregar Venta"**
3. Esperar 2 segundos
4. Seleccionar **"Tipo de Manguera"** o **"Hose"**
5. Elegir una manguera de la lista
6. Esperar 2 segundos
7. Localizar el campo **"Cantidad Acumulada"** o **"Amount Accumulate"**
8. Hacer clic en el campo
9. Ingresar el valor: `1`
10. Presionar Enter o confirmar
11. Esperar 2 segundos
12. Hacer clic en el botón **"Agregar"** o **"Add"**
13. Esperar 2 segundos

**Resultado Esperado:**
- ✅ La venta debe agregarse correctamente
- ✅ Debe aparecer en la lista de ventas del corte
- ✅ Los cálculos deben actualizarse automáticamente

---

#### Paso 3: Agregar Recaudo (Colección)

**Objetivo:** Registrar un ingreso en efectivo

**Acciones:**
1. Hacer clic en el botón **"Agregar Recaudo"** o **"Add Collection"**
2. Esperar 2 segundos
3. Hacer clic en **"Tipo de Recaudo"** o **"Type Collection"**
4. Seleccionar **"Efectivo"** o **"Cash"**
5. Esperar 2 segundos
6. En el campo **"Valor"** o **"Value"**, ingresar: `1`
7. Presionar Enter
8. Esperar 2 segundos
9. En el campo **"Descripción"** o **"Description"**, ingresar: `PruebaTestAuto`
10. Hacer clic en el botón **"Enviar Recaudo"** o **"Send Collection"**
11. Esperar 2 segundos

**Resultado Esperado:**
- ✅ El recaudo debe guardarse exitosamente
- ✅ Debe aparecer en el listado de recaudos
- ✅ El total de ingresos debe actualizarse

---

#### Paso 4: Registrar Nuevo Egreso

**Objetivo:** Agregar un gasto al corte

**Acciones:**
1. Desplazarse hacia abajo (scroll to end)
2. Esperar 2 segundos
3. Hacer clic en el botón **"Nuevo Egreso"** o **"New Egress"**
4. Esperar 2 segundos
5. Seleccionar **"Tipo de Gasto"** o **"Expenditure Type"**
6. Elegir **"Flete"** de la lista
7. Esperar 2 segundos
8. En el campo **"Valor"** o **"Amount"**, ingresar: `1`
9. Presionar Enter
10. En el campo **"Descripción"**, ingresar: `PruebaTestAuto`
11. Esperar 2 segundos
12. Hacer clic en el botón **"Agregar Gasto"** o **"Add Expense"**
13. Desplazarse hasta el final de la pantalla
14. Hacer clic en el botón **"Enviar Datos"** o **"Send Data"**
15. Esperar 3 segundos

**Resultado Esperado:**
- ✅ El egreso debe registrarse correctamente
- ✅ Debe aparecer en la lista de egresos
- ✅ El cálculo del balance debe actualizarse
- ✅ Puede aparecer un diálogo de confirmación

---

#### Paso 5: Finalización y Verificación

**Acciones:**
1. Si aparece un diálogo de confirmación:
   - Hacer clic en el botón **"OK"** o **"Aceptar"**
2. Hacer clic en el botón **"Abrir Lista"** o **"Open List"**
3. Esperar 5 segundos a que cargue el historial
4. Verificar que el corte aparezca en la lista
5. Hacer clic en el botón de navegación **"Atrás"** o **"Volver"**
6. Esperar 2 segundos
7. Hacer clic nuevamente en **"Atrás"** para volver al menú principal

**Resultado Esperado:**
- ✅ El corte debe guardarse en el sistema
- ✅ Debe aparecer en el historial de cortes
- ✅ Los datos deben ser consistentes
- ✅ La navegación debe funcionar correctamente
- ✅ Mensaje de éxito: **"Court testing Success!!"** (en pruebas automatizadas)

---

## 🧪 Escenarios de Prueba Adicionales

### Validación de Errores de Login

#### Credenciales Incorrectas

**Pasos:**
1. Ingresar usuario: `admin`
2. Ingresar contraseña incorrecta: `incorrect123`
3. Intentar iniciar sesión

**Resultado Esperado:**
- ❌ El login debe fallar
- ❌ Debe mostrar mensaje de error: "Usuario o contraseña incorrectos"
- ❌ No debe permitir acceso al sistema

---

#### Campos Vacíos

**Pasos:**
1. Dejar el campo usuario vacío
2. Dejar el campo contraseña vacío
3. Intentar iniciar sesión

**Resultado Esperado:**
- ❌ Debe mostrar validación de campos requeridos
- ❌ No debe procesar el login

---

### Pruebas de Seguridad y Roles

#### Intento de Acceso No Autorizado

**Pasos:**
1. Iniciar sesión como `islander/islander`
2. Intentar acceder al menú de Administración
3. Intentar acceder al módulo de Corte

**Resultado Esperado:**
- ❌ NO debe tener acceso visible al menú Administración
- ❌ Si intenta acceder por URL directa, debe rechazar el acceso
- ❌ Debe mostrar mensaje de permisos insuficientes

---

## 📱 Consideraciones de Prueba en Dispositivos Móviles

### Dispositivos Recomendados:
- Android (versión 8.0 o superior)
- Emulador Android Studio
- Dispositivos físicos: Samsung, Xiaomi, Huawei, etc.

### Aspectos a Verificar:
- ✅ Responsive design en diferentes tamaños de pantalla
- ✅ Rotación de pantalla (portrait/landscape)
- ✅ Rendimiento y velocidad de carga
- ✅ Consumo de batería
- ✅ Funcionamiento offline/online
- ✅ Persistencia de sesión al cerrar y reabrir la app

---

## 🔍 Pruebas de Regresión

Después de cualquier cambio en el código, verificar:

- [ ] Login funciona con todos los usuarios de prueba
- [ ] Cambio entre modo claro y oscuro
- [ ] Navegación entre pantallas
- [ ] Formularios de entrada de datos
- [ ] Operaciones de negocio críticas
- [ ] Solicitudes de red al backend
- [ ] Módulo de Corte completo

---

## 🐛 Reporte de Errores

### Información a Incluir:

1. **Usuario utilizado:** (admin, medellin, islander, apolo)
2. **Pasos para reproducir:** Detalle paso a paso
3. **Resultado esperado:** Qué debería suceder
4. **Resultado actual:** Qué sucedió realmente
5. **Capturas de pantalla:** Adjuntar imágenes del error
6. **Logs:** Incluir logs de depuración si están disponibles
7. **Dispositivo:** Modelo, versión de Android
8. **Versión de la app:** Verificar en Acerca de/About

### Ejemplo de Reporte:

```
Título: Error al agregar venta en módulo de Corte

Usuario: admin/admin
Dispositivo: Samsung Galaxy S21, Android 12
Versión App: v1.0.1

Pasos:
1. Login como admin
2. Ir a Administración > Corte
3. Seleccionar Negocio, EDS, Islero
4. Clic en "Nueva Venta"
5. Seleccionar manguera
6. Error aparece al ingresar cantidad

Esperado: Debe permitir ingresar cantidad numérica
Actual: La app se cierra inesperadamente
```

---

## ✅ Checklist de Pruebas Completas

### Pruebas de Login:
- [ ] Login admin/admin - Exitoso
- [ ] Login medellin/1234 - Exitoso
- [ ] Login islander/islander - Exitoso
- [ ] Login apolo/123456 - Exitoso
- [ ] Login con credenciales incorrectas - Falla apropiadamente
- [ ] Login con campos vacíos - Validación correcta

### Pruebas del Módulo de Corte:
- [ ] Acceso al menú de Administración
- [ ] Apertura del módulo de Corte
- [ ] Selección de Negocio
- [ ] Selección de EDS
- [ ] Selección de Islero
- [ ] Agregar nueva venta
- [ ] Agregar recaudo
- [ ] Agregar egreso
- [ ] Enviar datos del corte
- [ ] Visualizar historial de cortes
- [ ] Navegación de regreso al menú

### Pruebas de Seguridad:
- [ ] Usuario islander no puede acceder a funciones admin
- [ ] Cierre de sesión funciona correctamente
- [ ] Timeout de sesión (si aplica)
- [ ] Tokens de autenticación se manejan correctamente

---

## 📞 Soporte

Para preguntas o problemas durante las pruebas:
- Revisar los logs de depuración
- Consultar documentación técnica en `/APP.Eds/APP.Eds/Docs/`
- Contactar al equipo de desarrollo

---

## 📝 Historial de Cambios

| Fecha | Versión | Cambios |
|-------|---------|---------|
| 2025-11-28 | 1.0 | Creación inicial del documento de pruebas QA |

---

**Última actualización:** 28 de noviembre de 2025

**Preparado por:** Equipo de QA - Poliedro Software

---

## 🎯 Criterios de Éxito

Las pruebas se consideran exitosas si:

✅ Todos los usuarios pueden iniciar sesión correctamente  
✅ Los roles y permisos funcionan como se espera  
✅ El módulo de Corte completa el flujo sin errores  
✅ No hay caídas de la aplicación (crashes)  
✅ Los datos se guardan y persisten correctamente  
✅ La navegación es fluida y sin errores  
✅ Los mensajes de error son claros y apropiados  
✅ La seguridad y validación de roles funciona correctamente  

---

**¡Gracias por realizar las pruebas de calidad!** 🚀
