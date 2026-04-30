const canvas = document.getElementById("game");
const ctx = canvas.getContext("2d");

const ui = {
  levelName: document.getElementById("levelName"),
  mapViews: document.getElementById("mapViews"),
  stateText: document.getElementById("stateText"),
  message: document.getElementById("message"),
  startButton: document.getElementById("startButton"),
  mapButton: document.getElementById("mapButton"),
  hideButton: document.getElementById("hideButton"),
  restartButton: document.getElementById("restartButton"),
};

const TILE = 64;
const FOV = Math.PI / 3;
const RAYS = 240;
const PLAYER_SPEED = 150;
const OGRE_SPEED = PLAYER_SPEED * 0.5;
const PLAYER_VISIBILITY = TILE * 7.5;
const OGRE_VISIBILITY = PLAYER_VISIBILITY * 0.75;
const COLLISION_RADIUS = 14;
const HIDE_RADIUS = TILE * 1.05;

const keys = new Set();
const touch = {
  forward: false,
  back: false,
  left: false,
  right: false,
};
const lookDrag = {
  active: false,
  lastX: 0,
};

const levels = [
  {
    name: "1",
    map: [
      "###############",
      "#P....#.......#",
      "#.###.#.#####.#",
      "#...#...#...#.#",
      "###.#####.#.#.#",
      "#...#B....#...#",
      "#.###.#######.#",
      "#.....#.....#.#",
      "#.#####.###.#.#",
      "#.....#.#B#...#",
      "#.###.#.#.###.#",
      "#...#...#...#E#",
      "#.#.#####.#.###",
      "#O#.......#...#",
      "###############",
    ],
  },
  {
    name: "2",
    map: [
      "#################",
      "#P......#.......#",
      "#.#####.#.#####.#",
      "#.#...#.#.....#.#",
      "#.#.#.#.#####.#.#",
      "#...#.#.....#...#",
      "#####.#####.###.#",
      "#B....#...#...#.#",
      "#.#####.#.###.#.#",
      "#.....#.#.....#.#",
      "###.#.#.#######.#",
      "#...#.#.....B...#",
      "#.###.#####.###.#",
      "#.....#O....#..E#",
      "#################",
    ],
  },
  {
    name: "3",
    map: [
      "###################",
      "#P..#.............#",
      "###.#.###########.#",
      "#...#.....#.....#.#",
      "#.#######.#.###.#.#",
      "#.#.....#.#.#B#...#",
      "#.#.###.#.#.#.#####",
      "#...#...#...#.....#",
      "#.###.###########.#",
      "#...#.....#.......#",
      "###.#####.#.#####.#",
      "#B..#.....#.....#.#",
      "#.###.#########.#.#",
      "#.....O.........#E#",
      "###################",
    ],
  },
  {
    name: "4",
    map: [
      "#####################",
      "#P....#.............#",
      "#.###.#.###########.#",
      "#...#.#.....#.....#.#",
      "###.#.#####.#.###.#.#",
      "#...#.....#.#.#...#.#",
      "#.#######.#.#.#.###.#",
      "#.#B....#...#.#...#.#",
      "#.#.###.#####.###.#.#",
      "#...#...#.....#...#.#",
      "#####.###.#####.###.#",
      "#.....#...#.....#...#",
      "#.#####.###.#######.#",
      "#.#.....#...#B......#",
      "#.#.#####.#########.#",
      "#...#.....O.......#E#",
      "#####################",
    ],
  },
  {
    name: "5",
    map: [
      "#######################",
      "#P........#...........#",
      "#.#######.#.#########.#",
      "#.#.....#.#.#.......#.#",
      "#.#.###.#.#.#.#####.#.#",
      "#...#...#...#.#...#...#",
      "#####.#######.#.#.#####",
      "#.....#B......#.#.....#",
      "#.#####.#######.#####.#",
      "#.#.....#.....#.....#.#",
      "#.#.#####.###.#####.#.#",
      "#...#.....#O#.....#...#",
      "###.#.#####.#####.###.#",
      "#...#.....#.....#...#.#",
      "#.#######.#####.###.#.#",
      "#B..............#...#E#",
      "#######################",
    ],
  },
];

