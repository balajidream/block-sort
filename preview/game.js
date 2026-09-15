const COLORS = {
  R: ["red", "▲"], A: ["amber", "●"], V: ["violet", "★"],
  T: ["teal", "☘"], L: ["lime", "◆"], O: ["orange", "♥"],
};

const LEVELS = [
  { board: [["R","A","R","A"],["A","R","A","R"],[]], reward: 30 },
  { board: [["R","A","A","R"],["A","R","R","A"],[]], reward: 35 },
  { board: [["R","A","R","A"],["A","R","A","R"],[]], reward: 40 },
  { board: [["R","V","R","V"],["V","R","V","R"],[]], reward: 45 },
  { board: [["A","T","A","T"],["T","A","T","A"],[]], reward: 50 },
  { board: [["R","A","V","R"],["A","V","R","A"],["V","R","A","V"],[]], reward: 55 },
  { board: [["T","L","T","L"],["L","T","L","T"],[]], reward: 60 },
  { board: [["R","O","R","O"],["O","R","O","R"],[]], reward: 65 },
  { board: [["V","T","A","V"],["T","A","V","T"],["A","V","T","A"],[]], reward: 70 },
  { board: [["L","R","L","R"],["R","L","R","L"],[]], reward: 75 },
  { board: [["O","A","V","O"],["A","V","O","A"],["V","O","A","V"],[]], reward: 80 },
  { board: [["R","T","L","R"],["T","L","R","T"],["L","R","T","L"],[]], reward: 90 },
];

const ui = Object.fromEntries([...document.querySelectorAll("[id]")].map(node => [node.id, node]));
const state = {
  level: Number(localStorage.getItem("block-sort-current") || 1),
  cleared: JSON.parse(localStorage.getItem("block-sort-cleared") || "[]"),
  coins: Number(localStorage.getItem("block-sort-coins") || 120),
  board: [], selected: null, history: [], undos: 3, extraUsed: false, moves: 0,
  justMoved: null, soundOn: true, clearing: new Set(),
};

const Sfx = {
  context: null,
  play(frequency, duration, type = "sine", volume = 0.045, sweep = 1) {
    if (!state.soundOn) return;
    try {
      this.context ||= new (window.AudioContext || window.webkitAudioContext)();
      const oscillator = this.context.createOscillator();
      const gain = this.context.createGain();
      oscillator.type = type;
      oscillator.frequency.setValueAtTime(frequency, this.context.currentTime);
      oscillator.frequency.exponentialRampToValueAtTime(Math.max(40, frequency * sweep), this.context.currentTime + duration);
      gain.gain.setValueAtTime(volume, this.context.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, this.context.currentTime + duration);
      oscillator.connect(gain).connect(this.context.destination);
      oscillator.start(); oscillator.stop(this.context.currentTime + duration);
    } catch (_) { /* Audio is optional on restricted browsers. */ }
  },
  pick() { this.play(520, .07, "sine", .035, 1.25); },
  drop() { this.play(180, .12, "triangle", .07, .68); },
  undo() { this.play(330, .11, "sine", .035, .55); },
  invalid() { this.play(115, .12, "square", .025, .75); },
  complete() { this.play(620, .25, "sine", .065, 1.7); setTimeout(() => this.play(940, .18, "sine", .045, 1.3), 90); },
};

