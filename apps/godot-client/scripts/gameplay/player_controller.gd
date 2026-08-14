class_name PlayerController
extends CharacterBody2D

signal destination_changed(target: Vector2)
signal navigation_cancelled
signal navigation_completed
signal movement_state_changed(state: StringName)
signal seat_state_changed(seat_id: StringName, seated: bool)

const FRAME_ROOT := "res://assets/characters/featherfall-business/"
const FRAME_PATHS := {
	&"walk_left": ["walk-left-0.png", "walk-left-1.png", "walk-left-2.png"],
	&"walk_down": ["walk-down-0.png", "walk-down-1.png", "walk-down-2.png"],
	&"walk_up": ["walk-up-0.png", "walk-up-1.png", "walk-up-2.png"],
	&"walk_right": ["walk-right-0.png", "walk-right-1.png", "walk-right-2.png"],
}
const SEATED_FRAME_PATHS := {
	&"sit_left": "sit-left.png",
	&"sit_down": "sit-down.png",
	&"sit_up": "sit-up.png",
	&"sit_right": "sit-right.png",
}

@export var movement_speed := 260.0

@onready var navigation_agent: NavigationAgent2D = $NavigationAgent2D
@onready var character_sprite: AnimatedSprite2D = $AnimatedSprite2D
@onready var collision_shape: CollisionShape2D = $CollisionShape2D

var _virtual_joystick: Node
var _navigation_available := false
var _navigation_active := false
var _pending_target := Vector2.INF
var _last_facing: StringName = &"walk_down"
var _movement_state: StringName = &"ready"
var _seated := false
var _seat_transitioning := false
var _seat_id: StringName = &""
var _seat_animation: StringName = &"sit_down"
var _seat_walk_animation: StringName = &"walk_down"
var _seat_approach := Vector2.ZERO
var _seat_tween: Tween
var _standing_collision_layer := 2
var _standing_collision_mask := 1


func _ready() -> void:
	_build_sprite_frames()
	_update_animation(Vector2.ZERO)


func _physics_process(_delta: float) -> void:
	if is_movement_locked():
		velocity = Vector2.ZERO
		_update_animation(Vector2.ZERO)
		return

	var keyboard := Input.get_vector("move_left", "move_right", "move_up", "move_down")
	var touch := Vector2.ZERO
	if is_instance_valid(_virtual_joystick):
		touch = _virtual_joystick.output

	var manual := MovementInput.select_manual(keyboard, touch)
	var direction := Vector2.ZERO
	if not manual.is_zero_approx():
		if _navigation_active:
			cancel_navigation()
		direction = manual
		_set_movement_state(&"manual")
	elif _navigation_active:
		direction = _navigation_direction()
		_set_movement_state(&"path" if _navigation_active else &"ready")
	else:
		_set_movement_state(&"ready")

	velocity = direction * movement_speed
	move_and_slide()
	_update_animation(velocity)


func set_virtual_joystick(joystick: Node) -> void:
	_virtual_joystick = joystick


func set_navigation_available() -> void:
	_navigation_available = true
	if _pending_target != Vector2.INF:
		call_deferred("_activate_pending_target")


func set_move_target(world_target: Vector2) -> void:
	if is_movement_locked():
		return
	_pending_target = world_target
	if _navigation_available:
		_activate_pending_target()


func cancel_navigation() -> void:
	_pending_target = Vector2.INF
	if not _navigation_active:
		return
	_navigation_active = false
	navigation_agent.target_position = global_position
	navigation_cancelled.emit()


func has_active_navigation() -> bool:
	return _navigation_active


func sit_at(seat: Dictionary) -> bool:
	if is_movement_locked():
		return false
	var requested_id: StringName = seat.get("id", &"")
	var anchor: Vector2 = seat.get("anchor", Vector2.INF)
	var approach: Vector2 = seat.get("approach", Vector2.INF)
	var animation: StringName = seat.get("animation", &"")
	var walk_animation: StringName = seat.get("walk_animation", &"")
	if requested_id == &"" or anchor == Vector2.INF or approach == Vector2.INF:
		return false
	if not SEATED_FRAME_PATHS.has(animation) or not FRAME_PATHS.has(walk_animation):
		return false

	cancel_navigation()
	velocity = Vector2.ZERO
	_seat_id = requested_id
	_seat_animation = animation
	_seat_walk_animation = walk_animation
	_seat_approach = approach
	_seat_transitioning = true
	_standing_collision_layer = collision_layer
	_standing_collision_mask = collision_mask
	collision_layer = 0
	collision_mask = 0
	collision_shape.set_deferred("disabled", true)
	_set_movement_state(&"seating")
	_update_animation(Vector2.ZERO)

	_seat_tween = create_tween()
	_seat_tween.set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	_seat_tween.set_trans(Tween.TRANS_QUAD)
	_seat_tween.set_ease(Tween.EASE_IN_OUT)
	_seat_tween.tween_property(self, "global_position", anchor, 0.22)
	_seat_tween.finished.connect(_finish_sit, CONNECT_ONE_SHOT)
	return true


