extends Node2D

const PlayerController = preload("res://scripts/player.gd")

enum GameState { PLAYING, LEVEL_COMPLETE, GAME_OVER }

const START_POSITION := Vector2(110.0, 120.0)
const RIVER_RECT := Rect2(Vector2(300.0, -80.0), Vector2(340.0, 3280.0))
const FINISH_Y := 3030.0
const WATER_DANGER_SECONDS := 10.0
const MAX_HUNGER := 10
const MAX_RAFT_HEALTH := 100.0
const RAFT_CURRENT_SPEED := 92.0
const RAFT_STEER_SPEED := 145.0
const RAFT_DECAY_PER_SECOND := 4.0
const ANIMAL_ATTACK_COOLDOWN := 2.2

var state := GameState.PLAYING
var player: PlayerController
var raft: Node2D
var raft_visual: ColorRect
var player_on_raft := false
var raft_built := false
var raft_health := MAX_RAFT_HEALTH
var hunger := MAX_HUNGER
var wood := 0
var rope := 0
var fish := 0
var water_seconds := 0.0
var animal_attack_timer := 0.0
var message_timer := 0.0
var resource_items: Array[Dictionary] = []
var animal_positions: Array[Vector2] = []

@onready var background := $Background as ColorRect
@onready var world := $World as Node2D
@onready var actors := $Actors as Node2D
@onready var hud := $CanvasLayer/HUD as Label
@onready var message := $CanvasLayer/Message as Label

func _ready() -> void:
	_start_river_level()

func _process(delta: float) -> void:
	if Input.is_action_just_pressed("restart_level"):
		_start_river_level()
		return

	if state != GameState.PLAYING:
		_update_hud()
		return

	message_timer = maxf(0.0, message_timer - delta)
	animal_attack_timer = maxf(0.0, animal_attack_timer - delta)
	if message_timer <= 0.0:
		message.text = ""

	_handle_actions()
	_update_raft(delta)
	_update_survival_timers(delta)
	_update_animal_hazards()
	_check_finish()
	_update_hud()

func _start_river_level() -> void:
	for child in world.get_children():
		child.queue_free()
	for child in actors.get_children():
		child.queue_free()

	state = GameState.PLAYING
	player_on_raft = false
	raft_built = false
	raft_health = MAX_RAFT_HEALTH
	hunger = MAX_HUNGER
	wood = 0
	rope = 0
	fish = 0
	water_seconds = 0.0
	animal_attack_timer = 0.0
	resource_items.clear()
	animal_positions.clear()

	background.color = Color(0.09, 0.2, 0.12)
	_build_world()
	_spawn_player()
	_show_message("Find wood and rope. Build a raft at the river.", 4.0)

func _build_world() -> void:
	_add_rect(world, RIVER_RECT.position, RIVER_RECT.size, Color(0.04, 0.32, 0.58), "River")
	_add_rect(world, Vector2(225.0, -80.0), Vector2(38.0, 3280.0), Color(0.13, 0.28, 0.11), "Left Ridge")
	_add_rect(world, Vector2(678.0, -80.0), Vector2(45.0, 3280.0), Color(0.12, 0.25, 0.1), "Right Ridge")
	_add_rect(world, Vector2(300.0, FINISH_Y), Vector2(340.0, 120.0), Color(0.08, 0.58, 0.32), "Finish")

	for hill in [
		Rect2(Vector2(-60.0, 390.0), Vector2(260.0, 150.0)),
		Rect2(Vector2(710.0, 650.0), Vector2(300.0, 160.0)),
		Rect2(Vector2(-90.0, 1450.0), Vector2(280.0, 190.0)),
		Rect2(Vector2(720.0, 2160.0), Vector2(310.0, 210.0))
	]:
		_add_rect(world, hill.position, hill.size, Color(0.17, 0.34, 0.14), "Hill Forest")

	for item in [
		{"type": "wood", "pos": Vector2(155.0, 170.0)},
		{"type": "wood", "pos": Vector2(690.0, 250.0)},
		{"type": "wood", "pos": Vector2(165.0, 520.0)},
		{"type": "wood", "pos": Vector2(710.0, 920.0)},
		{"type": "wood", "pos": Vector2(150.0, 1220.0)},
		{"type": "wood", "pos": Vector2(705.0, 1760.0)},
		{"type": "rope", "pos": Vector2(735.0, 430.0)},
		{"type": "rope", "pos": Vector2(165.0, 820.0)},
		{"type": "rope", "pos": Vector2(715.0, 1380.0)},
		{"type": "rope", "pos": Vector2(160.0, 2050.0)}
	]:
		_add_resource(item["type"], item["pos"])

	for pos in [
		Vector2(750.0, 1110.0),
		Vector2(125.0, 1560.0),
		Vector2(735.0, 2340.0)
	]:
		animal_positions.append(pos)
		_add_marker(pos, Color(0.75, 0.16, 0.08), "Wild Animal")

