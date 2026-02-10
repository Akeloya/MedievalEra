import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'home',
    component: () => import('@/components/Views/HomeView.vue'),
    meta: {
      title: 'Главное меню',
      breadcrumb: 'Главное меню',
      icon: '🏰',
      description: 'Выберите раздел для продолжения'
    }
  },
  {
    path: '/game',
    name: 'game',
    component: () => import('@/components/Views/HomeView.vue'),
    meta: {
      title: 'Играть',
      breadcrumb: 'Игра',
      parent: 'home',
      icon: '⚔️',
      description: 'Старт новой игры'
    }
  },
  {
    path: '/lobby',
    name: 'lobby',
    component: () => import('@/components/Views/LobbyView.vue'),
    meta: {
      title: 'Новая игра здесь',
      breadcrumb: 'Онлайн игра',
      parent: 'game',
      icon: '👥',
      description: 'Начать новую онлайн игру'
    }
  },
  {
    path: '/results',
    name: 'results',
    component: () => import('@/components/Views/ScoresView.vue'),
    meta: {
      title: 'Результаты',
      breadcrumb: 'Результаты',
      parent: 'home',
      icon: '👥',
      description: 'Результаты прошедших игр'
    }
  },  
  {
    path: '/rules',
    name: 'rules',
    component: () => import('@/components/Views/RulesView.vue'),
    meta: {
      title: 'Правила',
      breadcrumb: 'Правила',
      parent: 'home',
      icon: '📜',
      description: 'Правила игры'
    }
  },
  {
    path: '/settings',
    name: 'settings',
    component: () => import('@/components/Views/SettingsView.vue'),
    meta: {
      title: 'Настройки',
      breadcrumb: 'Настройки',
      parent: 'webGame',
      icon: '📜',
      description: 'Настройки игры'
    }
  },
  {
    path: '/authors',
    name: 'authors',
    component: () => import('@/components/Views/AuthorsView.vue'),
    meta: {
      title: 'Авторы игры',
      breadcrumb: 'Над игрой работали',
      parent: 'home',
      icon: '👥',
      description: 'Разработчики игры'
    }
  },
  {
    path: '/tableGame',
    name: 'tableGame',
    component: () => import('@/components/Views/TableGameView.vue'),
    meta: {
      title: 'Настольная игра',
      breadcrumb: 'Настольная игра',
      parent: 'game',
      icon: '👥',
      description: 'Начать новую игру за столом'
    }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
