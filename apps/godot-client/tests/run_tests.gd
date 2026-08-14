extends SceneTree

const BOARDROOM_SCENE := preload("res://scenes/boardroom/boardroom.tscn")

var _checks := 0
var _failures: Array[String] = []


func _initialize() -> void:
	call_deferred("_run")


func _run() -> void:
	_test_project_settings()
	_test_layout_contract()
	_test_input_selection()
	_test_animation_selection()
	_test_camera_clamping()
	await _test_boardroom_scene()

	if _failures.is_empty():
		print("PASS: %d checks" % _checks)
		quit(0)
		return

	for failure in _failures:
		printerr("FAIL: " + failure)
	printerr("FAILED: %d of %d checks" % [_failures.size(), _checks])
	quit(1)


func _test_project_settings() -> void:
	_expect_equal(
		ProjectSettings.get_setting("display/window/size/viewport_width"),
		1280,
		"landscape viewport width"
	)
	_expect_equal(
		ProjectSettings.get_setting("display/window/size/viewport_height"),
		720,
		"landscape viewport height"
	)
	_expect_equal(
		ProjectSettings.get_setting("display/window/stretch/mode"),
		"canvas_items",
		"mobile 2D stretch mode"
	)
	_expect_equal(
		ProjectSettings.get_setting("display/window/stretch/aspect"),
		"expand",
		"wide-device stretch aspect"
	)
	_expect_equal(
		ProjectSettings.get_setting("rendering/textures/canvas_textures/default_texture_filter"),
		2,
		"high-resolution art uses linear mipmap filtering"
	)
	_expect_true(
		ProjectSettings.get_setting("input_devices/pointing/emulate_touch_from_mouse"),
		"mouse emulates touch for desktop development"
	)
	_expect_false(
		ProjectSettings.get_setting("input_devices/pointing/emulate_mouse_from_touch"),
		"touch does not synthesize duplicate mouse events"
	)


func _test_layout_contract() -> void:
	_expect_equal(BoardroomLayout.table_surface_rects().size(), 4, "four Plato table rows")
	_expect_equal(BoardroomLayout.CHAIR_COUNT_PER_SIDE, 10, "generated room has ten chairs per table side")
	var seats := BoardroomLayout.seats()
	var seat_ids := {}
	var north_count := 0
	var south_count := 0
	var all_approaches_walkable := true
	for seat in seats:
		seat_ids[seat.id] = true
		all_approaches_walkable = all_approaches_walkable and BoardroomLayout.point_is_walkable(seat.approach)
		if seat.side == &"north":
			north_count += 1
		elif seat.side == &"south":
			south_count += 1
	_expect_equal(seats.size(), 80, "four tables expose eighty seat anchors")
	_expect_equal(seat_ids.size(), 80, "every chair has a unique authoritative seat id")
	_expect_equal(north_count, 40, "forty chairs face south from the north table sides")
	_expect_equal(south_count, 40, "forty chairs face north from the south table sides")
	_expect_true(all_approaches_walkable, "every chair approach point is walkable")
	_expect_equal(
		BoardroomLayout.seat_by_id(seats[17].id).anchor,
		seats[17].anchor,
		"seat ids resolve to their exact sitting anchors"
	)
	_expect_true(
		BoardroomLayout.point_is_walkable(BoardroomLayout.PLAYER_SPAWN),
		"player spawn is walkable"
	)
	_expect_false(
		BoardroomLayout.point_is_walkable(Vector2(200.0, 340.0)),
		"trapezoid wall corners are not walkable"
	)
	for obstacle in BoardroomLayout.navigation_obstacles():
		_expect_false(obstacle.has_point(BoardroomLayout.PLAYER_SPAWN), "spawn clears every obstacle")
	_expect_false(
		BoardroomLayout.point_is_walkable(BoardroomLayout.table_surface_rects()[0].get_center()),
		"table center is not walkable"
	)