let game = createGame(0);
let lastTime = performance.now();
let running = false;
let mapOverlayUntil = 0;

function createGame(levelIndex) {
  const source = levels[levelIndex % levels.length];
  const grid = source.map.map((row) => row.split(""));
  const benches = [];
  let player = null;
  let ogre = null;
  let exit = null;

  for (let y = 0; y < grid.length; y += 1) {
    for (let x = 0; x < grid[y].length; x += 1) {
      const cell = grid[y][x];
      if (cell === "P") {
        player = { x: (x + 0.5) * TILE, y: (y + 0.5) * TILE, angle: 0 };
        grid[y][x] = ".";
      }
      if (cell === "O") {
        ogre = { x: (x + 0.5) * TILE, y: (y + 0.5) * TILE, lastSeenX: null, lastSeenY: null, searchX: null, searchY: null };
        grid[y][x] = ".";
      }
      if (cell === "B") {
        benches.push({ x: (x + 0.5) * TILE, y: (y + 0.5) * TILE });
        grid[y][x] = ".";
      }
      if (cell === "E") {
        exit = { x: (x + 0.5) * TILE, y: (y + 0.5) * TILE };
        grid[y][x] = ".";
      }
    }
  }

  return {
    levelIndex,
    name: source.name,
    grid,
    width: grid[0].length,
    height: grid.length,
    player,
    ogre,
    exit,
    benches,
    mapViews: 2,
    hidden: false,
    ogreSawHide: false,
    won: false,
    lost: false,
  };
}

function resizeCanvas() {
  const ratio = Math.max(1, Math.min(2, window.devicePixelRatio || 1));
  canvas.width = Math.floor(canvas.clientWidth * ratio);
  canvas.height = Math.floor(canvas.clientHeight * ratio);
  ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
}

function cellAt(x, y) {
  const gx = Math.floor(x / TILE);
  const gy = Math.floor(y / TILE);
  if (gy < 0 || gy >= game.height || gx < 0 || gx >= game.width) return "#";
  return game.grid[gy][gx];
}

function isWall(x, y) {
  return cellAt(x, y) === "#";
}

function canStand(x, y) {
  return !(
    isWall(x - COLLISION_RADIUS, y - COLLISION_RADIUS) ||
    isWall(x + COLLISION_RADIUS, y - COLLISION_RADIUS) ||
    isWall(x - COLLISION_RADIUS, y + COLLISION_RADIUS) ||
    isWall(x + COLLISION_RADIUS, y + COLLISION_RADIUS)
  );
}

function moveEntity(entity, dx, dy) {
  const nextX = entity.x + dx;
  const nextY = entity.y + dy;
  if (canStand(nextX, entity.y)) entity.x = nextX;
  if (canStand(entity.x, nextY)) entity.y = nextY;
}

