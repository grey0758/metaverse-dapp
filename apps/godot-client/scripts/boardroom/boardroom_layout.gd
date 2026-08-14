class_name BoardroomLayout
extends RefCounted

const ROOM_RECT := Rect2(0.0, 0.0, 2560.0, 1440.0)
const PLAYER_SPAWN := Vector2(1280.0, 850.0)
const TABLE_CENTERS := [539.0, 737.0, 947.0, 1176.0]
const CHAIR_COUNT_PER_SIDE := 10


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