func _test_input_selection() -> void:
	_expect_vector_near(
		MovementInput.select_manual(Vector2(0.7, 0.0), Vector2.ZERO),
		Vector2(0.7, 0.0),
		0.001,
		"keyboard development input passes through"
	)
	_expect_vector_near(
		MovementInput.select_manual(Vector2(0.2, 0.0), Vector2(0.0, -0.8)),
		Vector2(0.0, -0.8),
		0.001,
		"stronger touch input wins"
	)
	_expect_vector_near(
		MovementInput.select_manual(Vector2.ZERO, Vector2(0.03, 0.01)),
		Vector2.ZERO,
		0.001,
		"manual input dead zone"
	)
	_expect_near(
		MovementInput.select_manual(Vector2(2.0, 1.0), Vector2.ZERO).length(),
		1.0,
		0.001,
		"manual input is normalized"
	)


func _test_animation_selection() -> void:
	_expect_equal(MovementInput.animation_for(Vector2.LEFT), &"walk_left", "left animation")
	_expect_equal(MovementInput.animation_for(Vector2.RIGHT), &"walk_right", "right animation")
	_expect_equal(MovementInput.animation_for(Vector2.UP), &"walk_up", "up animation")
	_expect_equal(MovementInput.animation_for(Vector2.DOWN), &"walk_down", "down animation")
	_expect_equal(
		MovementInput.animation_for(Vector2.ZERO, &"walk_left"),
		&"walk_left",
		"idle keeps last facing"
	)


func _test_camera_clamping() -> void:
	var bounds := Rect2(0.0, 0.0, 1000.0, 800.0)
	var viewport_size := Vector2(400.0, 200.0)
	_expect_vector_near(
		BoardroomCamera.clamp_center(Vector2(-100.0, -100.0), bounds, viewport_size, Vector2.ONE),
		Vector2(200.0, 100.0),
		0.001,
		"camera clamps at top-left"
	)
	_expect_vector_near(
		BoardroomCamera.clamp_center(Vector2(1200.0, 900.0), bounds, viewport_size, Vector2.ONE),
		Vector2(800.0, 700.0),
		0.001,
		"camera clamps at bottom-right"
	)
	_expect_vector_near(
		BoardroomCamera.clamp_center(Vector2.ZERO, Rect2(0.0, 0.0, 200.0, 100.0), viewport_size, Vector2.ONE),
		Vector2(100.0, 50.0),
		0.001,
		"camera centers when viewport exceeds room"
	)


