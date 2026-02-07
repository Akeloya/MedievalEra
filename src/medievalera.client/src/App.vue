<template>
  <div class="medieval-era-app">
    <!-- Шапка с иконкой и текстом -->
    <header class="app-header">
      <div class="header-content">
        <!-- Иконка -->
        <div class="header-icon">
          <img src="./assets/logo.svg"
               alt="Иконка Эра средневековья"
               class="header-icon-img" />
        </div>

        <!-- Текст -->
        <div class="header-text">
          <h1 class="header-title">{{ title }}</h1>
        </div>
      </div>
    </header>

    <!-- Основной контент -->
    <main class="app-main">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
  </div>
</template>

<script>
  export default {
    name: 'AppHeader',
    props: {
      // Свойство для иконки (можно передавать путь или URL)
      iconSrc: {
        type: String,
        default: './assets/logo.svg'
      },
      // Свойство для текста
      title: {
        type: String,
        default: 'Эра средневековья'
      }
    }
  }
</script>

<style scoped>
  .medieval-era-app {
    font-family: 'Georgia', 'Times New Roman', serif;
    width: 95vw; /* 90% от ширины viewport */
    max-width: 1200px; /* Ограничение для больших экранов */
    min-height: 95vh; /* 90% от высоты viewport */
    margin: 0 auto; /* Центрирование по горизонтали */
    padding: 20px;
    display: flex;
    flex-direction: column;
    justify-content: flex-start; /* Контент вверху */
    align-items: center; /* Центрирование содержимого */
    box-sizing: border-box; /* Учитываем padding в ширине */
  }
  /* Стили для шапки */
  .app-header {
    background: linear-gradient(135deg, #2c3e50 0%, #4a6572 100%);
    border-radius: 10px;
    padding: 25px;
    margin-bottom: 30px;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
    border: 1px solid #5d6d7e;
    width: 100%; /* Занимает всю ширину родителя */
    box-sizing: border-box;
  }

  .header-content {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 30px;
    flex-wrap: wrap;
    width: 100%;
  }

  /* Стили для иконки */
  .header-icon {
    flex-shrink: 0;
  }

  .header-icon-img {
    width: 125px;
    height: 125px;
    object-fit: cover;
    box-shadow: 0 0 15px rgba(212, 175, 55, 0.3);
    transition: transform 0.3s ease;
  }

    .header-icon-img:hover {
      transform: scale(1.05);
    }

  /* Стили для текста */
  .header-text {
    text-align: left;
    flex: 1;
    min-width: 200px;
  }

  .header-title {
    font-size: 2.8rem;
    font-weight: bold;
    color: #f5f5f5;
    margin: 0;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.5);
    letter-spacing: 1px;
  }

  /* Стили для основного контента */
  .app-main {
    background-color: #f8f5f0;
    border-radius: 10px;
    padding: 30px;
    flex: 1; /* Занимает оставшееся пространство */
    width: 100%; /* Полная ширина */
    border: 1px solid #d4c8b5;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    box-sizing: border-box;
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
    align-items: center;
  }

  /* Адаптивность для мобильных устройств */
  @media (max-width: 768px) {
    .medieval-era-app {
      width: 90vw;
      padding: 15px;
      min-height: 90vh;
    }

    .app-header {
      padding: 20px;
      margin-bottom: 20px;
    }

    .header-content {
      flex-direction: column;
      text-align: center;
      gap: 20px;
    }

    .header-text {
      text-align: center;
      min-width: unset;
    }

    .header-title {
      font-size: 2.2rem;
    }

    .app-main {
      padding: 20px;
    }
  }

  @media (max-width: 480px) {
    .medieval-era-app {
      width: 90vw;
      padding: 10px;
    }

    .header-icon-img {
      width: 100px;
      height: 100px;
    }

    .header-title {
      font-size: 1.8rem;
    }

    .app-main {
      padding: 15px;
    }
  }

  /* Для очень маленьких экранов */
  @media (max-width: 360px) {
    .header-title {
      font-size: 1.5rem;
    }

    .header-icon-img {
      width: 85px;
      height: 85px;
    }
  }
</style>
