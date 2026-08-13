class_name PlayerMarker
extends Node2D


func _ready() -> void:
	var label := Label.new()
	label.text = "LOCAL PLAYER"
	label.position = Vector2(-70.0, -70.0)
	label.size = Vector2(140.0, 24.0)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	label.add_theme_font_size_override("font_size", 13)
	label.add_theme_color_override("font_color", Color("e9ffff"))
	label.add_theme_color_override("font_outline_color", Color(0.02, 0.05, 0.06, 0.95))
	label.add_theme_constant_override("outline_size", 5)
	label.z_index = 3
	add_child(label)
	queue_redraw()


func _draw() -> void:
	draw_set_transform(Vector2(0.0, 20.0), 0.0, Vector2(1.35, 0.48))
	draw_circle(Vector2.ZERO, 27.0, Color(0.0, 0.0, 0.0, 0.32))
	draw_set_transform(Vector2.ZERO)
	draw_arc(Vector2(0.0, 20.0), 31.0, 0.0, TAU, 40, Color("4fd2dc"), 3.0, true)
	draw_colored_polygon(PackedVector2Array([
		Vector2(0.0, -46.0),
		Vector2(-8.0, -33.0),
		Vector2(8.0, -33.0),
	]), Color("4fd2dc"))
