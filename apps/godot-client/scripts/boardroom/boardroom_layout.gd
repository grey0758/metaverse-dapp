class_name BoardroomLayout
extends RefCounted

const ROOM_RECT := Rect2(0.0, 0.0, 2560.0, 1440.0)
const PLAYER_SPAWN := Vector2(1280.0, 850.0)
const TABLE_CENTERS := [539.0, 737.0, 947.0, 1176.0]
const CHAIR_COUNT_PER_SIDE := 10
const SEAT_INTERACTION_RADIUS := 78.0
const SEAT_EDGE_MARGIN := 80.0


static func walkable_outline() -> PackedVector2Array:
	return PackedVector2Array([
		Vector2(485.0, 320.0),
		Vector2(2075.0, 320.0),
		Vector2(2375.0, 1380.0),
		Vector2(165.0, 1380.0),
	])


static func table_surface_rects() -> Array[Rect2]:
	return [
		Rect2(710.0, 500.0, 1080.0, 78.0),
		Rect2(690.0, 695.0, 1125.0, 84.0),
		Rect2(670.0, 904.0, 1165.0, 88.0),
		Rect2(650.0, 1132.0, 1200.0, 88.0),
	]


static func table_obstacle_rects() -> Array[Rect2]:
	return [
		Rect2(690.0, 480.0, 1120.0, 118.0),
		Rect2(670.0, 675.0, 1165.0, 120.0),
		Rect2(650.0, 884.0, 1205.0, 126.0),
		Rect2(630.0, 1112.0, 1245.0, 126.0),
	]


static func navigation_obstacles() -> Array[Rect2]:
	var rects := table_obstacle_rects()
	rects.append(Rect2(660.0, 345.0, 120.0, 130.0))
	return rects


static func physics_obstacles() -> Array[Rect2]:
	return navigation_obstacles()


static func interaction_points() -> Array[Dictionary]:
	return [
		{
			"id": &"screen",
			"label": "PRESENTATION",
			"position": Vector2(1280.0, 360.0),
			"radius": 120.0,
		},
		{
			"id": &"lectern",
			"label": "LECTERN",
			"position": Vector2(815.0, 430.0),
			"radius": 120.0,
		},
		{
			"id": &"exit",
			"label": "EXIT",
			"position": Vector2(2225.0, 1280.0),
			"radius": 115.0,
		},
	]


static func seats() -> Array[Dictionary]:
	var result: Array[Dictionary] = []
	var surfaces := table_surface_rects()
	var obstacles := table_obstacle_rects()
	for table_index in surfaces.size():
		var surface := surfaces[table_index]
		var obstacle := obstacles[table_index]
		var north_approach_y := obstacle.position.y - 68.0
		if table_index > 0:
			north_approach_y = obstacles[table_index - 1].end.y + 25.0
		var south_approach_y := obstacle.end.y + 68.0
		if table_index < obstacles.size() - 1:
			south_approach_y = obstacles[table_index + 1].position.y - 25.0
		else:
			south_approach_y = 1344.0

		for seat_index in CHAIR_COUNT_PER_SIDE:
			var ratio := float(seat_index) / float(CHAIR_COUNT_PER_SIDE - 1)
			var seat_x := lerpf(
				surface.position.x + SEAT_EDGE_MARGIN,
				surface.end.x - SEAT_EDGE_MARGIN,
				ratio
			)
			var north_approach_x := seat_x
			if table_index == 0 and seat_index == 0:
				north_approach_x = 824.0
			result.append(_seat(
				table_index,
				seat_index,
				&"north",
				Vector2(seat_x, obstacle.position.y + 28.0),
				Vector2(north_approach_x, north_approach_y),
				&"sit_down",
				&"walk_down",
				result.size()
			))
			result.append(_seat(
				table_index,
				seat_index,
				&"south",
				Vector2(seat_x, obstacle.end.y + 54.0),
				Vector2(seat_x, south_approach_y),
				&"sit_up",
				&"walk_up",
				result.size()
			))
	return result


static func seat_by_id(seat_id: StringName) -> Dictionary:
	for seat in seats():
		if seat.id == seat_id:
			return seat
	return {}


static func _seat(
	table_index: int,
	seat_index: int,
	side: StringName,
	anchor: Vector2,
	approach: Vector2,
	animation: StringName,
	walk_animation: StringName,
	global_index: int
) -> Dictionary:
	return {
		"id": StringName("seat_t%02d_%s_%02d" % [table_index + 1, side, seat_index + 1]),
		"label": "CHAIR %02d" % (global_index + 1),
		"table_index": table_index,
		"seat_index": seat_index,
		"side": side,
		"anchor": anchor,
		"approach": approach,
		"animation": animation,
		"walk_animation": walk_animation,
		"radius": SEAT_INTERACTION_RADIUS,
	}


static func rect_outline(rect: Rect2) -> PackedVector2Array:
	return PackedVector2Array([
		rect.position,
		Vector2(rect.end.x, rect.position.y),
		rect.end,
		Vector2(rect.position.x, rect.end.y),
	])


static func point_is_walkable(point: Vector2) -> bool:
	if not Geometry2D.is_point_in_polygon(point, walkable_outline()):
		return false
	for obstacle in navigation_obstacles():
		if obstacle.has_point(point):
			return false
	return true
