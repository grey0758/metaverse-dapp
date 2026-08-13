class_name BoardroomArt
extends Node2D

const CARPET := preload("res://assets/boardroom/carpet.png")
const OAK := preload("res://assets/boardroom/oak.png")
const PLATO_DISPLAY := preload("res://assets/boardroom/plato-display.png")

const SCREEN_RECT := Rect2(880.0, 70.0, 640.0, 360.0)
const COBALT := Color("123d87")
const COBALT_DARK := Color("092453")
const GRAPHITE := Color("202a31")
const CHARCOAL := Color("10171c")
const CYAN := Color("4fd2dc")
const WARM_WHITE := Color("eef0ec")
const BRASS := Color("b69352")
const RED := Color("b8323e")
const GREEN := Color("315f48")

var _screen_active := false
var _pulse := 0.0


func _ready() -> void:
	texture_repeat = CanvasItem.TEXTURE_REPEAT_ENABLED
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
	_draw_room_shell()
	_draw_front_wall()
	_draw_windows()
	_draw_oak_slat_wall()
	_draw_tables_and_chairs()
	_draw_side_furniture()
	_draw_floor_details()


func _draw_room_shell() -> void:
	draw_rect(BoardroomLayout.ROOM_RECT.grow(22.0), Color(0.01, 0.02, 0.025, 0.5), true)
	draw_rect(BoardroomLayout.ROOM_RECT, WARM_WHITE, true)
	draw_texture_rect(CARPET, BoardroomLayout.WALKABLE_RECT, true, Color(0.84, 0.87, 0.88))
	draw_rect(BoardroomLayout.WALKABLE_RECT, Color(0.14, 0.18, 0.2, 0.22), false, 3.0)

	var bottom_wall := Rect2(120.0, 1520.0, 2160.0, 80.0)
	draw_rect(bottom_wall, GRAPHITE, true)
	draw_rect(Rect2(180.0, 1520.0, 2040.0, 8.0), BRASS, true)


func _draw_front_wall() -> void:
	draw_rect(Rect2(120.0, 80.0, 2160.0, 350.0), COBALT_DARK, true)
	for column in range(18):
		var x := 140.0 + column * 120.0
		var shade := COBALT.lightened(0.035 if column % 2 == 0 else 0.0)
		draw_rect(Rect2(x, 92.0, 112.0, 326.0), shade, true)

	draw_rect(SCREEN_RECT.grow(15.0), CHARCOAL, true)
	draw_texture_rect(PLATO_DISPLAY, SCREEN_RECT, false)
	var glow_alpha := 0.32 + (sin(_pulse * 2.0) + 1.0) * 0.2 if _screen_active else 0.12
	draw_rect(SCREEN_RECT.grow(7.0), Color(CYAN, glow_alpha), false, 5.0)

	for index in range(5):
		var banner_color := RED if index % 2 == 0 else Color("e8e3d8")
		var banner_rect := Rect2(246.0 + index * 80.0, 132.0, 46.0, 190.0)
		draw_rect(banner_rect, banner_color, true)
		draw_line(
			Vector2(banner_rect.position.x - 8.0, banner_rect.position.y),
			Vector2(banner_rect.end.x + 8.0, banner_rect.position.y),
			BRASS,
			6.0
		)

	draw_rect(Rect2(1110.0, 420.0, 180.0, 10.0), BRASS, true)


func _draw_windows() -> void:
	draw_rect(Rect2(120.0, 430.0, 90.0, 1090.0), GRAPHITE, true)
	for index in range(5):
		var y := 458.0 + index * 205.0
		var window_rect := Rect2(132.0, y, 66.0, 166.0)
		draw_rect(window_rect, Color("9bcbd6"), true)
		draw_rect(Rect2(window_rect.position, Vector2(66.0, 44.0)), Color("dce9e9"), true)
		draw_line(
			Vector2(window_rect.position.x, window_rect.position.y + 82.0),
			Vector2(window_rect.end.x, window_rect.position.y + 82.0),
			Color("61757d"),
			3.0
		)
		for building in range(3):
			var building_height := 28.0 + float((index + building) % 3) * 14.0
			draw_rect(Rect2(
				146.0 + building * 17.0,
				window_rect.end.y - building_height,
				13.0,
				building_height
			), Color("64767a"), true)


func _draw_oak_slat_wall() -> void:
	draw_texture_rect(OAK, Rect2(2190.0, 430.0, 90.0, 1090.0), true, Color(0.84, 0.72, 0.55))
	for index in range(9):
		var x := 2194.0 + index * 10.0
		draw_line(Vector2(x, 430.0), Vector2(x, 1520.0), Color(0.23, 0.16, 0.1, 0.55), 3.0)

	var door_rect := Rect2(2180.0, 1325.0, 100.0, 195.0)
	draw_rect(door_rect, Color("b07d47"), true)
	draw_rect(door_rect, CHARCOAL, false, 5.0)
	draw_circle(Vector2(2202.0, 1425.0), 6.0, BRASS)


