<script setup>
  import MenuGrid from './components/MenuGrid.vue'

  const handleMenuItemClick = (item) => {
    console.log('Кликнули по пункту меню:', item)
    // Здесь твоя логика: например, открыть модалку или перейти по ссылке
    alert(`Вы выбрали: ${item.title}`)
  }
</script>

<template>
  <header>
    <img alt="Medieval era" class="logo" src="./assets/logo.svg" width="125" height="125" />
  </header>
  <main>
    <MenuGrid :api-url="menu" @menu-item-click="handleMenuItemClick" />
  </main>
</template>

<style scoped>
header {
  line-height: 1.5;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>
