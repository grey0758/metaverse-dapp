class_name BoardroomArt
extends Node2D

const ROOM_TEXTURE := preload("res://assets/boardroom/plato-boardroom-hifi.png")
const PLATO_DISPLAY := preload("res://assets/boardroom/plato-display.png")

var _screen_points := PackedVector2Array([
	Vector2(976.0, 101.0),
	Vector2(1583.0, 102.0),
	Vector2(1571.0, 278.0),
	Vector2(998.0, 277.0),
])
var _screen_uvs := PackedVector2Array([
	Vector2(0.0, 0.205),
	Vector2(1.0, 0.205),
	Vector2(1.0, 0.795),
	Vector2(0.0, 0.795),
])
var _screen_colors := PackedColorArray([
	Color.WHITE,
	Color.WHITE,
	Color.WHITE,
	Color.WHITE,
])
const CYAN := Color("4fd2dc")

var _screen_active := false
var _pulse := 0.0


func _ready() -> void:
	queue_redraw()


func _process(delta: float) -> void:
	if not _screen_active:
		return
	_pulse = fmod(_pulse + delta, TAU)
	queue_redraw()


func set_screen_active(active: bool) -> void:
	_screen_active = active
	queue_redraw()


func toggle_screen() -> bool:
	_screen_active = not _screen_active
	queue_redraw()
	return _screen_active


func _draw() -> void:
	draw_texture_rect(ROOM_TEXTURE, BoardroomLayout.ROOM_RECT, false)
	draw_polygon(_screen_points, _screen_colors, _screen_uvs, PLATO_DISPLAY)

	if not _screen_active:
		draw_colored_polygon(_screen_points, Color(0.01, 0.025, 0.055, 0.22))

	var glow_alpha := 0.45 + (sin(_pulse * 2.0) + 1.0) * 0.2 if _screen_active else 0.2
	var outline := PackedVector2Array(_screen_points)
	outline.append(_screen_points[0])
	draw_polyline(outline, Color(CYAN, glow_alpha), 5.0, true)
	draw_circle(Vector2(1281.0, 291.0), 5.0, Color(CYAN, glow_alpha + 0.15))
