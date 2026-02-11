<template>
  <div class="credits-container">
    <div class="credits-header">
      <h1>НАД ОРИГИНАЛЬНОЙ ИГРОЙ РАБОТАЛИ</h1>
    </div>

    <table class="credits-table">
      <tbody>
        <tr v-for="(member, index) in team" :key="index">
          <td class="role">{{ member.role }}</td>
          <td class="name">{{ member.name }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  const team = ref([
    { role: 'Автор игры', name: 'Мэтт Ликок' },
    { role: 'Продюссер', name: 'Софи гравель' },
    { role: 'Разработка', name: 'Питер Эггерт, Катя Волк, Андре Биерт, Мартин Бушар, Мориц Тиль' },
    { role: 'Художественный директор', name: 'Софи Гравель' },
    { role: 'Иллюстрации', name: 'Крис Квильямс' },
    { role: 'Графический дизайн', name: 'Даник Рено, Стефан Вашон, Марис Эберт-Лемир' },
    { role: 'Редактура', name: 'Андре Биерт' },
    { role: '3D моделирование', name: 'Уолфред Кандиалес' },
    { role: 'Разработано', name: '(c) 2020 Plan B Games Europe GmbH Sinstorfer Weg 70, 21077 Hamburn, Germany' },    
  ]);
</script>

<style scoped>
  .credits-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    font-family: var(--font-main);
    color: #ffffff;
    background-color: #1a1a1a;
    padding: 40px;    
  }

  .credits-header {
    border: 2px solid #ffffff;
    padding: 10px 40px;
    margin-bottom: 40px;
    text-transform: uppercase;
    letter-spacing: 2px;
  }

    .credits-header h1 {
      margin: 0;
      font-size: 1.5rem;
    }

  .credits-table {
    border-collapse: collapse;
    width: 100%;
    max-width: 600px;
    border: none;
  }

    .credits-table td {
      padding: 12px 20px;
      font-size: 1.1rem;
      border: none;
    }

  .role {
    font-weight: bold;
    text-align: right;
    width: 50%;
    text-transform: uppercase;
    color: #cccccc;
  }

  .name {
    text-align: left;
    width: 50%;
  }

  @media (max-width: 600px) {
    .credits-table td {
      font-size: 0.9rem;
      padding: 8px 10px;
    }
  }
</style>
