extends RefCounted
class_name LevelLoader

static func load_level(one_based_index: int) -> LevelData:
	var path := "res://levels/level_%d.txt" % one_based_index
	var text := FileAccess.get_file_as_string(path)
	if text.is_empty():
		push_error("Missing or empty level file: %s" % path)
	return parse(text)

static func parse(text: String) -> LevelData:
	var raw_lines := text.replace("\r", "").split("\n")
	var lines: Array[String] = []
	var width := 0
	for line in raw_lines:
		if line.is_empty():
			continue
		lines.append(line)
		width = max(width, line.length())

	var data := LevelData.new()
	data.width = width
	data.height = lines.size()

	var player_count := 0
	var ogre_count := 0
	var gate_count := 0

	for row in range(lines.size()):
		var line := lines[row]
		var grid_y := data.height - 1 - row
		var walkable_row: Array[bool] = []
		for x in range(width):
			var marker := "#" if x >= line.length() else line[x]
			var cell := Vector2i(x, grid_y)
			var can_walk := marker != "#"
			walkable_row.append(can_walk)

			match marker:
				"#":
					data.walls.append(cell)
				"P":
					player_count += 1
					data.player_spawn = cell
				"O":
					ogre_count += 1
					data.ogre_spawn = cell
				"B":
					data.benches.append(cell)
				"G", "E":
					gate_count += 1
					data.gate = cell

		data.walkable.push_front(walkable_row)

	_validate(data, player_count, ogre_count, gate_count)
	return data

static func _validate(data: LevelData, player_count: int, ogre_count: int, gate_count: int) -> void:
	if player_count != 1:
		push_error("Level must contain exactly one P. Found %d." % player_count)
	if ogre_count != 1:
		push_error("Level must contain exactly one O. Found %d." % ogre_count)
	if gate_count != 1:
		push_error("Level must contain exactly one G. Found %d." % gate_count)
	if data.benches.size() != GameConstants.BENCHES_PER_LEVEL:
		push_error("Level must contain exactly %d B tiles. Found %d." % [GameConstants.BENCHES_PER_LEVEL, data.benches.size()])
	if not GridPathfinder.find_path(data, data.player_spawn, data.gate):
		push_error("Player spawn cannot reach gate.")
	if not GridPathfinder.find_path(data, data.ogre_spawn, data.player_spawn):
		push_error("Ogre spawn is not connected to player spawn.")