func _test_boardroom_scene() -> void:
	root.size = Vector2i(1280, 720)
	await process_frame
	var boardroom := BOARDROOM_SCENE.instantiate() as Boardroom
	_expect_true(boardroom != null, "boardroom scene has its typed root script")
	if boardroom == null:
		return
	root.add_child(boardroom)
	await process_frame
	for _frame in range(5):
		await physics_frame

	_expect_true(is_instance_valid(boardroom.player), "scene has CharacterBody2D player")
	_expect_true(is_instance_valid(boardroom.player.navigation_agent), "player has NavigationAgent2D")
	_expect_true(is_instance_valid(boardroom.hud.virtual_joystick), "scene has community virtual joystick")
	_expect_equal(boardroom.seat_count(), 80, "scene registers all layout seats")
	_expect_equal(boardroom.foreground.occluder_count(), 98, "foreground redraws flags, lectern, tables, and chairs")
	_expect_true(
		boardroom.foreground.z_index > boardroom.player.z_index,
		"fixed facilities render above the character"
	)
	_expect_equal(
		boardroom.obstacles.get_child_count(),
		BoardroomLayout.physics_obstacles().size() + 1,
		"layout creates obstacle and perimeter colliders"
	)
	_expect_equal(
		boardroom.player.character_sprite.sprite_frames.get_frame_count(&"walk_down"),
		3,
		"character loads three-frame walk animation"
	)
	_expect_vector_near(
		boardroom.player.character_sprite.sprite_frames.get_frame_texture(&"walk_down", 0).get_size(),
		Vector2(176.0, 216.0),
		0.001,
		"character uses high-resolution business frames"
	)
	for seated_animation in [&"sit_down", &"sit_left", &"sit_up", &"sit_right"]:
		_expect_equal(
			boardroom.player.character_sprite.sprite_frames.get_frame_count(seated_animation),
			1,
			"%s has one stable seated pose" % seated_animation
		)
		_expect_vector_near(
			boardroom.player.character_sprite.sprite_frames.get_frame_texture(seated_animation, 0).get_size(),
			Vector2(176.0, 216.0),
			0.001,
			"%s uses the aligned business-character canvas" % seated_animation
		)

	var navigation_polygon := boardroom.navigation_region.navigation_polygon
	_expect_true(navigation_polygon != null, "navigation polygon is generated")
	if navigation_polygon != null:
		_expect_true(navigation_polygon.get_polygon_count() > 0, "navigation polygon contains walkable polygons")

	var navigation_map := boardroom.player.navigation_agent.get_navigation_map()
	_expect_true(navigation_map.is_valid(), "NavigationAgent2D is assigned to a map")
	_expect_true(
		NavigationServer2D.map_get_iteration_id(navigation_map) > 0,
		"navigation map synchronized"
	)
	var all_seat_approaches_on_navigation := true
	var all_seat_approaches_reachable := true
	for seat in BoardroomLayout.seats():
		var closest_approach := NavigationServer2D.map_get_closest_point(navigation_map, seat.approach)
		all_seat_approaches_on_navigation = (
			all_seat_approaches_on_navigation
			and closest_approach.distance_to(seat.approach) <= 5.0
		)
		var seat_path := NavigationServer2D.map_get_path(
			navigation_map,
			BoardroomLayout.PLAYER_SPAWN,
			closest_approach,
			true
		)
		all_seat_approaches_reachable = all_seat_approaches_reachable and not seat_path.is_empty()
	_expect_true(all_seat_approaches_on_navigation, "all chair approach points lie on the baked navigation map")
	_expect_true(all_seat_approaches_reachable, "all chair approach points are reachable from spawn")

	var start := BoardroomLayout.PLAYER_SPAWN
	var target := Vector2(1280.0, 360.0)
	var path := NavigationServer2D.map_get_path(navigation_map, start, target, true)
	_expect_true(path.size() >= 3, "path routes around four table obstacles")
	_expect_true(_path_length(path) > start.distance_to(target), "obstacle route is longer than direct line")

	boardroom.player.global_position = start
	boardroom.player.set_move_target(target)
	for _frame in range(3):
		await physics_frame
	_expect_true(boardroom.player.has_active_navigation(), "tap target activates NavigationAgent2D")

	Input.action_press("move_left", 1.0)
	await physics_frame
	Input.action_release("move_left")
	_expect_false(boardroom.player.has_active_navigation(), "manual input cancels tap navigation")

	boardroom.player.set_move_target(target)
	for _frame in range(3):
		await physics_frame
	_expect_true(boardroom.player.has_active_navigation(), "tap navigation can restart after manual input")

	var joystick_base := boardroom.hud.virtual_joystick.get_node("Base") as Control
	var joystick_center := joystick_base.get_global_rect().get_center()
	_parse_touch(7, joystick_center, true)
	_parse_drag(7, joystick_center + Vector2(64.0, 0.0), Vector2(64.0, 0.0))
	await process_frame
	_expect_true(boardroom.hud.virtual_joystick.is_pressed, "virtual joystick owns its touch pointer")
	_expect_true(boardroom.hud.virtual_joystick.output.x > 0.7, "virtual joystick emits normalized right input")
	var owned_output := boardroom.hud.virtual_joystick.output
	_parse_drag(8, joystick_center + Vector2(-64.0, 0.0), Vector2(-64.0, 0.0))
	await process_frame
	_expect_vector_near(
		boardroom.hud.virtual_joystick.output,
		owned_output,
		0.001,
		"second touch cannot steal the joystick"
	)
	await physics_frame
	_expect_false(boardroom.player.has_active_navigation(), "touch joystick cancels tap navigation")
	_parse_touch(7, joystick_center + Vector2(64.0, 0.0), false)
	await process_frame
	_expect_false(boardroom.hud.virtual_joystick.is_pressed, "virtual joystick releases its touch pointer")
	_expect_vector_near(
		boardroom.hud.virtual_joystick.output,
		Vector2.ZERO,
		0.001,
		"virtual joystick output resets on release"
	)

	var first_table := BoardroomLayout.table_obstacle_rects()[0]
	var collision_start := Vector2(first_table.position.x - 70.0, first_table.get_center().y)
	boardroom.player.global_position = collision_start
	Input.action_press("move_right", 1.0)
	for _frame in range(36):
		await physics_frame
	Input.action_release("move_right")
	_expect_true(
		boardroom.player.global_position.x <= first_table.position.x - 20.0,
		"CharacterBody2D stops at table collider"
	)
	_expect_true(
		boardroom.player.global_position.x > collision_start.x,
		"CharacterBody2D moved before collision"
	)

	boardroom.boardroom_camera.set_mode(BoardroomCamera.FREE)
	var camera_start := boardroom.boardroom_camera.global_position
	boardroom.boardroom_camera.pan_by_screen_delta(Vector2(80.0, 0.0))
	_expect_true(
		boardroom.boardroom_camera.global_position.x < camera_start.x,
		"FREE camera drag pans independently"
	)
	boardroom.boardroom_camera.set_mode(BoardroomCamera.LOCKED)
	_expect_equal(boardroom.boardroom_camera.mode, BoardroomCamera.LOCKED, "LOCK camera mode restores follow")

	var seats := BoardroomLayout.seats()
	var remotely_occupied := seats[3]
	_expect_false(
		boardroom.reserve_seat(&"missing_seat", &"remote_player"),
		"unknown seat ids cannot be reserved"
	)
	_expect_true(
		boardroom.reserve_seat(remotely_occupied.id, &"remote_player"),
		"an available seat accepts an authoritative occupant"
	)
	_expect_false(
		boardroom.reserve_seat(remotely_occupied.id, Boardroom.LOCAL_OCCUPANT_ID),
		"an occupied seat cannot be claimed twice"
	)
	_expect_equal(
		boardroom.seat_occupant(remotely_occupied.id),
		&"remote_player",
		"seat occupancy retains the authoritative occupant id"
	)
	_expect_false(
		boardroom.release_seat(remotely_occupied.id, Boardroom.LOCAL_OCCUPANT_ID),
		"a different occupant cannot release a reserved chair"
	)
	_expect_true(
		boardroom.release_seat(remotely_occupied.id, &"remote_player"),
		"the authoritative occupant can release a chair"
	)

	var selected_seat := seats[10]
	boardroom.player.global_position = selected_seat.approach
	boardroom._update_interaction()
	_expect_equal(boardroom.hud.action_text(), "SIT", "near an available chair the action becomes SIT")
	_expect_true(boardroom.hud.interaction_available(), "nearby available chair enables the action button")
	boardroom._interact()
	await process_frame
	_expect_true(boardroom.player.is_seat_transitioning(), "SIT begins a controlled alignment transition")
	_expect_equal(
		boardroom.seat_occupant(selected_seat.id),
		Boardroom.LOCAL_OCCUPANT_ID,
		"SIT reserves the chair before moving into it"
	)
	for _frame in range(20):
		await physics_frame
	await process_frame
	_expect_true(boardroom.player.is_seated(), "player reaches the seated state")
	_expect_false(boardroom.player.is_seat_transitioning(), "seat alignment completes")
	_expect_equal(boardroom.player.current_seat_id(), selected_seat.id, "player retains the occupied seat id")
	_expect_vector_near(
		boardroom.player.global_position,
		selected_seat.anchor,
		0.5,
		"player aligns to the exact chair anchor"
	)
	_expect_true(boardroom.player.collision_shape.disabled, "seated character collision is disabled")
	_expect_equal(boardroom.player.movement_state(), &"seated", "player movement state is seated")
	_expect_equal(boardroom.hud.movement_text(), "SEATED", "HUD clearly reports SEATED")
	_expect_equal(boardroom.hud.action_text(), "STAND", "occupied chair action becomes STAND")
	_expect_equal(
		boardroom.player.character_sprite.animation,
		selected_seat.animation,
		"chair direction selects the matching seated pose"
	)

	var seated_position := boardroom.player.global_position
	boardroom.player.set_move_target(Vector2(1280.0, 360.0))
	Input.action_press("move_right", 1.0)
	for _frame in range(8):
		await physics_frame
	Input.action_release("move_right")
	_expect_false(boardroom.player.has_active_navigation(), "seated tap navigation is rejected")
	_expect_vector_near(
		boardroom.player.global_position,
		seated_position,
		0.01,
		"seated manual input cannot move the character"
	)
	_parse_touch(9, joystick_center, true)
	_parse_drag(9, joystick_center + Vector2(64.0, 0.0), Vector2(64.0, 0.0))
	await process_frame
	_expect_true(
		boardroom.hud.virtual_joystick.output.x > 0.7,
		"virtual joystick still owns its pointer while seated"
	)
	for _frame in range(4):
		await physics_frame
	_expect_vector_near(
		boardroom.player.global_position,
		seated_position,
		0.01,
		"seated virtual joystick input cannot move the character"
	)
	_parse_touch(9, joystick_center + Vector2(64.0, 0.0), false)
	_parse_touch(10, Vector2(640.0, 360.0), true)
	_parse_touch(10, Vector2(640.0, 360.0), false)
	await process_frame
	await physics_frame
	_expect_false(boardroom.player.has_active_navigation(), "seated room touches cannot start navigation")
	_expect_false(
		boardroom.reserve_seat(selected_seat.id, &"remote_player"),
		"another player cannot claim the local occupied chair"
	)

	boardroom._interact()
	await process_frame
	_expect_true(boardroom.player.is_seat_transitioning(), "STAND begins a controlled exit transition")
	_expect_equal(boardroom.hud.action_text(), "STAND", "standing transition keeps the STAND action context")
	for _frame in range(20):
		await physics_frame
	await process_frame
	await physics_frame
	_expect_false(boardroom.player.is_seated(), "player leaves the seated state")
	_expect_false(boardroom.player.is_movement_locked(), "standing restores movement input")
	_expect_false(boardroom.player.collision_shape.disabled, "standing restores character collision")
	_expect_vector_near(
		boardroom.player.global_position,
		selected_seat.approach,
		0.5,
		"standing returns to the safe chair approach point"
	)
	_expect_equal(boardroom.seat_occupant(selected_seat.id), &"", "standing releases chair occupancy")
	_expect_equal(boardroom.hud.action_text(), "SIT", "released nearby chair returns to SIT")

	boardroom.queue_free()
	await process_frame


