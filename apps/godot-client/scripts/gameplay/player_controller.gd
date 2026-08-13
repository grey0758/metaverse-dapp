class_name PlayerController
extends CharacterBody2D

signal destination_changed(target: Vector2)
signal navigation_cancelled
signal navigation_completed
signal movement_state_changed(state: StringName)

const FRAME_ROOT := "res://assets/characters/kenney-rpg-urban/"
const FRAME_PATHS := {
	&"walk_left": ["walk-left-0.png", "walk-left-1.png", "walk-left-2.png"],
	&"walk_down": ["walk-down-0.png", "walk-down-1.png", "walk-down-2.png"],
	&"walk_up": ["walk-up-0.png", "walk-up-1.png", "walk-up-2.png"],
	&"walk_right": ["walk-right-0.png", "walk-right-1.png", "walk-right-2.png"],
}

@export var movement_speed := 260.0

@onready var navigation_agent: NavigationAgent2D = $NavigationAgent2D
@onready var character_sprite: AnimatedSprite2D = $AnimatedSprite2D

var _virtual_joystick: Node
var _navigation_available := false
var _navigation_active := false
var _pending_target := Vector2.INF
var _last_facing: StringName = &"walk_down"
var _movement_state: StringName = &"ready"


func _ready() -> void:
	_build_sprite_frames()
	_update_animation(Vector2.ZERO)


func _physics_process(_delta: float) -> void:
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


func _activate_pending_target() -> void:
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


func _build_sprite_frames() -> void:
	var frames := SpriteFrames.new()
	frames.remove_animation(&"default")
	for animation_name in FRAME_PATHS:
		frames.add_animation(animation_name)
		frames.set_animation_loop(animation_name, true)
		frames.set_animation_speed(animation_name, 8.0)
		for frame_file in FRAME_PATHS[animation_name]:
			frames.add_frame(animation_name, load(FRAME_ROOT + frame_file) as Texture2D)
	character_sprite.sprite_frames = frames


func _update_animation(movement: Vector2) -> void:
	if movement.length_squared() > 4.0:
		_last_facing = MovementInput.animation_for(movement, _last_facing)
		if character_sprite.animation != _last_facing or not character_sprite.is_playing():
			character_sprite.play(_last_facing)
		return
	character_sprite.animation = _last_facing
	character_sprite.frame = 1
	character_sprite.pause()
