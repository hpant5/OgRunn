extends Node2D

const GameConstants = preload("res://scripts/constants.gd")
const LevelLoader = preload("res://scripts/level_loader.gd")
const LevelData = preload("res://scripts/level_data.gd")
const PlayerController = preload("res://scripts/player.gd")
const OgreAI = preload("res://scripts/ogre_ai.gd")

enum GameState { MAIN_MENU, LEVEL_START, PLAYING, MAP_REVEAL, PAUSED, LEVEL_COMPLETE, GAME_OVER }

var level_index := 1
var max_levels := 5
var level: LevelData
var player: PlayerController
var ogre: OgreAI
var state := GameState.PLAYING
var reveal_uses_left := GameConstants.MAP_REVEAL_USES_PER_LEVEL
var reveal_ends_at := 0.0

@onready var world := $World
@onready var actors := $Actors
@onready var hud := $CanvasLayer/HUD
@onready var message := $CanvasLayer/Message

func _ready() -> void:
	load_level(level_index)

func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("pause_game"):
		_toggle_pause()
	if Input.is_action_just_pressed("restart_level"):
		load_level(level_index)
	if Input.is_action_just_pressed("map_reveal"):
		_start_map_reveal()

	if state == GameState.MAP_REVEAL and Time.get_ticks_msec() / 1000.0 >= reveal_ends_at:
		state = GameState.PLAYING
		player.frozen = false
		ogre.frozen = false
		_update_camera(false)

	_update_hud()

func load_level(index: int) -> void:
	for child in world.get_children():
		child.queue_free()
	for child in actors.get_children():
		child.queue_free()

	level = LevelLoader.load_level(index)
	reveal_uses_left = GameConstants.MAP_REVEAL_USES_PER_LEVEL
	state = GameState.PLAYING
	_build_world()
	_spawn_actors()
	_update_camera(false)
	message.text = ""

func _build_world() -> void:
	for wall in level.walls:
		_add_tile(wall, Color(0.11, 0.12, 0.16), "Wall")
	for bench in level.benches:
		_add_tile(bench, Color(0.55, 0.32, 0.12), "Bench")
	_add_tile(level.gate, Color(0.05, 0.62, 0.28), "Gate")

func _spawn_actors() -> void:
	player = PlayerController.new()
	player.init(level, self)
	actors.add_child(player)
	_add_actor_visual(player, Color(0.16, 0.52, 1.0), "Player")

	ogre = OgreAI.new()
	ogre.init(level, player, self)
	actors.add_child(ogre)
	_add_actor_visual(ogre, Color(0.8, 0.12, 0.08), "Ogre")

	var camera := Camera2D.new()
	camera.name = "Camera2D"
	camera.position_smoothing_enabled = true
	camera.zoom = Vector2(1.65, 1.65)
	player.add_child(camera)
	camera.make_current()

func _add_tile(cell: Vector2i, color: Color, tile_name: String) -> void:
	var rect := ColorRect.new()
	rect.name = tile_name
	rect.color = color
	rect.size = Vector2(GameConstants.TILE_SIZE, GameConstants.TILE_SIZE)
	rect.position = level.grid_to_world(cell) - rect.size * 0.5
	world.add_child(rect)

func _add_actor_visual(parent: Node2D, color: Color, visual_name: String) -> void:
	var rect := ColorRect.new()
	rect.name = visual_name
	rect.color = color
	rect.size = Vector2(GameConstants.TILE_SIZE * 0.7, GameConstants.TILE_SIZE * 0.7)
	rect.position = -rect.size * 0.5
	parent.add_child(rect)

func _start_map_reveal() -> void:
	if state != GameState.PLAYING or reveal_uses_left <= 0:
		return
	reveal_uses_left -= 1
	state = GameState.MAP_REVEAL
	reveal_ends_at = Time.get_ticks_msec() / 1000.0 + GameConstants.MAP_REVEAL_DURATION_SECONDS
	player.frozen = true
	ogre.frozen = true
	_update_camera(true)

func _update_camera(revealing: bool) -> void:
	var camera := player.get_node_or_null("Camera2D") as Camera2D
	if camera == null:
		return
	if revealing:
		var world_width := level.width * GameConstants.TILE_SIZE
		var world_height := level.height * GameConstants.TILE_SIZE
		camera.global_position = Vector2(world_width, world_height) * 0.5
		camera.zoom = Vector2(0.95, 0.95)
	else:
		camera.position = Vector2.ZERO
		camera.zoom = Vector2(1.65, 1.65)

func _toggle_pause() -> void:
	if state == GameState.PAUSED:
		state = GameState.PLAYING
		player.frozen = false
		ogre.frozen = false
	elif state == GameState.PLAYING:
		state = GameState.PAUSED
		player.frozen = true
		ogre.frozen = true

func _update_hud() -> void:
	var state_name: String = str(GameState.keys()[state])
	var hidden := "hidden" if player != null and player.is_hidden() else "visible"
	hud.text = "Level %d/%d | Map: %d | %s | Ogre: %s" % [
		level_index,
		max_levels,
		reveal_uses_left,
		hidden,
		state_name
	]

func _on_gate_check_timeout() -> void:
	if state != GameState.PLAYING or player == null:
		return
	if level.world_to_grid(player.position) == level.gate:
		on_level_complete()

func on_level_complete() -> void:
	state = GameState.LEVEL_COMPLETE
	player.frozen = true
	ogre.frozen = true
	if level_index < max_levels:
		level_index += 1
		message.text = "Level complete"
		await get_tree().create_timer(1.0).timeout
		load_level(level_index)
	else:
		message.text = "You escaped"

func on_player_caught() -> void:
	if state == GameState.GAME_OVER:
		return
	state = GameState.GAME_OVER
	player.frozen = true
	ogre.frozen = true
	message.text = "Caught - press R"
