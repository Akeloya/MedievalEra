// models/Dice.js
export class Dice {
  constructor(diceData) {
    this.id = diceData.id || `dice_${Date.now()}_${Math.random()}`;
    this.name = diceData.name || '';
    this.color = diceData.color || this.getDefaultColor(diceData.diceType);
    this.diceType = diceData.diceType;
    this.faces = diceData.faces || [];
    this.currentFace = null;
    this.isLocked = false;

    // Сохраняем оригинальные данные для API
    this._originalData = diceData;
  }

  // Бросок кубика
  roll() {
    if (this.isLocked) {
      return this.currentFace;
    }

    // Если есть faces, выбираем случайную грань
    if (this.faces && this.faces.length > 0) {
      const randomIndex = Math.floor(Math.random() * this.faces.length);
      this.currentFace = this.faces[randomIndex];
      return this.currentFace;
    }

    // Если нет граней, возвращаем заглушку
    console.warn('Dice has no faces:', this);
    return { values: {}, choose: false };
  }

  // Заблокировать кубик
  lock(face) {
    this.isLocked = true;
    if (face) {
      this.currentFace = face;
    }
  }

  // Разблокировать кубик
  unlock() {
    this.isLocked = false;
  }

  // Получить цвет по умолчанию для типа кубика
  getDefaultColor(type) {
    const colors = {
      1: '#8B4513', // Peasant - коричневый
      2: '#4A90E2', // Citizen - синий
      3: '#9B59B6', // Clergy - фиолетовый
      4: '#F1C40F', // Nobility - золотой
    };
    return colors[type] || '#95A5A6';
  }

  // Проверка на наличие ресурса на текущей грани
  hasResource(resource) {
    return this.currentFace?.values?.[resource] > 0;
  }

  // Клонирование кубика
  clone() {
    const clone = new Dice(this._originalData);
    clone.id = this.id;
    clone.currentFace = this.currentFace ? { ...this.currentFace } : null;
    clone.isLocked = this.isLocked;
    return clone;
  }
}
