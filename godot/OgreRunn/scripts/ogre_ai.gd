extends CharacterBody2D
class_name OgreAI

enum OgreState { PATROL, CHASE, SEARCH, LOST }

var level: LevelData
var player: PlayerController
var game: Node
var state := OgreState.PATROL
var frozen := false

var chase_loss_seconds := 3.0
var search_duration := 6.0
var reach_tile_distance := 6.0
var path: Array[Vector2i] = []
var path_index := 0
var rng := RandomNumberGenerator.new()
var last_seen_cell := Vector2i.ZERO
var last_seen_time := -999.0
var search_ends_at := 0.0

func init(level_data: LevelData, player_node: PlayerController, game_node: Node) -> void:
	level = level_data
	player = player_node
	game = game_node
	rng.randomize()
	position = level.grid_to_world(level.ogre_spawn)
	_pick_new_patrol_goal()

func _physics_process(delta: float) -> void:
	if level == null or player == null or frozen:
		return

	var sees_player := _can_see_player()
	match state:
		OgreState.PATROL:
			if sees_player:
				_enter_chase()
		OgreState.CHASE:
			if sees_player:
				last_seen_cell = level.world_to_grid(player.position)
				last_seen_time = Time.get_ticks_msec() / 1000.0
				_repath_to(last_seen_cell)
			elif Time.get_ticks_msec() / 1000.0 - last_seen_time > chase_loss_seconds:
				_enter_search()
		OgreState.SEARCH:
			if sees_player:
				_enter_chase()
			elif Time.get_ticks_msec() / 1000.0 > search_ends_at:
				_enter_lost()
		OgreState.LOST:
			if sees_player:
				_enter_chase()
			else:
				_enter_patrol()

	_follow_path(delta)

	if position.distance_to(player.position) <= GameConstants.CATCH_DISTANCE and not player.is_hidden():
		game.on_player_caught()

func _can_see_player() -> bool:
	if player.is_hidden():
		return false
	if position.distance_to(player.position) > GameConstants.OGRE_DETECTION_RADIUS:
		return false
	var ogre_cell := level.world_to_grid(position)
	var player_cell := level.world_to_grid(player.position)
	return not GridPathfinder.find_path(level, ogre_cell, player_cell).is_empty()

func _enter_patrol() -> void:
	state = OgreState.PATROL
	_pick_new_patrol_goal()

func _enter_chase() -> void:
	state = OgreState.CHASE
	last_seen_cell = level.world_to_grid(player.position)
	last_seen_time = Time.get_ticks_msec() / 1000.0
	_repath_to(last_seen_cell)

func _enter_search() -> void:
	state = OgreState.SEARCH
	search_ends_at = Time.get_ticks_msec() / 1000.0 + search_duration
	_repath_to(last_seen_cell)

func _enter_lost() -> void:
	state = OgreState.LOST

func _pick_new_patrol_goal() -> void:
	_repath_to(GridPathfinder.random_walkable(level, rng))

func _repath_to(goal: Vector2i) -> void:
	path = GridPathfinder.find_path(level, level.world_to_grid(position), goal)
	path_index = 0

func _follow_path(delta: float) -> void:
	if path.is_empty() or path_index >= path.size():
		if state == OgreState.PATROL:
			_pick_new_patrol_goal()
		return

	var target := level.grid_to_world(path[path_index])
	var to_target := target - position
	if to_target.length() <= reach_tile_distance:
		path_index += 1
		return

	var speed := GameConstants.OGRE_SPEED
	if state == OgreState.PATROL:
		speed *= 0.7
	position += to_target.normalized() * speed * delta
