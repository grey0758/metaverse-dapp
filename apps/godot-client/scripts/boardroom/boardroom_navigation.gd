class_name BoardroomNavigation
extends NavigationRegion2D

signal navigation_ready

const AGENT_RADIUS := 24.0


func bake_layout() -> void:
	var polygon := NavigationPolygon.new()
	polygon.agent_radius = AGENT_RADIUS
	polygon.cell_size = 4.0

	var source_geometry := NavigationMeshSourceGeometryData2D.new()
	source_geometry.add_traversable_outline(
		BoardroomLayout.rect_outline(BoardroomLayout.WALKABLE_RECT)
	)
	for obstacle in BoardroomLayout.navigation_obstacles():
		source_geometry.add_projected_obstruction(
			BoardroomLayout.rect_outline(obstacle),
			true
		)

	NavigationServer2D.bake_from_source_geometry_data(polygon, source_geometry)
	navigation_polygon = polygon
	call_deferred("_announce_ready")


func _announce_ready() -> void:
	navigation_ready.emit()
