extends RefCounted
class_name GridPathfinder

const LevelData = preload("res://scripts/level_data.gd")

const DIRECTIONS: Array[Vector2i] = [
	Vector2i.RIGHT,
	Vector2i.LEFT,
	Vector2i.UP,
	Vector2i.DOWN
]

static func find_path(level: LevelData, start: Vector2i, goal: Vector2i) -> Array[Vector2i]:
	if not level.is_walkable(start) or not level.is_walkable(goal):
		return []
	if start == goal:
		return [start]

	var open: Array[Vector2i] = [start]
	var came_from := {}
	var g_score := { start: 0 }
	var f_score := { start: _heuristic(start, goal) }

	while not open.is_empty():
		var current := _lowest_f(open, f_score)
		if current == goal:
			return _reconstruct(came_from, current)

		open.erase(current)
		for direction in DIRECTIONS:
			var neighbor := current + direction
			if not level.is_walkable(neighbor):
				continue

			var tentative_g: int = g_score[current] + 1
			if not g_score.has(neighbor) or tentative_g < g_score[neighbor]:
				came_from[neighbor] = current
				g_score[neighbor] = tentative_g
				f_score[neighbor] = tentative_g + _heuristic(neighbor, goal)
				if not open.has(neighbor):
					open.append(neighbor)

	return []

static func random_walkable(level: LevelData, rng: RandomNumberGenerator) -> Vector2i:
	for attempt in range(200):
		var cell := Vector2i(rng.randi_range(0, level.width - 1), rng.randi_range(0, level.height - 1))
		if level.is_walkable(cell):
			return cell

	for y in range(level.height):
		for x in range(level.width):
			var fallback := Vector2i(x, y)
			if level.is_walkable(fallback):
				return fallback

	return Vector2i.ZERO

static func _lowest_f(open: Array[Vector2i], f_score: Dictionary) -> Vector2i:
	var best := open[0]
	var best_score: int = f_score.get(best, 999999)
	for cell in open:
		var score: int = f_score.get(cell, 999999)
		if score < best_score:
			best = cell
			best_score = score
	return best

static func _heuristic(a: Vector2i, b: Vector2i) -> int:
	return abs(a.x - b.x) + abs(a.y - b.y)

static func _reconstruct(came_from: Dictionary, current: Vector2i) -> Array[Vector2i]:
	var path: Array[Vector2i] = [current]
	while came_from.has(current):
		current = came_from[current]
		path.push_front(current)
	return path
