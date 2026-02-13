const DiceResources = Object.freeze({
  None : 0,
  Goods : 1,
  Stone : 2,
  Wood : 4,
  Meal : 8,
  Skull : 16,
  Culture : 32,
  Attack : 64,
  Defence : 128,
  Building : 256,
  NewRoll : 512,

  // Отображение строкового значения в число
  fromString(str) {
    const map = {
      'None': this.None,
      'Goods': this.Goods,
      'Stone': this.Stone,
      'Wood': this.Wood,
      'Meal': this.Meal,
      'Skull': this.Skull,
      'Culture': this.Culture,
      'Attack': this.Attack,
      'Defence': this.Defence,
      'Building': this.Building,
      'NewRoll': this.NewRoll
    };
    return map[str] ?? this.None;
  },

  // Отображение числа в строку
  toString(val) {
    const map = {
      [this.None]: "None",
      [this.Goods]: "Goods",
      [this.Stone]: "Stone",
      [this.Wood]: "Wood",
      [this.Meal]: "Meal",
      [this.Skull]: "Skull",
      [this.Culture]: "Culture",
      [this.Attack]: "Attack",
      [this.Defence]: "Defence",
      [this.Building]: "Building",
      [this.NewRoll]: "NewRoll"
    };
    return map[val] ?? "None";
  }
});

export default DiceResources;