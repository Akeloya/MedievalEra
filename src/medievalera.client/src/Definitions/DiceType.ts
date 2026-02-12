const DiceType = Object.freeze({
  None: 0,
  Peasant: 1,
  Citizen: 2,
  Clergy: 3,
  Nobility: 4,

  // Отображение строкового значения в число
  fromString(str) {
    const map = {
      'None': this.None,
      'Peasant': this.Peasant,
      'Citizen': this.Citizen,
      'Clergy': this.Clergy,
      'Nobility': this.Nobility
    };
    return map[str] ?? this.None;
  },

  // Отображение числа в строку
  toString(val) {
    const map = {
      [this.None]: "None",
      [this.Peasant]: "Peasant",
      [this.Citizen]: "Citizen",
      [this.Clergy]: "Clergy",
      [this.Nobility]: "Nobility"
    };
    return map[val] ?? "None";
  }
});


export default DiceType;