const colors = {
  wall: '#b5bbc4', tower: '#b5bbc4',
  monastery: '#fffdf2', cathedral: '#fffdf2', church: '#fffdf2',
  market: '#be8d55', university: '#be8d55', sawmill: '#be8d55',
  farm: '#f4d34d', house: '#f4d34d',
  cityHouse: '#629dd7', guild: '#629dd7',
  hospital: '#ee9946', scorchedEarth: '#766451',
}

export function buildingColor(type) { return colors[type] ?? '#be8d55' }

// Keep a gesture crossing -180°/180° continuous.
export function rotationDelta(start, current) { return ((current - start + 540) % 360) - 180 }
