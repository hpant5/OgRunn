extends CharacterBody2D
class_name PlayerController

const GameConstants = preload("res://scripts/constants.gd")
const LevelData = preload("res://scripts/level_data.gd")

var level: LevelData
var game: Node
var frozen := false

func init(level_data: LevelData, game_node: Node) -> void:
	level = level_data
	game = game_node
	position = level.grid_to_world(level.player_spawn)

func _physics_process(_delta: float) -> void:
	if level == null or frozen:
		velocity = Vector2.ZERO
		return

	var direction := Input.get_vector("move_left", "move_right", "move_up", "move_down")
	velocity = direction.normalized() * GameConstants.PLAYER_SPEED
	if velocity == Vector2.ZERO:
		return

	var next_position := position + velocity * _delta
	var next_cell := level.world_to_grid(next_position)
	if level.is_walkable(next_cell):
		position = next_position
	else:
		velocity = Vector2.ZERO

func is_hidden() -> bool:
	if level == null:
		return false
	return level.benches.has(level.world_to_grid(position))
