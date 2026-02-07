<template>
  <div class="table-container">
    <table class="custom-table">
      <thead>
        <tr>
          <th class="header-empty"></th>
          <th class="header-empty"></th>
          <th class="header-group">Игрок 1</th>
          <th class="header-group">Игрок 2</th>
          <th class="header-group">Игрок 3</th>
          <th class="header-group">Игрок 4</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="row in 14"
            :key="row"
            :class="`row-${row} row-color-blue`">
          <td>{{ `здание ${row}` }}</td>
          <td v-if="row === 1" :rowspan="14" class="merged-cell">
            Здания,<br>окруженные стеной х2
          </td>
          <td v-if="row !== 1" class="hidden-cell"></td>
          <td>Строка {{ row }}, Колонка 3</td>
          <td>Строка {{ row }}, Колонка 4</td>
          <td>Строка {{ row }}, Колонка 5</td>
          <td>Строка {{ row }}, Колонка 6</td>
        </tr>

        <tr class="row-15 row-color-yellow">
          <td colspan="2">Бонусы рынка 1/</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>
        <tr class="row-16 row-color-yellow">
          <td colspan="2">Бонусы Гильдии 1/</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>
        <tr class="row-17 row-color-yellow">
          <td colspan="2">Бонусы университета 1/</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>
        <tr class="row-18 row-color-yellow">
          <td colspan="2">Бонусы Собора 1/</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>

        <tr v-for="row in 2"
            :key="row + 18"
            :class="`row-${row + 18} row-color-yellow`">
          <td colspan="2">&nbsp;</td>
          <td></td>
          <td></td>
          <td></td>
          <td></td>
        </tr>

        <tr class="row-21 row-color-green">
          <td colspan="2">Культура 1/иконка</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>
        <tr class="row-22 row-color-green">
          <td colspan="2">Наиболее развитая культура (5)</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>

        <tr class="row-23 row-color-purple">
          <td colspan="2">Наибольшая область окруженная стеной (10)</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>

        <tr class="row-24 row-color-white">
          <td colspan="2" class="black-bg">Промежуточный итог</td>
          <td>Строка 24, Колонка 3</td>
          <td>Строка 24, Колонка 4</td>
          <td>Строка 24, Колонка 5</td>
          <td>Строка 24, Колонка 6</td>
        </tr>

        <tr class="row-25 row-color-green">
          <td colspan="2">Очки бедствий</td>
          <td>Строка 25, Колонка 3</td>
          <td>Строка 25, Колонка 4</td>
          <td>Строка 25, Колонка 5</td>
          <td>Строка 25, Колонка 6</td>
        </tr>

        <tr class="row-26 row-color-white">
          <td colspan="2" class="black-bg">Финальный итог</td>
          <td>Строка 26, Колонка 3</td>
          <td>Строка 26, Колонка 4</td>
          <td>Строка 26, Колонка 5</td>
          <td>Строка 26, Колонка 6</td>
        </tr>
        <tr class="row-26 row-color-white">
          <td colspan="2" class="red-bg">ТИТУЛ</td>
          <td>Строка 26, Колонка 3</td>
          <td>Строка 26, Колонка 4</td>
          <td>Строка 26, Колонка 5</td>
          <td>Строка 26, Колонка 6</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
export default {
  name: 'CustomTable',
  data() {
    return {
      totalRows: 26,
      totalCols: 6
    }
  }
}
</script>

<style scoped>
  .table-container {
    overflow-x: auto;
    margin: 5px;
  }

  .custom-table {
    width: 100%;
    border-collapse: collapse;
    font-family: Arial, sans-serif;
    font-size: 14px;
  }

  .custom-table th,
  .custom-table td {
    border: 1px solid #aaa;
    padding: 8px 12px;
    text-align: center;
    vertical-align: middle;
  }
  .custom-table th.header-empty {
    border: none;
  }

  /* Заголовочные ячейки */
  .header-empty {
    background-color: transparent;    
  }

  .header-group {
    background-color: #e0e0e0;
    font-weight: bold;
    font-size: 16px;
  }

  /* Цвета строк */
  .row-color-blue {
    background-color: #e6f7ff; /* светло-голубой */
  }

  .row-color-yellow {
    background-color: #fffde7; /* светло-желтый */
  }

  .row-color-green {
    background-color: #e8f5e9; /* светло-зеленый */
  }

  .row-color-purple {
    background-color: #f3e5f5; /* светло-фиолетовый */
  }

  .row-color-white {
    background-color: #fff; /* белый */
  }

  .col-color-black{
    background-color: #000;
    color: #fff;
  }

  /* Объединенная ячейка */
  .merged-cell {
    background-color: #e1f5fe;
    font-weight: bold;
    vertical-align: middle;
    width: 50px;
  }

  .hidden-cell {
    display: none;
  }

  /* Альтернативный вариант для визуализации границ */
  .custom-table tbody tr:hover {
    background-color: rgba(100, 100, 200, 0.9);
  }

  /* Номера строк */
  .custom-table td:first-child {
    font-weight: bold;
    font-size: 12px;
    text-align: left;
    background-color: #f9f9f9;
    width: 200px;
  }

  /* Ячейка для выделения строки итогов */
  .custom-table td:first-child.black-bg {
    background-color: black;
    color: white;
  }

  /* Ячейка для выделения звания */
  .custom-table td:first-child.red-bg {
    background-color: #e45628;      
  }
</style>
