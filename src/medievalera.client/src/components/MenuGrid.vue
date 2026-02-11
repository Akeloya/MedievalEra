<template>
  <div class="menu-container">    
    <div class="menu-grid">
      <button v-for="item in menuItems"
              :key="item.id"
              class="menu-item"
              @click="handleMenuClick(item)">
        <div class="menu-content">
          <span class="menu-icon">{{ item.icon }}</span>
          <h3 class="menu-title">{{ item.title }}</h3>
        </div>
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

const menuItems = computed(() => {
  // 1. Находим текущий активный уровень в matched
  // Нам нужен последний элемент, у которого есть children
  const currentRecord = route.matched[route.matched.length - 1]

  if (!currentRecord || !currentRecord.children) return []

  // 2. Формируем пункты меню из детей текущего маршрута
  return currentRecord.children
    .filter(child => child.meta && child.name) // Берем только именованные роуты с метой
    .map(child => ({
      id: child.name,
      title: child.meta.title || child.name,
      icon: child.meta.icon || '📍',
      description: child.meta.description || '',
      actionType: child.name
    }))
})

const handleMenuClick = (item) => {
  router.push({ name: item.actionType })
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
      grid-template-columns: repeat(1, 1fr);
    }
  }

  @media (min-width: 1200px) {
    .menu-grid {
      grid-template-columns: repeat(1, 1fr);
    }
  }

  .menu-item {
    background: linear-gradient( to bottom, color-mix(in srgb, var(--era-yellow), transparent 30%), color-mix(in srgb, var(--era-orange), transparent 30%) );
    border-radius: 20px;
    padding: 10px 105px;
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
      background: linear-gradient(145deg, var(--era-yellow) 0%, var(--sky-grey) 100%);
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
    display:flex;
    align-items: center;
  }

  .menu-icon {
    font-size: 3rem;
    margin-bottom: 15px;
    display: block;
  }

  .menu-title {
    font-size: 1.4rem;
    font-weight: 600;       
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
