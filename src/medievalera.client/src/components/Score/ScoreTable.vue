<template>
  <div class="table-container">
    <table class="custom-table">
      <!-- Заголовочные строки для колонок 3-6 -->
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
        <!-- Строки 1-14 (светло-голубые) -->
        <tr v-for="row in 14"
            :key="row"
            :class="`row-${row} row-color-blue`">
          <td>{{ row }}</td>
          <td v-if="row === 1" :rowspan="14" class="merged-cell">
            Здания,<br>окруженные стеной х2
          </td>
          <td v-if="row !== 1" class="hidden-cell"></td>
          <td>Строка {{ row }}, Колонка 3</td>
          <td>Строка {{ row }}, Колонка 4</td>
          <td>Строка {{ row }}, Колонка 5</td>
          <td>Строка {{ row }}, Колонка 6</td>
        </tr>

        <!-- Строки 15-20 (светло-желтые) -->
        <tr v-for="row in 6"
            :key="row + 14"
            :class="`row-${row + 14} row-color-yellow`">
          <td>{{ row + 14 }}</td>
          <td>Строка {{ row + 14 }}, Колонка 2</td>
          <td>Строка {{ row + 14 }}, Колонка 3</td>
          <td>Строка {{ row + 14 }}, Колонка 4</td>
          <td>Строка {{ row + 14 }}, Колонка 5</td>
          <td>Строка {{ row + 14 }}, Колонка 6</td>
        </tr>

        <!-- Строки 21-22 (светло-зеленые) -->
        <tr v-for="row in 2"
            :key="row + 20"
            :class="`row-${row + 20} row-color-green`">
          <td>{{ row + 20 }}</td>
          <td>Строка {{ row + 20 }}, Колонка 2</td>
          <td>Строка {{ row + 20 }}, Колонка 3</td>
          <td>Строка {{ row + 20 }}, Колонка 4</td>
          <td>Строка {{ row + 20 }}, Колонка 5</td>
          <td>Строка {{ row + 20 }}, Колонка 6</td>
        </tr>

        <!-- Строка 23 (светло-фиолетовая) -->
        <tr class="row-23 row-color-purple">
          <td>23</td>
          <td>Строка 23, Колонка 2</td>
          <td>Строка 23, Колонка 3</td>
          <td>Строка 23, Колонка 4</td>
          <td>Строка 23, Колонка 5</td>
          <td>Строка 23, Колонка 6</td>
        </tr>

        <!-- Строка 24 (без специального цвета) -->
        <tr class="row-24">
          <td>24</td>
          <td>Строка 24, Колонка 2</td>
          <td>Строка 24, Колонка 3</td>
          <td>Строка 24, Колонка 4</td>
          <td>Строка 24, Колонка 5</td>
          <td>Строка 24, Колонка 6</td>
        </tr>

        <!-- Строка 25 (светло-зеленая) -->
        <tr class="row-25 row-color-green">
          <td>25</td>
          <td>Строка 25, Колонка 2</td>
          <td>Строка 25, Колонка 3</td>
          <td>Строка 25, Колонка 4</td>
          <td>Строка 25, Колонка 5</td>
          <td>Строка 25, Колонка 6</td>
        </tr>

        <!-- Строка 26 (без специального цвета) -->
        <tr class="row-26">
          <td>26</td>
          <td>Строка 26, Колонка 2</td>
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
    margin: 20px;
  }

  .custom-table {
    width: 100%;
    border-collapse: collapse;
    font-family: Arial, sans-serif;
    font-size: 14px;
  }

    .custom-table th,
    .custom-table td {
      border: 1px solid #ddd;
      padding: 8px 12px;
      text-align: center;
      vertical-align: middle;
    }

  /* Заголовочные ячейки */
  .header-empty {
    background-color: #f5f5f5;
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

  /* Объединенная ячейка */
  .merged-cell {
    background-color: #e1f5fe;
    font-weight: bold;
    vertical-align: middle;
  }

  .hidden-cell {
    display: none;
  }

  /* Альтернативный вариант для визуализации границ */
  .custom-table tr:hover {
    background-color: rgba(0, 0, 0, 0.05);
  }

  /* Номера строк */
  .custom-table td:first-child {
    font-weight: bold;
    background-color: #f9f9f9;
    width: 50px;
  }
</style>