func _spawn_player() -> void:
	player = PlayerController.new()
	player.init(self, START_POSITION)
	actors.add_child(player)
	_add_actor_visual(player, Color(0.96, 0.78, 0.28), "Player")

	var camera := Camera2D.new()
	camera.name = "Camera2D"
	camera.position_smoothing_enabled = true
	camera.zoom = Vector2(1.15, 1.15)
	player.add_child(camera)
	camera.make_current()

func _handle_actions() -> void:
	if Input.is_action_just_pressed("interact"):
		if _try_collect_resource():
			return
		if raft_built:
			_toggle_raft_mount()
		else:
			_try_build_raft()

	if Input.is_action_just_pressed("repair_raft"):
		_try_repair_raft()

	if Input.is_action_just_pressed("fish"):
		_try_fish()

	if Input.is_action_just_pressed("eat_fish"):
		_try_eat_fish()

func _try_collect_resource() -> bool:
	for index in range(resource_items.size()):
		var item := resource_items[index]
		if player.global_position.distance_to(item["pos"]) > 48.0:
			continue
		if item["type"] == "wood":
			wood += 1
			_show_message("Collected wood", 1.4)
		else:
			rope += 1
			_show_message("Collected rope", 1.4)
		var node := item["node"] as Node
		node.queue_free()
		resource_items.remove_at(index)
		return true
	return false

func _try_build_raft() -> void:
	if wood < 5:
		_show_message("Need 5 wood to build the raft", 2.0)
		return
	if not _is_near_river(player.global_position):
		_show_message("Build the raft at the river edge", 2.0)
		return

	wood -= 5
	raft_built = true
	raft_health = MAX_RAFT_HEALTH
	raft = Node2D.new()
	raft.name = "Raft"
	raft.global_position = Vector2(clampf(player.global_position.x, RIVER_RECT.position.x + 45.0, RIVER_RECT.end.x - 45.0), player.global_position.y)
	actors.add_child(raft)
	raft_visual = _add_actor_visual(raft, Color(0.46, 0.27, 0.11), "Raft")
	raft_visual.size = Vector2(86.0, 42.0)
	raft_visual.position = -raft_visual.size * 0.5
	_show_message("Raft built. Press E nearby to board.", 3.0)

func _toggle_raft_mount() -> void:
	if player_on_raft:
		player_on_raft = false
		player.on_raft = false
		player.global_position = raft.global_position + Vector2(-72.0, 0.0)
		_show_message("Back on shore", 1.4)
		return

	if raft != null and player.global_position.distance_to(raft.global_position) <= 78.0:
		player_on_raft = true
		player.on_raft = true
		player.global_position = raft.global_position
		_show_message("Current is pulling the raft downstream", 2.5)

func _try_repair_raft() -> void:
	if not raft_built or raft == null:
		return
	if not player_on_raft and player.global_position.distance_to(raft.global_position) > 90.0:
		_show_message("Get closer to the raft to repair it", 2.0)
		return
	if wood < 1 or rope < 1:
		_show_message("Repair needs 1 wood and 1 rope", 2.0)
		return

	wood -= 1
	rope -= 1
	raft_health = minf(MAX_RAFT_HEALTH, raft_health + 28.0)
	_show_message("Raft repaired", 1.5)

func _try_fish() -> void:
	if not player_on_raft and not _is_near_river(player.global_position):
		_show_message("Fish from the river edge or raft", 2.0)
		return
	if randf() < 0.72:
		fish += 1
		_show_message("Caught a fish", 1.5)
	else:
		_show_message("The fish slipped away", 1.5)

func _try_eat_fish() -> void:
	if fish <= 0:
		return
	fish -= 1
	hunger = mini(MAX_HUNGER, hunger + 2)
	_show_message("Ate fish. Hunger restored.", 1.6)