func stand_up() -> bool:
	if not _seated or _seat_transitioning:
		return false
	_seat_transitioning = true
	_set_movement_state(&"standing")
	_seat_tween = create_tween()
	_seat_tween.set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	_seat_tween.set_trans(Tween.TRANS_QUAD)
	_seat_tween.set_ease(Tween.EASE_IN_OUT)
	_seat_tween.tween_property(self, "global_position", _seat_approach, 0.18)
	_seat_tween.finished.connect(_finish_stand, CONNECT_ONE_SHOT)
	return true


func is_seated() -> bool:
	return _seated


func is_seat_transitioning() -> bool:
	return _seat_transitioning


func is_movement_locked() -> bool:
	return _seated or _seat_transitioning


func current_seat_id() -> StringName:
	return _seat_id


func movement_state() -> StringName:
	return _movement_state


func _activate_pending_target() -> void:
	if is_movement_locked():
		_pending_target = Vector2.INF
		return
	if _pending_target == Vector2.INF:
		return
	var navigation_map := navigation_agent.get_navigation_map()
	if not navigation_map.is_valid() or NavigationServer2D.map_get_iteration_id(navigation_map) == 0:
		call_deferred("_activate_pending_target")
		return

	var closest_target := NavigationServer2D.map_get_closest_point(navigation_map, _pending_target)
	_pending_target = Vector2.INF
	if global_position.distance_to(closest_target) <= navigation_agent.target_desired_distance:
		return
	navigation_agent.target_position = closest_target
	_navigation_active = true
	destination_changed.emit(closest_target)


func _navigation_direction() -> Vector2:
	var navigation_map := navigation_agent.get_navigation_map()
	if not navigation_map.is_valid() or NavigationServer2D.map_get_iteration_id(navigation_map) == 0:
		return Vector2.ZERO
	if navigation_agent.is_navigation_finished():
		_navigation_active = false
		navigation_completed.emit()
		return Vector2.ZERO
	var next_position := navigation_agent.get_next_path_position()
	return global_position.direction_to(next_position)


func _set_movement_state(state: StringName) -> void:
	if state == _movement_state:
		return
	_movement_state = state
	movement_state_changed.emit(state)


func _finish_sit() -> void:
	_seated = true
	_seat_transitioning = false
	_set_movement_state(&"seated")
	_update_animation(Vector2.ZERO)
	seat_state_changed.emit(_seat_id, true)


func _finish_stand() -> void:
	var released_seat := _seat_id
	_seated = false
	_seat_transitioning = false
	_seat_id = &""
	_last_facing = _seat_walk_animation
	collision_layer = _standing_collision_layer
	collision_mask = _standing_collision_mask
	collision_shape.set_deferred("disabled", false)
	_set_movement_state(&"ready")
	_update_animation(Vector2.ZERO)
	seat_state_changed.emit(released_seat, false)


func _build_sprite_frames() -> void:
	var frames := SpriteFrames.new()
	frames.remove_animation(&"default")
	for animation_name in FRAME_PATHS:
		frames.add_animation(animation_name)
		frames.set_animation_loop(animation_name, true)
		frames.set_animation_speed(animation_name, 8.0)
		for frame_file in FRAME_PATHS[animation_name]:
			frames.add_frame(animation_name, load(FRAME_ROOT + frame_file) as Texture2D)
	for animation_name in SEATED_FRAME_PATHS:
		frames.add_animation(animation_name)
		frames.set_animation_loop(animation_name, false)
		frames.set_animation_speed(animation_name, 1.0)
		frames.add_frame(
			animation_name,
			load(FRAME_ROOT + SEATED_FRAME_PATHS[animation_name]) as Texture2D
		)
	character_sprite.sprite_frames = frames


func _update_animation(movement: Vector2) -> void:
	if is_movement_locked():
		character_sprite.animation = _seat_animation
		character_sprite.frame = 0
		character_sprite.pause()
		return
	if movement.length_squared() > 4.0:
		_last_facing = MovementInput.animation_for(movement, _last_facing)
		if character_sprite.animation != _last_facing or not character_sprite.is_playing():
			character_sprite.play(_last_facing)
		return
	character_sprite.animation = _last_facing
	character_sprite.frame = 1
	character_sprite.pause()