function saveProgress() {
  localStorage.setItem("block-sort-current", state.level);
  localStorage.setItem("block-sort-cleared", JSON.stringify(state.cleared));
  localStorage.setItem("block-sort-coins", state.coins);
}
function screen(name) {
  ["splash","home","levels","game"].forEach(id => ui[id].classList.toggle("hidden", id !== name));
}
function refreshCoins() {
  ["homeCoins","levelsCoins","gameCoins"].forEach(id => ui[id].textContent = state.coins);
  ui.nextLevel.textContent = Math.min(state.level, LEVELS.length);
}
function openHome() { refreshCoins(); screen("home"); }
function openLevels() {
  refreshCoins();
  ui.levelGrid.innerHTML = "";
  LEVELS.forEach((_, index) => {
    const number = index + 1;
    const button = document.createElement("button");
    button.className = `level-node ${state.cleared.includes(number) ? "cleared" : number === state.level ? "current" : number > state.level ? "locked" : ""}`;
    button.innerHTML = number > state.level ? "🔒" : state.cleared.includes(number) ? "✓" : number;
    button.disabled = number > state.level;
    button.onclick = () => startLevel(number);
    ui.levelGrid.append(button);
  });
  screen("levels");
}
function startLevel(number) {
  state.level = number;
  state.board = LEVELS[number - 1].board.map(slot => [...slot]);
  state.selected = null; state.history = []; state.undos = 3; state.extraUsed = false; state.moves = 0; state.justMoved = null; state.clearing = new Set();
  document.querySelectorAll(".flying-block").forEach(node => node.remove());
  ui.levelNumber.textContent = number;
  ui.hint.textContent = "Tap a stack, then another.";
  ui.undoCount.textContent = state.undos;
  ui.extraButton.style.opacity = "1";
  refreshCoins(); renderBoard(); screen("game");
}
function topRun(slot) {
  if (!slot.length) return 0;
  const top = slot.at(-1);
  let run = 0;
  for (let i = slot.length - 1; i >= 0 && slot[i] === top; i--) run++;
  return run;
}
function canMove(from, to) {
  const source = state.board[from], dest = state.board[to];
  return from !== to && source.length && dest.length < 4 && (!dest.length || dest.at(-1) === source.at(-1));
}
function renderBoard() {
  ui.board.innerHTML = "";
  state.board.forEach((slot, index) => {
    const element = document.createElement("button");
    element.className = `slot ${state.selected === index ? "selected" : ""}`;
    element.setAttribute("aria-label", `Slot ${index + 1}, ${slot.length} blocks`);
    element.onclick = () => tapSlot(index, element);
    slot.forEach((color, blockIndex) => {
      const block = document.createElement("span");
      const isHeld = state.selected === index && blockIndex >= slot.length - topRun(slot);
      const justArrived = state.justMoved && state.justMoved.to === index && blockIndex >= slot.length - state.justMoved.count;
      block.className = `block ${COLORS[color][0]} ${isHeld ? "held" : ""} ${justArrived ? "arriving" : ""}`;
      block.textContent = COLORS[color][1];
      element.append(block);
    });
    ui.board.append(element);
  });
  ui.moves.textContent = `${state.moves} MOVE${state.moves === 1 ? "" : "S"}`;
}
function tapSlot(index, element) {
  if (state.selected === null) {
    if (!state.board[index].length) return invalid(element);
    state.selected = index;
    Sfx.pick();
    navigator.vibrate?.(8);
    renderBoard();
    return;
  }
  if (state.selected === index) {
    state.selected = null; renderBoard(); return;
  }
  const from = state.selected;
  if (!canMove(from, index)) {
    invalid(element);
    state.selected = null;
    renderBoard();
    return;
  }
  move(from, index);
}
function invalid(element) {
  if (!element) return;
  Sfx.invalid();
  navigator.vibrate?.(18);
  element.classList.remove("invalid");
  void element.offsetWidth;
  element.classList.add("invalid");
}
function move(from, to) {
  const slots = ui.board.querySelectorAll(".slot");
  const sourceElement = slots[from], destinationElement = slots[to];
  const source = state.board[from], dest = state.board[to];
  const count = Math.min(topRun(source), 4 - dest.length);
  const moved = source.slice(source.length - count);
  const flyFrom = [...sourceElement.querySelectorAll(".block")].slice(-count).map(block => block.getBoundingClientRect());
  const destRect = destinationElement.getBoundingClientRect();
  const destHeightBefore = dest.length;
  state.history.push(state.board.map(slot => [...slot]));
  source.splice(source.length - count, count);
  dest.push(...moved);
  state.selected = null;
  state.moves++;
  state.justMoved = { to, count };
  Sfx.drop();
  navigator.vibrate?.(10);
  flyGhosts(flyFrom, destRect, destHeightBefore, moved);
  renderBoard();
  requestAnimationFrame(() => ui.board.querySelectorAll(".slot")[to]?.classList.add("receiving"));
  setTimeout(() => { if (state.justMoved?.to === to) state.justMoved = null; }, 180);
  if (dest.length === 4 && dest.every(color => color === dest[0])) clearComplete(to);
}
function flyGhosts(starts, destRect, destHeightBefore, colors) {
  starts.forEach((rect, index) => {
    const flying = document.createElement("span");
    flying.className = `block ${COLORS[colors[index]][0]} flying-block`;
    flying.textContent = COLORS[colors[index]][1];
    flying.style.cssText = `left:${rect.left}px;top:${rect.top}px;width:${rect.width}px;height:${rect.height}px`;
    document.body.append(flying);
    const targetX = destRect.left + (destRect.width - rect.width) / 2 - rect.left;
    const targetY = destRect.bottom - 12 - rect.height * (destHeightBefore + index + 1) - rect.top;
    flying.animate([
      { transform: "translate(0,0) scale(1)", opacity: 1, offset: 0 },
      { transform: `translate(${targetX * .5}px, ${targetY * .3 - 36}px) scale(1.05)`, opacity: .9, offset: .45 },
      { transform: `translate(${targetX}px, ${targetY}px) scale(1)`, opacity: 0, offset: 1 },
    ], { duration: 170, delay: index * 18, easing: "cubic-bezier(.2,.85,.32,1)", fill: "forwards" }).onfinish = () => flying.remove();
  });
}
function clearComplete(index) {
  const slot = ui.board.querySelectorAll(".slot")[index];
  Sfx.complete();
  navigator.vibrate?.([8, 24, 12]);
  if (slot) {
    slot.classList.add("clearing");
    confetti(slot);
  }
  state.board[index] = [];
  renderBoard();
  if (state.board.every(stack => !stack.length)) completeLevel();
}
function confetti(slot) {
  const origin = slot.getBoundingClientRect(), area = ui.board.getBoundingClientRect();
  for (let i = 0; i < 16; i++) {
    const piece = document.createElement("i");
    piece.textContent = i % 2 ? "✦" : "•";
    piece.style.cssText = `position:absolute;z-index:8;left:${origin.left - area.left + origin.width / 2}px;top:${origin.top - area.top + 45}px;color:${["#ffda3d","#e96442","#a95ce5","#4ecfc4"][i % 4]};font-size:${10 + i % 9}px;pointer-events:none;transition:transform .65s ease-out,opacity .65s;`;
    ui.board.append(piece);
    requestAnimationFrame(() => { piece.style.transform = `translate(${(Math.random()-.5)*150}px,${-55-Math.random()*150}px) rotate(${Math.random()*500}deg)`; piece.style.opacity = "0"; });
    setTimeout(() => piece.remove(), 700);
  }
}
function undo() {
  if (!state.history.length || !state.undos) return;
  state.board = state.history.pop(); state.undos--; state.moves = Math.max(0, state.moves - 1); state.selected = null;
  Sfx.undo();
  ui.undoCount.textContent = state.undos;
  document.querySelectorAll(".flying-block").forEach(node => node.remove());
  renderBoard();
}
function extraSlot() {
  if (state.extraUsed) return;
  state.board.push([]); state.extraUsed = true; ui.extraButton.style.opacity = ".45";
  Sfx.play(410, .14, "sine", .05, 1.45);
  renderBoard();
}
function completeLevel() {
  const completedLevel = state.level;
  const reward = LEVELS[completedLevel - 1].reward;
  state.coins += reward;
  if (!state.cleared.includes(completedLevel)) state.cleared.push(completedLevel);
  if (completedLevel < LEVELS.length) state.level = Math.max(state.level, completedLevel + 1);
  saveProgress(); refreshCoins();
  ui.completeLevel.textContent = completedLevel;
  ui.rewardAmount.textContent = reward;
  ui.complete.classList.remove("hidden");
}

ui.playButton.onclick = () => startLevel(state.level);
ui.levelsButton.onclick = openLevels;
document.querySelectorAll(".back-button,.home-button").forEach(button => button.onclick = openHome);
ui.undoButton.onclick = undo; ui.extraButton.onclick = extraSlot;
ui.nextButton.onclick = () => { ui.complete.classList.add("hidden"); startLevel(state.level); };
ui.doubleButton.onclick = () => { state.coins += LEVELS[Math.max(0, state.level - 2)].reward; saveProgress(); refreshCoins(); ui.doubleButton.textContent = "REWARD CLAIMED"; };
ui.workshopButton.onclick = () => ui.workshop.classList.remove("hidden");
document.querySelectorAll("#workshop .close-modal").forEach(button => button.onclick = () => ui.workshop.classList.add("hidden"));
ui.soundButton.onclick = () => {
  state.soundOn = !state.soundOn;
  ui.soundButton.textContent = state.soundOn ? "♪" : "×";
  if (state.soundOn) Sfx.pick();
};
ui.splash.onclick = openHome;
setTimeout(openHome, 2200);
