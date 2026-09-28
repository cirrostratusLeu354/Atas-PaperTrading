# Atas Paper Trading

**Paper Trading Simulator** es un indicador personalizado para la plataforma ATAS (Advanced Trading Analytical Software) diseñado para simular operaciones de trading en tiempo real o en datos históricos mediante accesos directos de teclado y ratón. Permite gestionar un balance virtual, aplicar apalancamiento, configurar órdenes Stop Loss / Take Profit dinámicas y visualizar el PnL e información de cuenta directamente en el gráfico.

---

## Características Principales

* **Simulación en Tiempo Real/Histórica:** Permite abrir y cerrar posiciones Market (Buy/Sell) directamente sobre el gráfico de precios.
* **Gestión de Riesgo y Apalancamiento:** Soporte para apalancamiento ajustable ($1\text{x}$ a $150\text{x}$) y cálculo automático del precio de liquidación.
* **Stop Loss & Take Profit Visuales:** Configuración e interacción directa mediante clics de ratón en el gráfico.
* **HUD Informativo:** Renderizado en pantalla del balance actual, equidad (equity), margen de entrada utilizado y PnL/RoI en tiempo real.

---

## Controles del Simulador

Los controles del indicador combinan modificadores de teclado con acciones del ratón dentro de la interfaz del gráfico:

### Accesos Directos de Teclado (Atajos con `Shift`)

Para ejecutar cualquiera de las siguientes acciones, mantén presionada la tecla **`Shift`** y presiona la tecla correspondiente:

| Tecla | Acción |
| :--- | :--- |
| **Shift + W** | Abrir posición de **Compra a Mercado** (*Market Buy*). |
| **Shift + S** | Abrir posición de **Venta a Mercado** (*Market Sell*). |
| **Shift + A** | **Cierre rápido (*Quick Close*):** Cierra inmediatamente todas las posiciones abiertas y resetea las líneas de TP/SL. |
| **Shift + R** | Incrementar apalancamiento en $+1$ (`máximo x150`). |
| **Shift + E** | Reducir apalancamiento en $-1$ (mínimo $1\text{x}$). |
| **Shift + F** | Incrementar el margen de entrada en $+0.01$. |
| **Shift + D** | Reducir el margen de entrada en $-0.01$. |
| **Shift + C** | Incrementar el margen de entrada progresivamente (por porcentaje del margen actual). |
| **Shift + X** | Reducir el margen de entrada progresivamente. |
| **Shift + Z** | **Reiniciar simulador:** Limpia todas las posiciones y restaura el balance inicial original. |

---

### Controles de Ratón e Interacción Gráfica

* **Doble Clic Izquierdo:** Establece una línea de **Stop Loss** o **Take Profit** en el nivel de precio marcado por el cursor.
  * Si la posición es de compra (`Market Buy`), hacer clic por debajo del precio actual define el **Stop Loss**; por encima define el **Take Profit**.
  * Si la posición es de venta (`Market Sell`), los roles se invierten de forma automática.
  * Hacer doble clic sobre una línea existente de TP/SL la eliminará.
* **Shift + Clic Central (Botón de la rueda):** Asigna dinámicamente un nivel de TP/SL siguiendo la posición actual del puntero.

---

## Configuración e Instalación

1. Copia el archivo `.cs` en la carpeta de indicadores personalizados de ATAS (generalmente en `Documents/ATAS/Indicators`).
2. Compila el indicador dentro de la plataforma ATAS.
3. Añade **Paper Trading Simulator** al gráfico desde el menú de indicadores (**Trading / Simulation**).
4. Configura el **Balance** inicial deseado y el **Apalancamiento** desde el panel de propiedades del indicador.
