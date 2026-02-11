<template>
  <div class="not-found-container">
    <div class="error-code">404</div>
    <h1>Страница не найдена</h1>
    <p>Похоже, этот путь ведет в пустоту...</p>
    <router-link to="/" class="home-btn">На главную</router-link>
  </div>
</template>

<style scoped>
  .not-found-container {
    text-align: center;
    padding: 50px;
    color: #fff;
  }

  .error-code {
    font-size: 8rem;
    font-weight: bold;
    opacity: 0.2;
  }

  .home-btn {
    display: inline-block;
    margin-top: 20px;
    padding: 12px 24px;
    background: var(--era-orange);
    color: white;
    text-decoration: none;
    border-radius: 8px;
  }
</style>
