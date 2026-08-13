class_name BoardroomCamera
extends Camera2D

signal mode_changed(mode: StringName)

const LOCKED := &"locked"
const FREE := &"free"

@export var target_path: NodePath
@export var room_bounds := BoardroomLayout.ROOM_RECT
@export var locked_look_ahead := Vector2(0.0, -400.0)

var mode: StringName = LOCKED
var _target: Node2D


func _ready() -> void:
	_target = get_node_or_null(target_path) as Node2D
	zoom = Vector2(0.7, 0.7)
	position_smoothing_enabled = true
	position_smoothing_speed = 7.5
	limit_left = int(room_bounds.position.x)
	limit_top = int(room_bounds.position.y)
	limit_right = int(room_bounds.end.x)
	limit_bottom = int(room_bounds.end.y)
	limit_smoothed = true
	get_viewport().size_changed.connect(_clamp_to_room)


func _process(_delta: float) -> void:
	if mode == LOCKED and is_instance_valid(_target):
		global_position = clamp_center(
			_target.global_position + locked_look_ahead,
			room_bounds,
			get_viewport_rect().size,
			zoom
		)


func set_mode(requested_mode: StringName) -> void:
	if requested_mode != LOCKED and requested_mode != FREE:
		return
	if mode == requested_mode:
		return
	mode = requested_mode
	position_smoothing_enabled = mode == LOCKED
	_clamp_to_room()
	mode_changed.emit(mode)


func pan_by_screen_delta(screen_delta: Vector2) -> void:
	if mode != FREE:
		return
	global_position -= screen_delta / zoom.x
	_clamp_to_room()


func _clamp_to_room() -> void:
	global_position = clamp_center(global_position, room_bounds, get_viewport_rect().size, zoom)


static func clamp_center(center: Vector2, bounds: Rect2, viewport_size: Vector2, camera_zoom: Vector2) -> Vector2:
	var safe_zoom := Vector2(maxf(camera_zoom.x, 0.01), maxf(camera_zoom.y, 0.01))
	var half_view := viewport_size * 0.5 / safe_zoom
	var minimum := bounds.position + half_view
	var maximum := bounds.end - half_view
	if minimum.x > maximum.x:
		minimum.x = bounds.get_center().x
		maximum.x = minimum.x
	if minimum.y > maximum.y:
		minimum.y = bounds.get_center().y
		maximum.y = minimum.y
	return center.clamp(minimum, maximum)
