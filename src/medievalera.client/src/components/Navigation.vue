<template>
  <nav class="navigation-panel">
    <!-- Кнопка назад -->
    <button class="nav-btn back-btn"
            @click="goBack"
            :disabled="!canGoBack"
            title="Назад"
            v-if="showNavigation">
      <span class="btn-icon">←</span>
      <span class="btn-text">Назад</span>
    </button>

    <!-- Хлебные крошки -->
    <div class="breadcrumbs">
      <div v-for="(crumb, index) in breadcrumbs"
           :key="crumb.path"
           class="breadcrumb-item">
        <!-- Маршрут-ссылка -->
        <router-link v-if="index < breadcrumbs.length - 1"
                     :to="crumb.path"
                     class="breadcrumb-link">
          {{ crumb.title }}
        </router-link>

        <!-- Текущая страница (не ссылка) -->
        <span v-else class="breadcrumb-current">
          {{ crumb.title }}
        </span>

        <!-- Разделитель -->
        <span v-if="index < breadcrumbs.length - 1"
              class="breadcrumb-separator">
          ›
        </span>
      </div>
    </div>

     <!--Информация о текущем маршруте--> 
    <div class="route-info" v-if="currentRouteMeta">
      <span class="route-icon" v-if="currentRouteMeta.icon">
        {{ currentRouteMeta.icon }}
      </span>
      <span class="route-description">
        {{ currentRouteMeta.description || '' }}
      </span>
    </div>
  </nav>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const showNavigation = computed(() => route.path !== '/')
const canGoBack = computed(()=> route.path !== '/')

const currentRouteMeta = computed(() => {
  // Находим все совпадения в иерархии
  return route.matched.length > 0 ? route.matched.at(-1).meta : null
});

const breadcrumbs = computed(() => {
  // Находим все совпадения в иерархии
  return route.matched.map(m => ({
    path: m.path || '/',
    title: m.meta.breadcrumb || m.meta.title
  })).filter(c => c.title)
})

const goBack = () => {
  // Если есть куда идти по истории браузера - идем, иначе на уровень вверх
  if (window.history.state?.back) {
    router.back()
  } else {
    const parentPath = route.path.split('/').slice(0, -1).join('/') || '/'
    router.push(parentPath)
  }
}
</script>

<style scoped>
  .navigation-panel {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 20px;
    border: var(--era-border);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
    min-height: 60px;
    width: 100%;
    margin-bottom: 10px;
    border-radius: 10px;    
  }

  /* Кнопка назад */
  .back-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 20px;
    background: var(--sky-grey);
    border: 2px solid #D2B48C;
    border-radius: 6px;
    color: #FFF8DC;
    font-weight: bold;
    cursor: pointer;
    transition: all 0.3s ease;
    min-width: 120px;
    justify-content: center;
    font-family: var(--font-main);
  }

    .back-btn:hover:not(:disabled) {
      background: rgba(210, 180, 140, 0.6);
      transform: translateX(-2px);
      box-shadow: -2px 0 8px rgba(0, 0, 0, 0.4);
    }

    .back-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
      border-color: #8B4513;
    }

  .btn-icon {
    font-size: 10px;
    font-weight: bold;
  }

  .btn-text {
    font-size: 14px;
  }

  /* Хлебные крошки */
  .breadcrumbs {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
    padding: 0 20px;
    max-width: 50%;
    justify-content: center;
  }

  .breadcrumb-item {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .breadcrumb-link {
    text-decoration: none;
    color: #F5DEB3;
    font-weight: bold;
    padding: 5px 12px;
    border-radius: 4px;
    transition: all 0.2s ease;
    white-space: nowrap;
    background: var(--sky-grey);
  }

    .breadcrumb-link:hover {
      background: var(--sky-grey);      
      color: #FFF8DC;
      text-shadow: 0 0 8px rgba(255, 255, 220, 0.5);
    }

  .breadcrumb-current {
    color: #FFF8DC;
    font-weight: bold;
    padding: 5px 12px;
    background: var(--sky-grey);
    border-radius: 4px;
    border: 1px solid var(--era-yellow);
    white-space: nowrap;
  }

  .breadcrumb-separator {
    color: #D2B48C;
    font-size: 18px;
    font-weight: bold;
    margin-left: 4px;
  }

  /* Информация о маршруте */
  .route-info {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 8px 16px;
    background: rgba(101, 67, 33, 0.5);
    border-radius: 6px;
    border: 1px solid #8B4513;
    min-width: 120px;
    justify-content: center;
  }

  .route-icon {
    font-size: 20px;
  }

  .route-description {
    color: #F5DEB3;
    font-size: 14px;
    font-style: italic;
    text-align: center;
  }

  /* Адаптивность */
  @media (max-width: 1024px) {
    .navigation-panel {
      flex-wrap: wrap;
      gap: 10px;
      justify-content: center;
    }

    .breadcrumbs {
      max-width: 100%;
      order: 3;
      margin-top: 10px;
    }

    .back-btn {
      order: 1;
    }

    .route-info {
      order: 2;
    }
  }

  @media (max-width: 768px) {
    .navigation-panel {
      padding: 10px;
    }

    .breadcrumb-link,
    .breadcrumb-current {
      padding: 4px 8px;
      font-size: 12px;
    }

    .back-btn {
      min-width: 100px;
      padding: 8px 12px;
    }

    .route-info {
      min-width: 80px;
      padding: 6px 10px;
    }

    .route-description {
      display: none; /* Скрываем описание на мобильных */
    }
  }
</style>
