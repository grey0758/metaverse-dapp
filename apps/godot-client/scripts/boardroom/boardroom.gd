class_name Boardroom
extends Node2D

const LOCAL_OCCUPANT_ID := &"local_player"

@onready var navigation_region: BoardroomNavigation = $NavigationRegion2D
@onready var obstacles: Node2D = $Obstacles
@onready var player: PlayerController = $Player
@onready var boardroom_camera: BoardroomCamera = $BoardroomCamera
@onready var target_marker: MoveTargetMarker = $MoveTargetMarker
@onready var boardroom_art: BoardroomArt = $BoardroomArt
@onready var foreground: BoardroomForeground = $BoardroomForeground
@onready var hud: GameHud = $GameHud

var _pointer_index := -1
var _pointer_start := Vector2.ZERO
var _pointer_dragged := false
var _current_interaction: Dictionary = {}
var _seats: Array[Dictionary] = []
var _seat_lookup: Dictionary = {}
var _seat_occupants: Dictionary = {}


func _ready() -> void:
	player.global_position = BoardroomLayout.PLAYER_SPAWN
	_seats = BoardroomLayout.seats()
	for seat in _seats:
		_seat_lookup[seat.id] = seat
	_build_static_colliders()
	navigation_region.navigation_ready.connect(player.set_navigation_available)
	navigation_region.bake_layout()

	player.set_virtual_joystick(hud.virtual_joystick)
	player.destination_changed.connect(target_marker.show_target)
	player.navigation_cancelled.connect(target_marker.clear_target)
	player.navigation_completed.connect(target_marker.clear_target)
	player.movement_state_changed.connect(hud.set_movement_state)
	player.seat_state_changed.connect(_on_player_seat_state_changed)
	hud.camera_mode_requested.connect(_set_camera_mode)
	hud.interaction_requested.connect(_interact)
	_update_interaction()


func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("camera_lock"):
		_set_camera_mode(BoardroomCamera.LOCKED)
	if Input.is_action_just_pressed("camera_free"):
		_set_camera_mode(BoardroomCamera.FREE)
	if Input.is_action_just_pressed("interact"):
		_interact()
	_update_interaction()


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		_handle_screen_touch(event as InputEventScreenTouch)
	elif event is InputEventScreenDrag:
		_handle_screen_drag(event as InputEventScreenDrag)


func _handle_screen_touch(event: InputEventScreenTouch) -> void:
	if player.is_movement_locked():
		if event.index == _pointer_index:
			_pointer_index = -1
			_pointer_dragged = false
		return
	if event.pressed:
		if _pointer_index == -1:
			_pointer_index = event.index
			_pointer_start = event.position
			_pointer_dragged = false
		return
	if event.index != _pointer_index:
		return
	if not _pointer_dragged and not event.canceled:
		player.set_move_target(_screen_to_world(event.position))
	_pointer_index = -1
	_pointer_dragged = false


func _handle_screen_drag(event: InputEventScreenDrag) -> void:
	if player.is_movement_locked():
		return
	if event.index != _pointer_index:
		return
	if event.position.distance_to(_pointer_start) >= 14.0:
		_pointer_dragged = true
	if _pointer_dragged and boardroom_camera.mode == BoardroomCamera.FREE:
		boardroom_camera.pan_by_screen_delta(event.relative)


func _screen_to_world(screen_position: Vector2) -> Vector2:
	return get_viewport().get_canvas_transform().affine_inverse() * screen_position


func _set_camera_mode(mode: StringName) -> void:
	boardroom_camera.set_mode(mode)
	hud.set_camera_mode(mode)
	if mode == BoardroomCamera.LOCKED:
		hud.show_toast("CAMERA LOCKED")
	else:
		hud.show_toast("FREE CAMERA")


func _update_interaction() -> void:
	if player.is_seat_transitioning():
		if player.movement_state() == &"standing":
			_set_current_interaction({}, "LEAVING CHAIR", "STAND")
		else:
			_set_current_interaction({}, "TAKING SEAT", "SIT")
		return
	if player.is_seated():
		var current_seat: Dictionary = _seat_lookup.get(player.current_seat_id(), {})
		if current_seat.is_empty():
			_set_current_interaction({})
			return
		var stand_interaction := current_seat.duplicate()
		stand_interaction["kind"] = &"seat"
		stand_interaction["action"] = "STAND"
		_set_current_interaction(stand_interaction)
		return

	var nearest: Dictionary = {}
	var nearest_distance := INF
	for interaction in BoardroomLayout.interaction_points():
		var distance := player.global_position.distance_to(interaction.position)
		if distance <= interaction.radius and distance < nearest_distance:
			nearest = interaction.duplicate()
			nearest["kind"] = &"facility"
			nearest["action"] = "USE"
			nearest_distance = distance
	for seat in _seats:
		if seat_occupant(seat.id) != &"":
			continue
		var distance := player.global_position.distance_to(seat.approach)
		if distance <= seat.radius and distance < nearest_distance:
			nearest = seat.duplicate()
			nearest["kind"] = &"seat"
			nearest["action"] = "SIT"
			nearest_distance = distance
	_set_current_interaction(nearest)


