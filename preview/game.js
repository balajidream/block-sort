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
  animating: false, justMoved: null, soundOn: true,
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
  state.selected = null; state.history = []; state.undos = 3; state.extraUsed = false; state.moves = 0; state.animating = false; state.justMoved = null;
  ui.levelNumber.textContent = number;
  ui.hint.textContent = "Tap a stack to pick it up";
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
  if (state.animating) return;
  if (state.selected === null) {
    if (!state.board[index].length) return invalid(element);
    state.selected = index;
    Sfx.pick();
    navigator.vibrate?.(8);
    ui.hint.textContent = `Holding ${topRun(state.board[index])} ${COLORS[state.board[index].at(-1)][0]} block${topRun(state.board[index]) > 1 ? "s" : ""}`;
    renderBoard();
    return;
  }
  if (state.selected === index) {
    state.selected = null; ui.hint.textContent = "Tap a stack to pick it up"; renderBoard(); return;
  }
  const from = state.selected;
  if (!canMove(from, index)) {
    invalid(element);
    state.selected = null;
    ui.hint.textContent = "That stack cannot take those blocks";
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
  state.history.push(state.board.map(slot => [...slot]));
  const source = state.board[from], dest = state.board[to];
  const count = Math.min(topRun(source), 4 - dest.length);
  const moved = source.slice(source.length - count);
  const slots = ui.board.querySelectorAll(".slot");
  const sourceElement = slots[from], destinationElement = slots[to];
  state.selected = null; state.animating = true;
  ui.hint.textContent = "Flowing blocks...";
  sourceElement.classList.add("moving-source");
  destinationElement.classList.add("receiving");
  flyBlocks(sourceElement, destinationElement, moved, () => {
    source.splice(source.length - count, count);
    dest.push(...moved);
    state.moves++;
    state.justMoved = { to, count };
    state.animating = false;
    Sfx.drop();
    navigator.vibrate?.(14);
    renderBoard();
    ui.hint.textContent = "Nice move — keep sorting!";
    setTimeout(() => { state.justMoved = null; }, 520);
    if (dest.length === 4 && dest.every(color => color === dest[0])) setTimeout(() => clearComplete(to), 260);
  });
}
function flyBlocks(source, destination, colors, onFinish) {
  const sourceBlocks = [...source.querySelectorAll(".block")].slice(-colors.length);
  const start = source.getBoundingClientRect(), end = destination.getBoundingClientRect();
  let completed = 0;
  sourceBlocks.forEach((block, index) => {
    const rect = block.getBoundingClientRect();
    const flying = document.createElement("span");
    flying.className = `${block.className.replace("held", "")} flying-block`;
    flying.textContent = block.textContent;
    flying.style.cssText = `left:${rect.left}px;top:${rect.top}px;width:${rect.width}px;height:${rect.height}px`;
    flying.style.backgroundImage = getComputedStyle(block).backgroundImage;
    flying.style.backgroundSize = "136%";
    flying.style.backgroundPosition = "center";
    flying.style.color = "transparent";
    document.body.append(flying);
    const targetX = end.left + (end.width - rect.width) / 2 - rect.left;
    const targetY = end.bottom - 12 - rect.height * (destination.querySelectorAll(".block").length + index + 1) - rect.top;
    const animation = flying.animate([
      { transform: "translate(0, 0) scale(1)", offset: 0 },
      { transform: `translate(${targetX * .54}px, ${targetY * .22 - 70}px) scale(1.11) rotate(${index % 2 ? -4 : 4}deg)`, offset: .48 },
      { transform: `translate(${targetX}px, ${targetY}px) scale(1) rotate(0deg)`, offset: 1 },
    ], { duration: 520, delay: index * 58, easing: "cubic-bezier(.23, .9, .36, 1)", fill: "forwards" });
    animation.onfinish = () => {
      flying.remove();
      completed++;
      if (completed === colors.length) onFinish();
    };
  });
}
function clearComplete(index) {
  const slot = ui.board.querySelectorAll(".slot")[index];
  slot.classList.add("clearing");
  ui.hint.textContent = "Perfect stack! ✦";
  Sfx.complete();
  navigator.vibrate?.([12, 40, 20]);
  confetti(slot);
  setTimeout(() => {
    state.board[index] = [];
    renderBoard();
    if (state.board.every(stack => !stack.length)) completeLevel();
  }, 390);
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
  ui.undoCount.textContent = state.undos; ui.hint.textContent = "Move rewound";
  renderBoard();
}
function extraSlot() {
  if (state.extraUsed) { ui.hint.textContent = "Extra slot already used"; return; }
  state.board.push([]); state.extraUsed = true; ui.extraButton.style.opacity = ".45";
  Sfx.play(410, .14, "sine", .05, 1.45);
  ui.hint.textContent = "An empty slot appeared!";
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
