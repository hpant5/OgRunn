extends Node2D
class_name GameSprite

enum SpriteKind { PLAYER, RAFT, WOOD, ROPE, ANIMAL, ALLIGATOR, FISH, TREE }

@export var kind := SpriteKind.PLAYER
@export var base_scale := 1.0

func setup(sprite_kind: int, sprite_scale: float = 1.0) -> void:
	kind = sprite_kind
	base_scale = sprite_scale
	queue_redraw()

func _draw() -> void:
	match kind:
		SpriteKind.PLAYER:
			_draw_player()
		SpriteKind.RAFT:
			_draw_raft()
		SpriteKind.WOOD:
			_draw_wood()
		SpriteKind.ROPE:
			_draw_rope()
		SpriteKind.ANIMAL:
			_draw_animal()
		SpriteKind.ALLIGATOR:
			_draw_alligator()
		SpriteKind.FISH:
			_draw_fish()
		SpriteKind.TREE:
			_draw_tree()

func _draw_player() -> void:
	var s := base_scale
	draw_circle(Vector2(0.0, -15.0) * s, 7.0 * s, Color(0.79, 0.54, 0.32))
	draw_colored_polygon([
		Vector2(-9.0, -7.0) * s,
		Vector2(9.0, -7.0) * s,
		Vector2(12.0, 16.0) * s,
		Vector2(-12.0, 16.0) * s
	], Color(0.88, 0.68, 0.21))
	draw_line(Vector2(-8.0, 1.0) * s, Vector2(-17.0, 12.0) * s, Color(0.79, 0.54, 0.32), 4.0 * s)
	draw_line(Vector2(8.0, 1.0) * s, Vector2(17.0, 12.0) * s, Color(0.79, 0.54, 0.32), 4.0 * s)
	draw_line(Vector2(-5.0, 15.0) * s, Vector2(-9.0, 27.0) * s, Color(0.2, 0.13, 0.08), 4.0 * s)
	draw_line(Vector2(5.0, 15.0) * s, Vector2(9.0, 27.0) * s, Color(0.2, 0.13, 0.08), 4.0 * s)

func _draw_raft() -> void:
	var s := base_scale
	for y in [-16.0, 0.0, 16.0]:
		draw_rect(Rect2(Vector2(-46.0, y - 6.0) * s, Vector2(92.0, 12.0) * s), Color(0.45, 0.25, 0.1))
		draw_line(Vector2(-35.0, y) * s, Vector2(35.0, y) * s, Color(0.32, 0.17, 0.07), 2.0 * s)
	draw_line(Vector2(-28.0, -24.0) * s, Vector2(-28.0, 24.0) * s, Color(0.71, 0.61, 0.38), 4.0 * s)
	draw_line(Vector2(28.0, -24.0) * s, Vector2(28.0, 24.0) * s, Color(0.71, 0.61, 0.38), 4.0 * s)

func _draw_wood() -> void:
	var s := base_scale
	draw_rect(Rect2(Vector2(-18.0, -7.0) * s, Vector2(36.0, 14.0) * s), Color(0.5, 0.29, 0.11))
	draw_circle(Vector2(-18.0, 0.0) * s, 7.0 * s, Color(0.68, 0.45, 0.22))
	draw_circle(Vector2(18.0, 0.0) * s, 7.0 * s, Color(0.36, 0.19, 0.07))
	draw_line(Vector2(-8.0, -4.0) * s, Vector2(10.0, -4.0) * s, Color(0.31, 0.16, 0.06), 2.0 * s)

func _draw_rope() -> void:
	var s := base_scale
	draw_arc(Vector2.ZERO, 15.0 * s, 0.0, TAU * 0.86, 36, Color(0.78, 0.68, 0.42), 5.0 * s)
	draw_arc(Vector2(5.0, 1.0) * s, 9.0 * s, 0.2, TAU * 0.8, 28, Color(0.59, 0.49, 0.28), 3.0 * s)

