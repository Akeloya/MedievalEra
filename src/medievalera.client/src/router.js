import { createRouter, createWebHistory } from 'vue-router'
import HomePage from '@/components/MenuGrid.vue'
import GameResults from '@/components/Score/ScoreTable.vue'
// импортируйте другие ваши компоненты

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomePage
  },
  {
    path: '/results',
    name: 'results',
    component: GameResults
  },
  // добавьте другие маршруты по мере необходимости
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