function distance(a, b) {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function hasLineOfSight(a, b, range) {
  const dx = b.x - a.x;
  const dy = b.y - a.y;
  const total = Math.hypot(dx, dy);
  if (total > range) return false;
  const steps = Math.max(1, Math.ceil(total / 12));
  for (let i = 1; i < steps; i += 1) {
    const x = a.x + (dx * i) / steps;
    const y = a.y + (dy * i) / steps;
    if (isWall(x, y)) return false;
  }
  return true;
}

function gridPoint(entity) {
  return {
    x: Math.floor(entity.x / TILE),
    y: Math.floor(entity.y / TILE),
  };
}

function isOpenCell(x, y) {
  return y >= 0 && y < game.height && x >= 0 && x < game.width && game.grid[y][x] !== "#";
}

function findNextStep(startEntity, target) {
  const start = gridPoint(startEntity);
  const goal = gridPoint(target);
  if (start.x === goal.x && start.y === goal.y) return target;

  const queue = [start];
  const cameFrom = new Map();
  const key = (x, y) => `${x},${y}`;
  cameFrom.set(key(start.x, start.y), null);

  for (let i = 0; i < queue.length; i += 1) {
    const current = queue[i];
    if (current.x === goal.x && current.y === goal.y) break;

    const neighbors = [
      { x: current.x + 1, y: current.y },
      { x: current.x - 1, y: current.y },
      { x: current.x, y: current.y + 1 },
      { x: current.x, y: current.y - 1 },
    ];

    for (const next of neighbors) {
      const nextKey = key(next.x, next.y);
      if (!isOpenCell(next.x, next.y) || cameFrom.has(nextKey)) continue;
      cameFrom.set(nextKey, current);
      queue.push(next);
    }
  }

  const goalKey = key(goal.x, goal.y);
  if (!cameFrom.has(goalKey)) return target;

  let step = goal;
  let previous = cameFrom.get(goalKey);
  while (previous && !(previous.x === start.x && previous.y === start.y)) {
    step = previous;
    previous = cameFrom.get(key(step.x, step.y));
  }

  return {
    x: (step.x + 0.5) * TILE,
    y: (step.y + 0.5) * TILE,
  };
}

function nearestBench() {
  let best = null;
  let bestDistance = Infinity;
  for (const bench of game.benches) {
    const d = distance(game.player, bench);
    if (d < bestDistance) {
      best = bench;
      bestDistance = d;
    }
  }
  return { bench: best, distance: bestDistance };
}

function toggleHide() {
  if (game.won || game.lost) return;
  if (game.hidden) {
    game.hidden = false;
    game.ogreSawHide = false;
    return;
  }

  const near = nearestBench();
  if (near.distance <= HIDE_RADIUS) {
    game.hidden = true;
    game.ogreSawHide = hasLineOfSight(game.ogre, game.player, OGRE_VISIBILITY);
  }
}

function updatePlayer(dt) {
  const turningLeft = keys.has("ArrowLeft") || touch.left;
  const turningRight = keys.has("ArrowRight") || touch.right;
  if (turningLeft) game.player.angle -= dt * 2.4;
  if (turningRight) game.player.angle += dt * 2.4;

  const forward = keys.has("KeyW") || keys.has("ArrowUp") || touch.forward;
  const back = keys.has("KeyS") || keys.has("ArrowDown") || touch.back;
  const strafeLeft = keys.has("KeyA");
  const strafeRight = keys.has("KeyD");

  if (game.hidden && (forward || back || strafeLeft || strafeRight)) {
    game.hidden = false;
    game.ogreSawHide = false;
  }

  if (game.hidden) return;

  let dx = 0;
  let dy = 0;

  if (forward) {
    dx += Math.cos(game.player.angle) * PLAYER_SPEED * dt;
    dy += Math.sin(game.player.angle) * PLAYER_SPEED * dt;
  }
  if (back) {
    dx -= Math.cos(game.player.angle) * PLAYER_SPEED * 0.72 * dt;
    dy -= Math.sin(game.player.angle) * PLAYER_SPEED * 0.72 * dt;
  }
  if (strafeLeft) {
    dx += Math.cos(game.player.angle - Math.PI / 2) * PLAYER_SPEED * 0.72 * dt;
    dy += Math.sin(game.player.angle - Math.PI / 2) * PLAYER_SPEED * 0.72 * dt;
  }
  if (strafeRight) {
    dx += Math.cos(game.player.angle + Math.PI / 2) * PLAYER_SPEED * 0.72 * dt;
    dy += Math.sin(game.player.angle + Math.PI / 2) * PLAYER_SPEED * 0.72 * dt;
  }

  moveEntity(game.player, dx, dy);
}

function updateOgre(dt) {
  if (game.won || game.lost) return;
  const visible = !game.hidden && hasLineOfSight(game.ogre, game.player, OGRE_VISIBILITY);
  if (visible) {
    game.ogre.lastSeenX = game.player.x;
    game.ogre.lastSeenY = game.player.y;
    game.ogre.searchX = null;
    game.ogre.searchY = null;
  }

  if (game.hidden && !game.ogreSawHide && game.ogre.lastSeenX === null) {
    pickSearchTarget();
  }

  const target = {
    x: game.ogre.lastSeenX ?? game.ogre.searchX ?? game.player.x,
    y: game.ogre.lastSeenY ?? game.ogre.searchY ?? game.player.y,
  };
  const nextStep = findNextStep(game.ogre, target);
  const dx = nextStep.x - game.ogre.x;
  const dy = nextStep.y - game.ogre.y;
  const len = Math.hypot(dx, dy);

  if (len > 6) {
    moveEntity(game.ogre, (dx / len) * OGRE_SPEED * dt, (dy / len) * OGRE_SPEED * dt);
  }

  if (Math.hypot(target.x - game.ogre.x, target.y - game.ogre.y) < 18) {
    game.ogre.lastSeenX = null;
    game.ogre.lastSeenY = null;
    pickSearchTarget();
  }

  if (distance(game.ogre, game.player) < 28 && (!game.hidden || game.ogreSawHide)) {
    game.lost = true;
    showMessage("Caught", "The ogre found you. Restart and use the benches carefully.", "Restart");
  }
}

function pickSearchTarget() {
  const options = [];
  const ogreCell = gridPoint(game.ogre);
  for (let y = Math.max(1, ogreCell.y - 4); y <= Math.min(game.height - 2, ogreCell.y + 4); y += 1) {
    for (let x = Math.max(1, ogreCell.x - 4); x <= Math.min(game.width - 2, ogreCell.x + 4); x += 1) {
      if (isOpenCell(x, y)) options.push({ x: (x + 0.5) * TILE, y: (y + 0.5) * TILE });
    }
  }
  const choice = options[Math.floor(Math.random() * options.length)] ?? game.player;
  game.ogre.searchX = choice.x;
  game.ogre.searchY = choice.y;
}

function checkExit() {
  if (distance(game.player, game.exit) < 34) {
    game.won = true;
    const next = game.levelIndex + 1;
    if (next < levels.length) {
      showMessage("Escaped", "You found the route out. The next maze is waiting.", "Next Level");
    } else {
      showMessage("Prototype Clear", "You escaped all five starter levels.", "Play Again");
    }
  }
}

function showMessage(title, text, buttonText) {
  ui.message.querySelector("h1").textContent = title;
  ui.message.querySelector("p").textContent = text;
  ui.startButton.textContent = buttonText;
  ui.message.classList.remove("hidden");
}

function castRay(angle) {
  const sin = Math.sin(angle);
  const cos = Math.cos(angle);
  let distanceTravelled = 0;

  while (distanceTravelled < PLAYER_VISIBILITY) {
    const x = game.player.x + cos * distanceTravelled;
    const y = game.player.y + sin * distanceTravelled;
    if (isWall(x, y)) {
      return { distance: distanceTravelled, hitX: x, hitY: y };
    }
    distanceTravelled += 4;
  }

  return { distance: PLAYER_VISIBILITY, hitX: game.player.x + cos * PLAYER_VISIBILITY, hitY: game.player.y + sin * PLAYER_VISIBILITY };
}

function drawScene(width, height) {
  const sky = ctx.createLinearGradient(0, 0, 0, height * 0.52);
  sky.addColorStop(0, "#141b18");
  sky.addColorStop(1, "#30352a");
  ctx.fillStyle = sky;
  ctx.fillRect(0, 0, width, height * 0.52);

  const floor = ctx.createLinearGradient(0, height * 0.48, 0, height);
  floor.addColorStop(0, "#16140f");
  floor.addColorStop(1, "#302619");
  ctx.fillStyle = floor;
  ctx.fillRect(0, height * 0.48, width, height);

  const sliceWidth = width / RAYS + 1;
  for (let i = 0; i < RAYS; i += 1) {
    const rayAngle = game.player.angle - FOV / 2 + (i / RAYS) * FOV;
    const ray = castRay(rayAngle);
    const corrected = Math.max(1, ray.distance * Math.cos(rayAngle - game.player.angle));
    const wallHeight = Math.min(height, (TILE * height) / corrected);
    const shade = Math.max(34, 210 - corrected * 0.18);
    const sideShade = Math.abs((ray.hitX % TILE) - TILE / 2) < Math.abs((ray.hitY % TILE) - TILE / 2) ? 0.85 : 1;
    ctx.fillStyle = `rgb(${Math.floor(shade * 0.52 * sideShade)}, ${Math.floor(shade * 0.58 * sideShade)}, ${Math.floor(shade * 0.48 * sideShade)})`;
    ctx.fillRect(i * sliceWidth, (height - wallHeight) / 2, sliceWidth, wallHeight);
  }

  drawSprite(game.exit, width, height, "#70d88b", "EXIT", 0.85);
  for (const bench of game.benches) drawSprite(bench, width, height, "#b6804c", "BENCH", 0.5);
  drawSprite(game.ogre, width, height, "#c84032", "OGRE", 1.2);

  const vignette = ctx.createRadialGradient(width / 2, height / 2, height * 0.12, width / 2, height / 2, height * 0.76);
  vignette.addColorStop(0, "rgba(0,0,0,0)");
  vignette.addColorStop(1, "rgba(0,0,0,0.56)");
  ctx.fillStyle = vignette;
  ctx.fillRect(0, 0, width, height);
}

function drawSprite(entity, width, height, color, label, scale) {
  const dx = entity.x - game.player.x;
  const dy = entity.y - game.player.y;
  const distanceToEntity = Math.hypot(dx, dy);
  if (distanceToEntity > PLAYER_VISIBILITY) return;

  let angleToEntity = Math.atan2(dy, dx) - game.player.angle;
  while (angleToEntity < -Math.PI) angleToEntity += Math.PI * 2;
  while (angleToEntity > Math.PI) angleToEntity -= Math.PI * 2;
  if (Math.abs(angleToEntity) > FOV / 2 + 0.2) return;
  if (!hasLineOfSight(game.player, entity, PLAYER_VISIBILITY)) return;

  const x = (0.5 + angleToEntity / FOV) * width;
  const size = Math.min(height * 0.9, (TILE * height * scale) / Math.max(24, distanceToEntity));
  const y = height * 0.52 - size * 0.72;

  ctx.fillStyle = color;
  ctx.fillRect(x - size * 0.28, y, size * 0.56, size);
  ctx.fillStyle = "rgba(0,0,0,0.32)";
  ctx.fillRect(x - size * 0.22, y + size * 0.12, size * 0.44, size * 0.16);
  ctx.fillStyle = "#fff6e6";
  ctx.font = `${Math.max(10, Math.min(16, size * 0.16))}px sans-serif`;
  ctx.textAlign = "center";
  ctx.fillText(label, x, y - 8);
}

function drawMapOverlay(width, height) {
  const mapSize = Math.min(width, height) * 0.78;
  const cell = mapSize / Math.max(game.width, game.height);
  const left = (width - game.width * cell) / 2;
  const top = (height - game.height * cell) / 2;

  ctx.fillStyle = "rgba(4, 6, 5, 0.86)";
  ctx.fillRect(0, 0, width, height);
  for (let y = 0; y < game.height; y += 1) {
    for (let x = 0; x < game.width; x += 1) {
      ctx.fillStyle = game.grid[y][x] === "#" ? "#d7d0bc" : "#20251f";
      ctx.fillRect(left + x * cell, top + y * cell, cell - 1, cell - 1);
    }
  }

  for (const bench of game.benches) drawMapDot(bench, left, top, cell, "#b6804c", 0.66);
  drawMapDot(game.exit, left, top, cell, "#70d88b", 0.78);
  drawMapDot(game.ogre, left, top, cell, "#c84032", 0.78);
  drawMapDot(game.player, left, top, cell, "#f4f1e8", 0.7);

  ctx.fillStyle = "#f4f1e8";
  ctx.font = "18px sans-serif";
  ctx.textAlign = "center";
  ctx.fillText("Map view", width / 2, top - 18);
}

function drawMapDot(entity, left, top, cell, color, scale) {
  ctx.fillStyle = color;
  ctx.beginPath();
  ctx.arc(left + (entity.x / TILE) * cell, top + (entity.y / TILE) * cell, cell * scale, 0, Math.PI * 2);
  ctx.fill();
}

function drawMiniPrompts(width, height) {
  const near = nearestBench();
  let prompt = "";
  if (near.distance <= HIDE_RADIUS && !game.hidden) prompt = "Near bench: press H or Hide";
  if (game.hidden) prompt = game.ogreSawHide ? "Hidden, but seen: move to leave" : "Hidden: move or press Hide to leave";
  if (!prompt) return;

  ctx.fillStyle = game.hidden && !game.ogreSawHide ? "rgba(27, 80, 50, 0.82)" : "rgba(18, 18, 16, 0.82)";
  ctx.fillRect(width / 2 - 170, height - 76, 340, 42);
  ctx.fillStyle = "#fff8ea";
  ctx.font = "16px sans-serif";
  ctx.textAlign = "center";
  ctx.fillText(prompt, width / 2, height - 50);
}

function render() {
  const width = canvas.clientWidth;
  const height = canvas.clientHeight;
  ctx.clearRect(0, 0, width, height);
  drawScene(width, height);
  if (performance.now() < mapOverlayUntil) drawMapOverlay(width, height);
  drawMiniPrompts(width, height);
}

function updateUi() {
  ui.levelName.textContent = game.name;
  ui.mapViews.textContent = String(game.mapViews);
  ui.hideButton.textContent = game.hidden ? "Leave" : "Hide";
  if (game.lost) ui.stateText.textContent = "Caught";
  else if (game.won) ui.stateText.textContent = "Escaped";
  else if (game.hidden) ui.stateText.textContent = "Hidden";
  else if (hasLineOfSight(game.ogre, game.player, OGRE_VISIBILITY)) ui.stateText.textContent = "Seen";
  else ui.stateText.textContent = "Run";
}

function tick(now) {
  const dt = Math.min(0.05, (now - lastTime) / 1000);
  lastTime = now;
  if (running && !game.won && !game.lost) {
    updatePlayer(dt);
    updateOgre(dt);
    checkExit();
  }
  updateUi();
  render();
  requestAnimationFrame(tick);
}

function startOrAdvance() {
  if (game.won) {
    const next = game.levelIndex + 1;
    game = createGame(next < levels.length ? next : 0);
  } else if (game.lost) {
    game = createGame(game.levelIndex);
  }
  running = true;
  ui.message.classList.add("hidden");
  lastTime = performance.now();
}

function showMap() {
  if (game.mapViews <= 0 || game.won || game.lost) return;
  game.mapViews -= 1;
  mapOverlayUntil = performance.now() + 2600;
}

window.addEventListener("resize", resizeCanvas);
window.addEventListener("keydown", (event) => {
  keys.add(event.code);
  if (event.code === "KeyM") showMap();
  if (event.code === "KeyH" || event.code === "KeyE" || event.code === "Space") toggleHide();
});
window.addEventListener("keyup", (event) => keys.delete(event.code));

canvas.addEventListener("click", () => canvas.requestPointerLock?.());
canvas.addEventListener("pointerdown", (event) => {
  lookDrag.active = true;
  lookDrag.lastX = event.clientX;
  canvas.setPointerCapture?.(event.pointerId);
});
canvas.addEventListener("pointermove", (event) => {
  if (!running || document.pointerLockElement === canvas || !lookDrag.active) return;
  const movementX = event.clientX - lookDrag.lastX;
  lookDrag.lastX = event.clientX;
  game.player.angle += movementX * 0.006;
});
canvas.addEventListener("pointerup", (event) => {
  lookDrag.active = false;
  canvas.releasePointerCapture?.(event.pointerId);
});
canvas.addEventListener("pointercancel", () => {
  lookDrag.active = false;
});
window.addEventListener("mousemove", (event) => {
  if (document.pointerLockElement === canvas && running) {
    game.player.angle += event.movementX * 0.002;
  }
});

document.querySelectorAll("[data-move], [data-turn]").forEach((button) => {
  const set = (value) => {
    if (button.dataset.move) touch[button.dataset.move] = value;
    if (button.dataset.turn) touch[button.dataset.turn] = value;
  };
  button.addEventListener("pointerdown", () => set(true));
  button.addEventListener("pointerup", () => set(false));
  button.addEventListener("pointerleave", () => set(false));
  button.addEventListener("pointercancel", () => set(false));
});

ui.startButton.addEventListener("click", startOrAdvance);
ui.restartButton.addEventListener("click", () => {
  game = createGame(game.levelIndex);
  running = true;
  ui.message.classList.add("hidden");
});
ui.hideButton.addEventListener("click", toggleHide);
ui.mapButton.addEventListener("click", showMap);

resizeCanvas();
requestAnimationFrame(tick);
