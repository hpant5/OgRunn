extends RefCounted
class_name LevelData

const GameConstants = preload("res://scripts/constants.gd")

var width := 0
var height := 0
var walkable: Array = []
var walls: Array[Vector2i] = []
var benches: Array[Vector2i] = []
var player_spawn := Vector2i.ZERO
var ogre_spawn := Vector2i.ZERO
var gate := Vector2i.ZERO

func is_walkable(cell: Vector2i) -> bool:
	if cell.x < 0 or cell.y < 0 or cell.x >= width or cell.y >= height:
		return false
	return walkable[cell.y][cell.x]

func grid_to_world(cell: Vector2i) -> Vector2:
	return Vector2(
		float(cell.x) * GameConstants.TILE_SIZE + GameConstants.TILE_SIZE * 0.5,
		float(height - 1 - cell.y) * GameConstants.TILE_SIZE + GameConstants.TILE_SIZE * 0.5
	)

func world_to_grid(position: Vector2) -> Vector2i:
	return Vector2i(
		int(floor(position.x / GameConstants.TILE_SIZE)),
		height - 1 - int(floor(position.y / GameConstants.TILE_SIZE))
	)
