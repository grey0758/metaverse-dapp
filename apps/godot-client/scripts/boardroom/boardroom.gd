class_name Boardroom
extends Node2D

@onready var navigation_region: BoardroomNavigation = $NavigationRegion2D
@onready var obstacles: Node2D = $Obstacles
@onready var player: PlayerController = $Player
@onready var boardroom_camera: BoardroomCamera = $BoardroomCamera
@onready var target_marker: MoveTargetMarker = $MoveTargetMarker
@onready var boardroom_art: BoardroomArt = $BoardroomArt
@onready var hud: GameHud = $GameHud

var _pointer_index := -1
var _pointer_start := Vector2.ZERO
var _pointer_dragged := false
var _current_interaction: Dictionary = {}


func _ready() -> void:
	player.global_position = BoardroomLayout.PLAYER_SPAWN
	_build_static_colliders()
	navigation_region.navigation_ready.connect(player.set_navigation_available)
	navigation_region.bake_layout()

	player.set_virtual_joystick(hud.virtual_joystick)
	player.destination_changed.connect(target_marker.show_target)
	player.navigation_cancelled.connect(target_marker.clear_target)
	player.navigation_completed.connect(target_marker.clear_target)
	player.movement_state_changed.connect(hud.set_movement_state)
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
	var nearest: Dictionary = {}
	var nearest_distance := INF
	for interaction in BoardroomLayout.interaction_points():
		var distance := player.global_position.distance_to(interaction.position)
		if distance <= interaction.radius and distance < nearest_distance:
			nearest = interaction
			nearest_distance = distance

	var previous_id: StringName = _current_interaction.get("id", &"")
	var current_id: StringName = nearest.get("id", &"")
	if previous_id == current_id:
		return
	_current_interaction = nearest
	hud.set_interaction(nearest.get("label", ""), not nearest.is_empty())


func _interact() -> void:
	if _current_interaction.is_empty():
		return
	match _current_interaction.id:
		&"screen":
			var screen_active := boardroom_art.toggle_screen()
			hud.show_toast("PRESENTATION ON" if screen_active else "PRESENTATION STANDBY")
		&"lectern":
			hud.show_toast("LECTERN RESERVED")
		&"exit":
			hud.show_toast("LOBBY CONNECTION PENDING")


func _build_static_colliders() -> void:
	for index in BoardroomLayout.physics_obstacles().size():
		var rect: Rect2 = BoardroomLayout.physics_obstacles()[index]
		var body := StaticBody2D.new()
		body.name = "Obstacle%02d" % index
		body.position = rect.get_center()
		body.collision_layer = 1
		body.collision_mask = 0

		var shape := RectangleShape2D.new()
		shape.size = rect.size
		var collision := CollisionShape2D.new()
		collision.shape = shape
		body.add_child(collision)
		obstacles.add_child(body)