func _update_raft(delta: float) -> void:
	if not player_on_raft or raft == null:
		return

	var steer := Input.get_axis("move_left", "move_right")
	raft.global_position.x += steer * RAFT_STEER_SPEED * delta
	raft.global_position.y += RAFT_CURRENT_SPEED * delta
	raft.global_position.x = clampf(raft.global_position.x, RIVER_RECT.position.x + 42.0, RIVER_RECT.end.x - 42.0)
	player.global_position = raft.global_position

	raft_health -= RAFT_DECAY_PER_SECOND * delta
	if raft_health <= 0.0:
		raft_health = 0.0
		player_on_raft = false
		player.on_raft = false
		_show_message("The raft broke. Get to shore and rebuild your supplies.", 3.0)

func _update_survival_timers(delta: float) -> void:
	if player_on_raft:
		water_seconds = 0.0
		return

	if is_position_in_water(player.global_position):
		water_seconds += delta
		if water_seconds >= WATER_DANGER_SECONDS:
			water_seconds = 0.0
			_damage_hunger("Alligator attack in deep water")
	else:
		water_seconds = 0.0

func _update_animal_hazards() -> void:
	if player_on_raft or is_position_in_water(player.global_position) or animal_attack_timer > 0.0:
		return
	for pos in animal_positions:
		if player.global_position.distance_to(pos) <= 72.0:
			animal_attack_timer = ANIMAL_ATTACK_COOLDOWN
			_damage_hunger("Wild animal attack on shore")
			return

func _damage_hunger(reason: String) -> void:
	hunger -= 1
	_show_message("%s. Hunger -1." % reason, 2.0)
	if hunger <= 0:
		hunger = 0
		state = GameState.GAME_OVER
		player.frozen = true
		message.text = "You did not survive. Press R to restart."

func _check_finish() -> void:
	if player.global_position.y < FINISH_Y:
		return
	state = GameState.LEVEL_COMPLETE
	player.frozen = true
	message.text = "River cleared. Press R to play again."

func _on_gate_check_timeout() -> void:
	pass

func is_position_in_water(pos: Vector2) -> bool:
	return RIVER_RECT.has_point(pos)

func _is_near_river(pos: Vector2) -> bool:
	var expanded := RIVER_RECT.grow(52.0)
	return expanded.has_point(pos)

func _update_hud() -> void:
	var raft_text := "not built"
	if raft_built:
		raft_text = "%d%%" % int(raft_health)
	var water_text := "%.1fs" % water_seconds if is_position_in_water(player.global_position) and not player_on_raft else "safe"
	hud.text = "Hunger %d/%d | Wood %d | Rope %d | Fish %d | Raft %s | Water %s | E use/build/board | Q repair | F fish | C eat | R restart" % [
		hunger,
		MAX_HUNGER,
		wood,
		rope,
		fish,
		raft_text,
		water_text
	]

func _show_message(text: String, seconds: float) -> void:
	message.text = text
	message_timer = seconds

func _add_rect(parent: Node, pos: Vector2, size: Vector2, color: Color, rect_name: String) -> ColorRect:
	var rect := ColorRect.new()
	rect.name = rect_name
	rect.position = pos
	rect.size = size
	rect.color = color
	parent.add_child(rect)
	return rect

func _add_resource(resource_type: String, pos: Vector2) -> void:
	var color := Color(0.56, 0.34, 0.14) if resource_type == "wood" else Color(0.76, 0.68, 0.45)
	var node := _add_marker(pos, color, resource_type.capitalize())
	resource_items.append({"type": resource_type, "pos": pos, "node": node})

func _add_marker(pos: Vector2, color: Color, marker_name: String) -> ColorRect:
	var marker := ColorRect.new()
	marker.name = marker_name
	marker.color = color
	marker.size = Vector2(28.0, 28.0)
	marker.position = pos - marker.size * 0.5
	world.add_child(marker)
	return marker

func _add_actor_visual(parent: Node2D, color: Color, visual_name: String) -> ColorRect:
	var rect := ColorRect.new()
	rect.name = visual_name
	rect.color = color
	rect.size = Vector2(32.0, 32.0)
	rect.position = -rect.size * 0.5
	parent.add_child(rect)
	return rect
