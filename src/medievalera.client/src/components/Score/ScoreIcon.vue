<template>
  <svg class="score-icon" viewBox="0 0 32 32" aria-hidden="true" focusable="false">
    <template v-if="name === 'book'">
      <path d="M3 7.5c5.2-1.8 9.5-.9 13 2.4v17c-3.5-3.2-7.8-4-13-2.2z" />
      <path d="M29 7.5c-5.2-1.8-9.5-.9-13 2.4v17c3.5-3.2 7.8-4 13-2.2z" />
      <path d="M6.5 12c2.2-.5 4.4-.3 6.5.6M6.5 16c2.2-.5 4.4-.3 6.5.6M25.5 12c-2.2-.5-4.4-.3-6.5.6m6.5 3.4c-2.2-.5-4.4-.3-6.5.6" />
    </template>
    <template v-else-if="name === 'die'">
      <rect x="3" y="3" width="26" height="26" rx="6" />
      <circle cx="10" cy="10" r="1.8" class="die-pip" />
      <circle cx="22" cy="10" r="1.8" class="die-pip" />
      <circle cx="16" cy="16" r="1.8" class="die-pip" />
      <circle cx="10" cy="22" r="1.8" class="die-pip" />
      <circle cx="22" cy="22" r="1.8" class="die-pip" />
    </template>
    <template v-else-if="name === 'chessboard'">
      <rect x="3" y="3" width="26" height="26" rx="2" />
      <path class="board-dark" d="M5 5h6v6H5zm12 0h6v6h-6zM11 11h6v6h-6zm12 0h6v6h-6zM5 17h6v6H5zm12 0h6v6h-6zM11 23h6v4h-6zm12 0h6v4h-6z" />
    </template>
    <template v-else-if="name === 'tower'">
      <path d="M7 29V11h4V5h10v6h4v18z" />
      <path class="icon-detail" d="M13 15h6v5h-6zm1 14v-6h4v6z" />
      <path d="M5 11h5v3H5zm17 0h5v3h-5z" />
    </template>
    <template v-else-if="name === 'house' || name === 'city-house'">
      <path :class="name === 'city-house' ? 'city-house' : ''" d="M3 14 16 4l13 10v15H3z" />
      <path class="icon-detail" d="M13 19h6v10h-6zm-6-3h4v4H7zm14 0h4v4h-4z" />
    </template>
    <template v-else-if="name === 'farm'">
      <path d="M4 13 16 5l12 8v16H4z" />
      <path class="icon-detail" d="M10 18h12v11H10zm3 3v4m3-4v4m3-4v4" />
      <path d="M7 12h18v3H7z" />
    </template>
    <template v-else-if="name === 'church'">
      <path d="M5 14 16 8l11 6v15H5zM13 8V4h6v4z" />
      <path class="icon-detail" d="M13 20h6v9h-6z" />
      <path d="M15 1h2v7h-2zM12 3h8v2h-8z" />
    </template>
    <template v-else-if="name === 'monastery'">
      <path d="M3 12 16 5l13 7v17H3z" />
      <path class="icon-detail" d="M6 16h4v5H6zm8 0h4v5h-4zm8 0h4v5h-4zM14 23h5v6h-5z" />
      <path d="M3 12h26v3H3z" />
    </template>
    <template v-else-if="name === 'cathedral'">
      <path d="M3 13 9 7l7 5 7-5 6 6v16H3z" />
      <path class="icon-detail" d="M13 20h6v9h-6zM6 18h4v5H6zm16 0h4v5h-4z" />
      <path d="M7 7V3h4v4zm14 0V3h4v4zM8 1h2v8H8zm14 0h2v8h-2z" />
    </template>
    <template v-else-if="name === 'guild'">
      <path d="M4 12 16 5l12 7v17H4z" />
      <path class="icon-detail" d="M7 17h4v5H7zm7 0h4v5h-4zm7 0h4v5h-4zM13 24h6v5h-6z" />
      <path d="M3 11h26v3H3z" />
    </template>
    <template v-else-if="name === 'market'">
      <path d="M5 13h22v16H5z" />
      <path class="icon-detail" d="M9 19h5v10H9zm9 0h5v5h-5z" />
      <path d="m4 12 3-8h18l3 8-3 4-4-3-4 3-4-3-4 3z" />
      <path class="icon-detail" d="M9 5 7 14l4 2 2-11zm7 0v11l4-2-1-9z" />
    </template>
    <template v-else-if="name === 'hospital'">
      <path d="M4 8h24v21H4z" />
      <path class="icon-detail" d="M14 11h4v5h5v4h-5v5h-4v-5H9v-4h5z" />
      <path d="M2 5h28v4H2z" />
    </template>
    <template v-else-if="name === 'sawmill'">
      <path d="M4 16 16 7l12 9v13H4z" />
      <path class="icon-detail" d="M8 20h16v3H8zm0 5h16v3H8z" />
      <path d="M13 7 16 2l3 5z" />
    </template>
    <template v-else-if="name === 'university'">
      <path d="m2 11 14-8 14 8v3H2zM5 15h4v11H5zm9 0h4v11h-4zm9 0h4v11h-4zM3 27h26v3H3z" />
      <path class="icon-detail" d="M11 17h2v9h-2zm8 0h2v9h-2z" />
    </template>
    <template v-else>
      <circle cx="16" cy="16" r="12.5" />
      <path d="m16 7 2.7 5.5 6.1.9-4.4 4.3 1 6.1-5.4-2.9-5.4 2.9 1-6.1-4.4-4.3 6.1-.9z" />
    </template>
  </svg>
</template>

<script setup>
defineProps({
  name: {
    type: String,
    default: 'point'
  }
})
</script>

<style scoped>
.score-icon {
  width: 20px;
  height: 20px;
  flex: 0 0 20px;
  overflow: visible;
  fill: #d4a83f;
  stroke: #553d1f;
  stroke-width: 1.5;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.score-icon .die-pip {
  fill: #553d1f;
  stroke: none;
}

.score-icon .board-dark {
  fill: #553d1f;
  stroke: none;
}

.score-icon .icon-detail {
  fill: #553d1f;
  stroke: #553d1f;
  stroke-width: 0.7;
}

.score-icon .city-house {
  fill: #4f91c5;
  stroke: #264e72;
}
</style>