func _draw_tables_and_chairs() -> void:
	var table_rects := BoardroomLayout.table_surface_rects()
	for row_index in table_rects.size():
		var table_rect := table_rects[row_index]
		draw_rect(Rect2(table_rect.position + Vector2(10.0, 12.0), table_rect.size), Color(0.01, 0.02, 0.025, 0.34), true)
		draw_texture_rect(OAK, table_rect, true, Color(0.92, 0.78, 0.58))
		draw_rect(table_rect, Color(0.31, 0.2, 0.11, 0.72), false, 3.0)
		draw_rect(Rect2(table_rect.position + Vector2(26.0, 29.0), Vector2(table_rect.size.x - 52.0, 8.0)), BRASS, true)

		for chair_index in BoardroomLayout.CHAIR_COUNT_PER_SIDE:
			var x := table_rect.position.x + (float(chair_index) + 0.5) * table_rect.size.x / float(BoardroomLayout.CHAIR_COUNT_PER_SIDE)
			_draw_chair(Vector2(x, table_rect.position.y - 37.0), true)
			_draw_chair(Vector2(x, table_rect.end.y + 37.0), false)

		for device_index in range(4):
			var device_x := table_rect.position.x + 225.0 + device_index * 320.0
			draw_rect(Rect2(device_x, table_rect.position.y + 16.0, 64.0, 38.0), Color("26343a"), true)
			draw_rect(Rect2(device_x + 7.0, table_rect.position.y + 21.0, 50.0, 22.0), Color("6eb5bd"), true)


func _draw_chair(center: Vector2, faces_down: bool) -> void:
	var direction := 1.0 if faces_down else -1.0
	draw_rect(Rect2(center + Vector2(-23.0, -17.0 + direction * 5.0), Vector2(46.0, 38.0)), Color(0.01, 0.015, 0.02, 0.3), true)
	draw_rect(Rect2(center + Vector2(-20.0, -16.0), Vector2(40.0, 34.0)), Color("222d33"), true)
	draw_rect(Rect2(center + Vector2(-18.0, -14.0), Vector2(36.0, 24.0)), Color("35464d"), true)
	var back_y := center.y - direction * 25.0
	draw_line(Vector2(center.x - 19.0, back_y), Vector2(center.x + 19.0, back_y), CHARCOAL, 8.0)
	draw_line(Vector2(center.x - 19.0, back_y), Vector2(center.x - 19.0, center.y), CHARCOAL, 4.0)
	draw_line(Vector2(center.x + 19.0, back_y), Vector2(center.x + 19.0, center.y), CHARCOAL, 4.0)


func _draw_side_furniture() -> void:
	var lectern := Rect2(1930.0, 475.0, 120.0, 120.0)
	draw_rect(Rect2(lectern.position + Vector2(8.0, 10.0), lectern.size), Color(0.0, 0.0, 0.0, 0.28), true)
	draw_texture_rect(OAK, lectern, true, Color(0.82, 0.67, 0.46))
	draw_rect(lectern, BRASS, false, 4.0)
	draw_rect(Rect2(1952.0, 492.0, 76.0, 42.0), Color("24353b"), true)
	draw_circle(Vector2(1990.0, 568.0), 8.0, CYAN)

	var rear_console := Rect2(260.0, 1390.0, 210.0, 72.0)
	draw_texture_rect(OAK, rear_console, true, Color(0.78, 0.62, 0.42))
	draw_rect(rear_console, GRAPHITE, false, 3.0)
	for cup_index in range(3):
		draw_circle(Vector2(300.0 + cup_index * 56.0, 1418.0), 11.0, Color("e4e7e3"))

	_draw_planter(Vector2(336.0, 796.0))
	_draw_planter(Vector2(2076.0, 756.0))


func _draw_planter(center: Vector2) -> void:
	draw_circle(center + Vector2(4.0, 6.0), 35.0, Color(0.0, 0.0, 0.0, 0.25))
	draw_circle(center, 33.0, Color("3d4748"))
	for leaf_index in range(7):
		var angle := float(leaf_index) / 7.0 * TAU
		var leaf_center := center + Vector2.from_angle(angle) * 21.0
		draw_circle(leaf_center, 13.0, GREEN.lightened(0.04 * float(leaf_index % 3)))


func _draw_floor_details() -> void:
	for center_y in BoardroomLayout.TABLE_CENTERS:
		draw_rect(Rect2(222.0, center_y - 3.0, 240.0, 6.0), Color(BRASS, 0.55), true)
		draw_rect(Rect2(1938.0, center_y - 3.0, 240.0, 6.0), Color(BRASS, 0.55), true)

	var font := ThemeDB.fallback_font
	draw_string(font, Vector2(1900.0, 1492.0), "EXIT", HORIZONTAL_ALIGNMENT_LEFT, -1.0, 20, Color("dbe9e7"))
	draw_circle(Vector2(2152.0, 1460.0), 8.0, CYAN)