func _path_length(path: PackedVector2Array) -> float:
	var total := 0.0
	for index in range(1, path.size()):
		total += path[index - 1].distance_to(path[index])
	return total


func _parse_touch(index: int, position: Vector2, pressed: bool) -> void:
	var event := InputEventScreenTouch.new()
	event.index = index
	event.position = position
	event.pressed = pressed
	root.push_input(event, true)


func _parse_drag(index: int, position: Vector2, relative: Vector2) -> void:
	var event := InputEventScreenDrag.new()
	event.index = index
	event.position = position
	event.relative = relative
	root.push_input(event, true)


func _expect_true(value: bool, message: String) -> void:
	_checks += 1
	if not value:
		_failures.append(message)


func _expect_false(value: bool, message: String) -> void:
	_expect_true(not value, message)


func _expect_equal(actual: Variant, expected: Variant, message: String) -> void:
	_checks += 1
	if actual != expected:
		_failures.append("%s (expected %s, got %s)" % [message, str(expected), str(actual)])


func _expect_near(actual: float, expected: float, tolerance: float, message: String) -> void:
	_checks += 1
	if absf(actual - expected) > tolerance:
		_failures.append("%s (expected %.4f, got %.4f)" % [message, expected, actual])


func _expect_vector_near(actual: Vector2, expected: Vector2, tolerance: float, message: String) -> void:
	_checks += 1
	if actual.distance_to(expected) > tolerance:
		_failures.append("%s (expected %s, got %s)" % [message, str(expected), str(actual)])
