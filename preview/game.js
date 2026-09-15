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
  state.selected = null; state.history = []; state.undos = 3; state.extraUsed = false; state.moves = 0;
  ui.levelNumber.textContent = number;
  ui.hint.textContent = "Tap a stack to pick it up";
  ui.undoCount.textContent = state.undos;
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
      block.className = `block ${COLORS[color][0]} ${state.selected === index && blockIndex >= slot.length - topRun(slot) ? "held" : ""}`;
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
  element.classList.remove("invalid");
  void element.offsetWidth;
  element.classList.add("invalid");
}
function move(from, to) {
  state.history.push(state.board.map(slot => [...slot]));
  const source = state.board[from], dest = state.board[to];
  const count = Math.min(topRun(source), 4 - dest.length);
  const moved = source.splice(source.length - count, count);
  state.board[to].push(...moved);
  state.selected = null; state.moves++;
  animateTrail(from, to, COLORS[moved[0]][0]);
  renderBoard();
  ui.hint.textContent = "Nice move — keep sorting!";
  if (state.board[to].length === 4 && state.board[to].every(color => color === state.board[to][0])) {
    setTimeout(() => clearComplete(to), 220);
  }
}
function animateTrail(from, to, color) {
  requestAnimationFrame(() => {
    const slots = ui.board.querySelectorAll(".slot");
    const a = slots[from].getBoundingClientRect(), b = slots[to].getBoundingClientRect(), area = ui.board.getBoundingClientRect();
    const dx = b.left + b.width / 2 - (a.left + a.width / 2), dy = b.top + b.height / 2 - (a.top + 30);
    const trail = document.createElement("i");
    trail.className = "trail";
    trail.style.left = `${a.left + a.width / 2 - area.left}px`;
    trail.style.top = `${a.top + 25 - area.top}px`;
    trail.style.height = `${Math.hypot(dx, dy)}px`;
    trail.style.transform = `rotate(${Math.atan2(dx, -dy)}rad)`;
    trail.style.background = color === "lime" ? "linear-gradient(#fffcc1,#9bec4b,transparent)" : "";
    ui.board.append(trail);
    setTimeout(() => trail.remove(), 430);
  });
}
function clearComplete(index) {
  const slot = ui.board.querySelectorAll(".slot")[index];
  slot.classList.add("clearing");
  ui.hint.textContent = "Perfect stack! ✦";
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
  ui.undoCount.textContent = state.undos; ui.hint.textContent = "Move rewound";
  renderBoard();
}
function extraSlot() {
  if (state.extraUsed) { ui.hint.textContent = "Extra slot already used"; return; }
  state.board.push([]); state.extraUsed = true; ui.extraButton.style.opacity = ".45";
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
ui.soundButton.onclick = () => { ui.soundButton.textContent = ui.soundButton.textContent === "♪" ? "×" : "♪"; };
ui.splash.onclick = openHome;
setTimeout(openHome, 2200);
