class_name BoardroomLayout
extends RefCounted

const ROOM_RECT := Rect2(120.0, 80.0, 2160.0, 1520.0)
const WALKABLE_RECT := Rect2(210.0, 430.0, 1980.0, 1090.0)
const PLAYER_SPAWN := Vector2(1200.0, 995.0)
const TABLE_WIDTH := 1400.0
const TABLE_HEIGHT := 70.0
const TABLE_OBSTACLE_HEIGHT := 152.0
const TABLE_CENTERS := [650.0, 880.0, 1110.0, 1340.0]
const CHAIR_COUNT_PER_SIDE := 8


static func table_surface_rects() -> Array[Rect2]:
	var rects: Array[Rect2] = []
	for center_y in TABLE_CENTERS:
		rects.append(Rect2(
			Vector2(1200.0 - TABLE_WIDTH * 0.5, center_y - TABLE_HEIGHT * 0.5),
			Vector2(TABLE_WIDTH, TABLE_HEIGHT)
		))
	return rects


static func navigation_obstacles() -> Array[Rect2]:
	var rects: Array[Rect2] = []
	for center_y in TABLE_CENTERS:
		rects.append(Rect2(
			Vector2(1200.0 - TABLE_WIDTH * 0.5, center_y - TABLE_OBSTACLE_HEIGHT * 0.5),
			Vector2(TABLE_WIDTH, TABLE_OBSTACLE_HEIGHT)
		))

	rects.append(Rect2(1930.0, 475.0, 120.0, 120.0))
	rects.append(Rect2(260.0, 1390.0, 210.0, 72.0))
	rects.append(Rect2(2040.0, 720.0, 72.0, 72.0))
	rects.append(Rect2(300.0, 760.0, 72.0, 72.0))
	return rects


static func physics_obstacles() -> Array[Rect2]:
	var rects := navigation_obstacles()
	var room_end := ROOM_RECT.end
	var walk_end := WALKABLE_RECT.end

	rects.append(Rect2(
		ROOM_RECT.position,
		Vector2(ROOM_RECT.size.x, WALKABLE_RECT.position.y - ROOM_RECT.position.y)
	))
	rects.append(Rect2(
		Vector2(ROOM_RECT.position.x, WALKABLE_RECT.position.y),
		Vector2(WALKABLE_RECT.position.x - ROOM_RECT.position.x, WALKABLE_RECT.size.y)
	))
	rects.append(Rect2(
		Vector2(walk_end.x, WALKABLE_RECT.position.y),
		Vector2(room_end.x - walk_end.x, WALKABLE_RECT.size.y)
	))
	rects.append(Rect2(
		Vector2(ROOM_RECT.position.x, walk_end.y),
		Vector2(ROOM_RECT.size.x, room_end.y - walk_end.y)
	))
	return rects


static func interaction_points() -> Array[Dictionary]:
	return [
		{
			"id": &"screen",
			"label": "PRESENTATION",
			"position": Vector2(1200.0, 470.0),
			"radius": 118.0,
		},
		{
			"id": &"lectern",
			"label": "LECTERN",
			"position": Vector2(1880.0, 535.0),
			"radius": 126.0,
		},
		{
			"id": &"exit",
			"label": "EXIT",
			"position": Vector2(2130.0, 1435.0),
			"radius": 105.0,
		},
	]


static func rect_outline(rect: Rect2) -> PackedVector2Array:
	return PackedVector2Array([
		rect.position,
		Vector2(rect.end.x, rect.position.y),
		rect.end,
		Vector2(rect.position.x, rect.end.y),
	])


static func point_is_walkable(point: Vector2) -> bool:
	if not WALKABLE_RECT.has_point(point):
		return false
	for obstacle in navigation_obstacles():
		if obstacle.has_point(point):
			return false
	return true
