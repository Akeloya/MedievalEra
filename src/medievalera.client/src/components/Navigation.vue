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

    <!-- Информация о текущем маршруте -->
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

<script>
  import { ref, computed, watch } from 'vue'
  import { useRoute, useRouter } from 'vue-router'

  export default {
    name: 'Navigation',

    setup() {
      const route = useRoute()
      const router = useRouter()

      // Храним историю посещений
      const visitHistory = ref([])

      // Флаг для показа навигации (скрываем на главной странице)
      const showNavigation = computed(() => {
        return route.path !== '/' && route.meta.showNavigation !== false
      })

      // Можем ли идти назад
      const canGoBack = computed(() => {
        return visitHistory.value.length > 1
      })

      // Мета-данные текущего маршрута
      const currentRouteMeta = computed(() => {
        return route.meta || {}
      })

      // Строим хлебные крошки
      const breadcrumbs = computed(() => {
        const crumbs = []

        // Начинаем с текущего маршрута
        let current = route

        while (current) {
          // Добавляем крошку, если у маршрута есть название
          if (current.meta?.breadcrumb || current.meta?.title) {
            crumbs.unshift({
              path: current.path,
              title: current.meta.breadcrumb || current.meta.title,
              name: current.name
            })
          }

          // Если у маршрута есть родительский маршрут в метаданных
          if (current.meta?.parent) {
            // Ищем родительский маршрут в истории или в роутах
            const parentInHistory = visitHistory.value.find(
              visit => visit.name === current.meta.parent
            )

            if (parentInHistory) {
              current = {
                path: parentInHistory.path,
                meta: {
                  breadcrumb: parentInHistory.meta?.breadcrumb || parentInHistory.name,
                  title: parentInHistory.meta?.title
                }
              }
            } else {
              // Если не нашли в истории, ищем в текущем маршруте
              const matched = route.matched.find(
                match => match.name === current.meta.parent
              )

              if (matched) {
                current = {
                  path: matched.path,
                  meta: matched.meta
                }
              } else {
                current = null
              }
            }
          } else if (current.matched.length > 1) {
            // Иначе берем родительский маршрут из matched
            const parentIndex = current.matched.length - 2
            if (parentIndex >= 0) {
              const parent = current.matched[parentIndex]
              current = {
                path: parent.path,
                meta: parent.meta
              }
            } else {
              current = null
            }
          } else {
            current = null
          }
        }

        // Всегда добавляем главную страницу в начало
        if (crumbs.length === 0 || crumbs[0].path !== '/') {
          crumbs.unshift({
            path: '/',
            title: 'Главное меню',
            name: 'home'
          })
        }

        return crumbs
      })

      // Функция для перехода назад
      const goBack = () => {
        if (canGoBack.value) {
          // Удаляем текущий маршрут из истории
          visitHistory.value.pop()

          // Получаем предыдущий маршрут
          const previousRoute = visitHistory.value[visitHistory.value.length - 1]

          // Переходим на предыдущий маршрут
          router.push({
            path: previousRoute.path,
            query: previousRoute.query,
            params: previousRoute.params
          })
        }
      }

      // Функция для добавления маршрута в историю
      const addToHistory = (toRoute) => {
        // Проверяем, не является ли этот маршрут уже последним в истории
        const lastInHistory = visitHistory.value[visitHistory.value.length - 1]

        if (!lastInHistory || lastInHistory.path !== toRoute.path) {
          // Добавляем только если это не редирект на ту же страницу
          visitHistory.value.push({
            path: toRoute.path,
            fullPath: toRoute.fullPath,
            name: toRoute.name,
            meta: toRoute.meta,
            query: { ...toRoute.query },
            params: { ...toRoute.params }
          })

          // Ограничиваем историю (например, последние 20 посещений)
          if (visitHistory.value.length > 20) {
            visitHistory.value = visitHistory.value.slice(-20)
          }
        }
      }

      // Следим за изменениями маршрута
      watch(
        () => ({
          path: route.path,
          fullPath: route.fullPath
        }),
        (newRoute, oldRoute) => {
          if (newRoute.path !== oldRoute?.path) {
            addToHistory(route)
          }
        },
        { immediate: true }
      )

      // Инициализируем историю текущим маршрутом
      addToHistory(route)

      return {
        showNavigation,
        canGoBack,
        currentRouteMeta,
        breadcrumbs,
        goBack
      }
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
    font-size: 20px;
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
