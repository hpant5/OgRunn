extends CharacterBody2D
class_name PlayerController

const WALK_SPEED := 210.0
const SWIM_SPEED := 125.0

var game: Node
var frozen := false
var on_raft := false

func init(game_node: Node, spawn_position: Vector2) -> void:
	game = game_node
	position = spawn_position

func _physics_process(_delta: float) -> void:
	if frozen or on_raft:
		velocity = Vector2.ZERO
		return

	var direction := Input.get_vector("move_left", "move_right", "move_up", "move_down")
	var speed := WALK_SPEED
	if game != null and game.has_method("is_position_in_water") and game.is_position_in_water(global_position):
		speed = SWIM_SPEED

	velocity = direction.normalized() * speed
	move_and_slide()