func _draw_animal() -> void:
	var s := base_scale
	_draw_ellipse(Vector2.ZERO, Vector2(46.0, 25.0) * s, Color(0.48, 0.25, 0.13))
	draw_circle(Vector2(23.0, -9.0) * s, 11.0 * s, Color(0.54, 0.29, 0.16))
	draw_colored_polygon([
		Vector2(16.0, -18.0) * s,
		Vector2(20.0, -30.0) * s,
		Vector2(25.0, -17.0) * s
	], Color(0.37, 0.18, 0.1))
	draw_colored_polygon([
		Vector2(25.0, -18.0) * s,
		Vector2(31.0, -29.0) * s,
		Vector2(32.0, -15.0) * s
	], Color(0.37, 0.18, 0.1))
	draw_line(Vector2(-20.0, 11.0) * s, Vector2(-26.0, 25.0) * s, Color(0.24, 0.12, 0.07), 4.0 * s)
	draw_line(Vector2(9.0, 11.0) * s, Vector2(5.0, 25.0) * s, Color(0.24, 0.12, 0.07), 4.0 * s)
	draw_circle(Vector2(28.0, -11.0) * s, 2.0 * s, Color.BLACK)

func _draw_alligator() -> void:
	var s := base_scale
	_draw_ellipse(Vector2(-8.0, 0.0) * s, Vector2(68.0, 22.0) * s, Color(0.17, 0.42, 0.19))
	draw_colored_polygon([
		Vector2(20.0, -9.0) * s,
		Vector2(54.0, -5.0) * s,
		Vector2(54.0, 5.0) * s,
		Vector2(20.0, 10.0) * s
	], Color(0.18, 0.48, 0.21))
	draw_colored_polygon([
		Vector2(-42.0, -4.0) * s,
		Vector2(-66.0, 0.0) * s,
		Vector2(-42.0, 5.0) * s
	], Color(0.12, 0.32, 0.14))
	for x in [-24.0, -10.0, 4.0, 18.0]:
		draw_colored_polygon([
			Vector2(x, -11.0) * s,
			Vector2(x + 5.0, -19.0) * s,
			Vector2(x + 10.0, -10.0) * s
		], Color(0.09, 0.25, 0.1))
	draw_circle(Vector2(45.0, -5.0) * s, 2.5 * s, Color.BLACK)
	draw_line(Vector2(25.0, 2.0) * s, Vector2(53.0, 2.0) * s, Color(0.78, 0.86, 0.63), 2.0 * s)

func _draw_fish() -> void:
	var s := base_scale
	_draw_ellipse(Vector2.ZERO, Vector2(28.0, 16.0) * s, Color(0.96, 0.58, 0.23))
	draw_colored_polygon([
		Vector2(-14.0, 0.0) * s,
		Vector2(-27.0, -10.0) * s,
		Vector2(-27.0, 10.0) * s
	], Color(0.91, 0.38, 0.18))
	draw_circle(Vector2(8.0, -3.0) * s, 2.0 * s, Color.BLACK)

func _draw_tree() -> void:
	var s := base_scale
	draw_rect(Rect2(Vector2(-5.0, -1.0) * s, Vector2(10.0, 23.0) * s), Color(0.36, 0.2, 0.09))
	draw_circle(Vector2(0.0, -16.0) * s, 18.0 * s, Color(0.12, 0.38, 0.13))
	draw_circle(Vector2(-11.0, -6.0) * s, 13.0 * s, Color(0.09, 0.31, 0.11))
	draw_circle(Vector2(12.0, -6.0) * s, 13.0 * s, Color(0.16, 0.43, 0.16))

func _draw_ellipse(center: Vector2, size: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	var radius := size * 0.5
	for index in range(24):
		var angle := TAU * float(index) / 24.0
		points.append(center + Vector2(cos(angle) * radius.x, sin(angle) * radius.y))
	draw_colored_polygon(points, color)
