<template>
  <div class="menu-container">
    <div v-if="loading" class="loading">
      Загрузка меню...
    </div>

    <div v-else-if="error" class="error">
      {{ error }}
    </div>

    <div v-else>
      <div class="menu-grid">
        <button v-for="item in menuItems"
                :key="item.id"
                class="menu-item"
                @click="handleMenuClick(item)">
          <div class="menu-content">
            <span class="menu-icon">{{ item.icon }}</span>
            <h3 class="menu-title">{{ item.title }}</h3>
            <p class="menu-description">{{ item.description }}</p>
          </div>
        </button>
      </div>

      <div v-if="currentMessage" class="message-container">
        <div class="message">
          {{ currentMessage }}
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'MenuGrid',
  props: {
    apiUrl: {
      type: String,
      default: 'menu/items'
    }
  },
  data() {
    return {
      menuItems: [],
      loading: true,
      error: null,
      currentMessage: ''
    }
  },
  async mounted() {
    await this.fetchData();
    },
    async created() {
      // fetch the data when the view is created and the data is
      // already being observed
      await this.fetchData();
    },
    watch: {
      // call again the method if the route changes
      '$route': 'fetchData'
    },
  methods: {
    async fetchData() {
      try {
        this.loading = true;
        this.error = null;

        var response = await fetch('api/menu/items', {
          headers: { 'Accept': 'application/json' }
        });
        if (response.ok) {
          this.menuItems = await response.json();
          this.loading = false;
        }
        else {
          this.error = 'Произошла ошибка запроса данных'
        }
        
        
      } catch (err) {
        this.error = err.message;
        console.error('Ошибка:', err);
      } finally {
        this.loading = false;
      }
    },

    handleMenuClick(item) {
      const messages = {
        'profile': '📊 Открыт раздел профиля. Здесь вы можете управлять своими данными.',
        'settings': '⚙️ Открыты настройки системы. Настройте параметры под себя.',
        'reports': '📈 Загружаются отчеты... Пожалуйста, подождите.',
        'help': '❔ Раздел помощи. Выберите интересующий вас вопрос.',
        'notifications': '🔔 У вас 3 новых уведомления. Проверьте их!',
        'logout': '🚪 Вы уверены, что хотите выйти из системы?'
      };

      this.currentMessage = messages[item.actionType] || `Действие: ${item.title}`;

      // Эмитим событие для родительского компонента
      this.$emit('menu-item-click', item);

      console.log(`Выполняется действие: ${item.actionType}`, item);
    }
  }
}
</script>

<style scoped>
  * {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
  }

  .menu-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
  }

  .menu-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: 25px;
    justify-content: center;
  }

  @media (min-width: 768px) {
    .menu-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }

  @media (min-width: 1200px) {
    .menu-grid {
      grid-template-columns: repeat(3, 1fr);
    }
  }

  .menu-item {
    background: linear-gradient(145deg, #6a11cb 0%, #2575fc 100%);
    border-radius: 20px;
    padding: 30px 25px;
    text-align: center;
    cursor: pointer;
    transition: all 0.3s ease;
    border: none;
    color: white;
    box-shadow: 8px 8px 16px rgba(0, 0, 0, 0.2), -4px -4px 10px rgba(255, 255, 255, 0.1);
    position: relative;
    overflow: hidden;
  }

    .menu-item::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: linear-gradient(145deg, #2575fc 0%, #6a11cb 100%);
      opacity: 0;
      transition: opacity 0.3s ease;
      border-radius: 20px;
    }

    .menu-item:hover {
      transform: translateY(-5px);
      box-shadow: 12px 12px 24px rgba(0, 0, 0, 0.3), -6px -6px 12px rgba(255, 255, 255, 0.15);
    }

    .menu-item:active {
      transform: translateY(2px);
      box-shadow: 4px 4px 8px rgba(0, 0, 0, 0.2), -2px -2px 6px rgba(255, 255, 255, 0.1);
    }

    .menu-item:hover::before {
      opacity: 1;
    }

  .menu-content {
    position: relative;
    z-index: 2;
  }

  .menu-icon {
    font-size: 3rem;
    margin-bottom: 15px;
    display: block;
  }

  .menu-title {
    font-size: 1.4rem;
    font-weight: 600;
    margin-bottom: 10px;
  }

  .menu-description {
    font-size: 0.9rem;
    opacity: 0.9;
    line-height: 1.4;
  }

  .message-container {
    margin-top: 30px;
    padding: 20px;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 15px;
    backdrop-filter: blur(10px);
  }

  .message {
    color: white;
    font-size: 1.1rem;
    text-align: center;
  }

  .loading {
    text-align: center;
    color: white;
    font-size: 1.2rem;
    padding: 40px;
  }

  .error {
    color: #ff6b6b;
    text-align: center;
    padding: 20px;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 10px;
    margin: 20px 0;
  }
</style>