func _set_current_interaction(
	interaction: Dictionary,
	disabled_context: String = "NO ACTION",
	disabled_action: String = "USE"
) -> void:
	var previous_id: StringName = _current_interaction.get("id", &"")
	var previous_action: String = _current_interaction.get("action", "")
	var current_id: StringName = interaction.get("id", &"")
	var current_action: String = interaction.get("action", disabled_action)
	if previous_id == current_id and previous_action == current_action:
		return
	_current_interaction = interaction
	if interaction.is_empty():
		hud.set_interaction(disabled_context, false, disabled_action)
	else:
		hud.set_interaction(interaction.get("label", ""), true, current_action)


func _interact() -> void:
	if player.is_seat_transitioning():
		return
	if player.is_seated():
		if player.stand_up():
			_set_current_interaction({}, "LEAVING CHAIR", "STAND")
		return
	if _current_interaction.is_empty():
		return
	if _current_interaction.get("kind", &"") == &"seat":
		_sit_at(_current_interaction)
		return
	match _current_interaction.id:
		&"screen":
			var screen_active := boardroom_art.toggle_screen()
			hud.show_toast("PRESENTATION ON" if screen_active else "PRESENTATION STANDBY")
		&"lectern":
			hud.show_toast("LECTERN RESERVED")
		&"exit":
			hud.show_toast("LOBBY CONNECTION PENDING")


func reserve_seat(seat_id: StringName, occupant_id: StringName) -> bool:
	if occupant_id == &"" or not _seat_lookup.has(seat_id):
		return false
	var current: StringName = _seat_occupants.get(seat_id, &"")
	if current != &"" and current != occupant_id:
		return false
	_seat_occupants[seat_id] = occupant_id
	return true


func release_seat(seat_id: StringName, occupant_id: StringName) -> bool:
	if _seat_occupants.get(seat_id, &"") != occupant_id:
		return false
	_seat_occupants.erase(seat_id)
	return true


func seat_occupant(seat_id: StringName) -> StringName:
	return _seat_occupants.get(seat_id, &"")


func seat_count() -> int:
	return _seats.size()


func _sit_at(seat: Dictionary) -> void:
	var seat_id: StringName = seat.get("id", &"")
	if not reserve_seat(seat_id, LOCAL_OCCUPANT_ID):
		hud.show_toast("CHAIR OCCUPIED")
		_update_interaction()
		return
	if not player.sit_at(seat):
		release_seat(seat_id, LOCAL_OCCUPANT_ID)
		return
	foreground.set_active_seat(seat_id)
	target_marker.clear_target()
	_set_current_interaction({}, "TAKING SEAT", "SIT")


func _on_player_seat_state_changed(seat_id: StringName, seated: bool) -> void:
	if seated:
		hud.show_toast("SEATED")
	else:
		release_seat(seat_id, LOCAL_OCCUPANT_ID)
		foreground.set_active_seat(&"")
		hud.show_toast("STANDING CLEAR")
	_update_interaction()


func _build_static_colliders() -> void:
	for index in BoardroomLayout.physics_obstacles().size():
		var rect: Rect2 = BoardroomLayout.physics_obstacles()[index]
		_add_rect_collider(rect, "Obstacle%02d" % index)
	_add_walkable_boundary()


func _add_rect_collider(rect: Rect2, body_name: String) -> void:
	var body := StaticBody2D.new()
	body.name = body_name
	body.position = rect.get_center()
	body.collision_layer = 1
	body.collision_mask = 0

	var shape := RectangleShape2D.new()
	shape.size = rect.size
	var collision := CollisionShape2D.new()
	collision.shape = shape
	body.add_child(collision)
	obstacles.add_child(body)


func _add_walkable_boundary() -> void:
	var outline := BoardroomLayout.walkable_outline()
	var segments := PackedVector2Array()
	for index in outline.size():
		segments.append(outline[index])
		segments.append(outline[(index + 1) % outline.size()])

	var body := StaticBody2D.new()
	body.name = "WalkableBoundary"
	body.collision_layer = 1
	body.collision_mask = 0
	var shape := ConcavePolygonShape2D.new()
	shape.segments = segments
	var collision := CollisionShape2D.new()
	collision.shape = shape
	body.add_child(collision)
	obstacles.add_child(body)
