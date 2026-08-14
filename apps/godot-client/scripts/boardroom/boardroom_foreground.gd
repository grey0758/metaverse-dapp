class_name BoardroomForeground
extends Node2D

const ROOM_TEXTURE := preload("res://assets/boardroom/plato-boardroom-hifi.png")
const ROOM_SIZE := Vector2(2560.0, 1440.0)

var _fixed_occluders: Array[PackedVector2Array] = []
var _seat_occluders: Dictionary = {}
var _active_seat_id: StringName = &""


func _ready() -> void:
	_build_occluders()
	queue_redraw()


func occluder_count() -> int:
	return _fixed_occluders.size() + _seat_occluders.size()


func set_active_seat(seat_id: StringName) -> void:
	if seat_id == _active_seat_id:
		return
	_active_seat_id = seat_id
	queue_redraw()


func _draw() -> void:
	for polygon in _fixed_occluders:
		_draw_texture_polygon(polygon)
	for seat_id in _seat_occluders:
		if seat_id != _active_seat_id:
			_draw_texture_polygon(_seat_occluders[seat_id])


func _build_occluders() -> void:
	_fixed_occluders.assign([
		PackedVector2Array([
			Vector2(555.0, 82.0), Vector2(588.0, 82.0), Vector2(665.0, 258.0),
			Vector2(628.0, 294.0), Vector2(578.0, 226.0), Vector2(555.0, 218.0),
		]),
		PackedVector2Array([
			Vector2(665.0, 82.0), Vector2(696.0, 82.0), Vector2(761.0, 258.0),
			Vector2(723.0, 296.0), Vector2(682.0, 250.0), Vector2(665.0, 218.0),
		]),
		PackedVector2Array([
			Vector2(776.0, 82.0), Vector2(808.0, 82.0), Vector2(872.0, 258.0),
			Vector2(824.0, 297.0), Vector2(786.0, 244.0), Vector2(776.0, 214.0),
		]),
		PackedVector2Array([
			Vector2(1748.0, 82.0), Vector2(1777.0, 82.0), Vector2(1818.0, 242.0),
			Vector2(1766.0, 296.0), Vector2(1722.0, 256.0), Vector2(1741.0, 198.0),
		]),
		PackedVector2Array([
			Vector2(1858.0, 82.0), Vector2(1888.0, 82.0), Vector2(1938.0, 248.0),
			Vector2(1888.0, 296.0), Vector2(1838.0, 260.0), Vector2(1852.0, 198.0),
		]),
		PackedVector2Array([
			Vector2(660.0, 344.0), Vector2(780.0, 344.0), Vector2(784.0, 405.0),
			Vector2(775.0, 476.0), Vector2(692.0, 476.0), Vector2(686.0, 406.0),
			Vector2(660.0, 402.0),
		]),
	])

	var surfaces := BoardroomLayout.table_surface_rects()
	var obstacles := BoardroomLayout.table_obstacle_rects()
	for table_index in surfaces.size():
		var surface := surfaces[table_index]
		var obstacle := obstacles[table_index]
		_fixed_occluders.append(_rect_polygon(Rect2(
			surface.position - Vector2(4.0, 6.0),
			surface.size + Vector2(8.0, 30.0)
		)))
		_fixed_occluders.append(_rect_polygon(Rect2(
			Vector2(obstacle.position.x, surface.end.y - 2.0),
			Vector2(24.0, obstacle.end.y - surface.end.y + 46.0)
		)))
		_fixed_occluders.append(_rect_polygon(Rect2(
			Vector2(obstacle.end.x - 24.0, surface.end.y - 2.0),
			Vector2(24.0, obstacle.end.y - surface.end.y + 46.0)
		)))

	for seat in BoardroomLayout.seats():
		var table_index: int = seat.table_index
		var surface := surfaces[table_index]
		var obstacle := obstacles[table_index]
		var center_x: float = seat.anchor.x
		if seat.side == &"north":
			_seat_occluders[seat.id] = PackedVector2Array([
				Vector2(center_x - 30.0, surface.position.y + 8.0),
				Vector2(center_x - 39.0, surface.position.y - 23.0),
				Vector2(center_x - 31.0, surface.position.y - 50.0),
				Vector2(center_x + 31.0, surface.position.y - 50.0),
				Vector2(center_x + 39.0, surface.position.y - 23.0),
				Vector2(center_x + 30.0, surface.position.y + 8.0),
			])
		else:
			_seat_occluders[seat.id] = PackedVector2Array([
				Vector2(center_x - 39.0, surface.end.y - 4.0),
				Vector2(center_x + 39.0, surface.end.y - 4.0),
				Vector2(center_x + 36.0, obstacle.end.y + 40.0),
				Vector2(center_x + 27.0, obstacle.end.y + 72.0),
				Vector2(center_x - 27.0, obstacle.end.y + 72.0),
				Vector2(center_x - 36.0, obstacle.end.y + 40.0),
			])


func _draw_texture_polygon(polygon: PackedVector2Array) -> void:
	var colors := PackedColorArray()
	var uvs := PackedVector2Array()
	for point in polygon:
		colors.append(Color.WHITE)
		uvs.append(point / ROOM_SIZE)
	draw_polygon(polygon, colors, uvs, ROOM_TEXTURE)


func _rect_polygon(rect: Rect2) -> PackedVector2Array:
	return PackedVector2Array([
		rect.position,
		Vector2(rect.end.x, rect.position.y),
		rect.end,
		Vector2(rect.position.x, rect.end.y),
	])
