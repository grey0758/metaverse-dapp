class_name MoveTargetMarker
extends Node2D

var _phase := 0.0


func _ready() -> void:
	hide()


func _process(delta: float) -> void:
	if not visible:
		return
	_phase = fmod(_phase + delta * 3.5, TAU)
	queue_redraw()


func show_target(target: Vector2) -> void:
	global_position = target
	_phase = 0.0
	show()
	queue_redraw()


func clear_target() -> void:
	hide()


func _draw() -> void:
	var radius := 24.0 + (sin(_phase) + 1.0) * 5.0
	draw_circle(Vector2.ZERO, 7.0, Color("4fd2dc"))
	draw_arc(Vector2.ZERO, radius, 0.0, TAU, 32, Color(0.31, 0.82, 0.86, 0.8), 3.0, true)
	draw_line(Vector2(-12.0, 0.0), Vector2(12.0, 0.0), Color("e9ffff"), 2.0)
	draw_line(Vector2(0.0, -12.0), Vector2(0.0, 12.0), Color("e9ffff"), 2.0)
