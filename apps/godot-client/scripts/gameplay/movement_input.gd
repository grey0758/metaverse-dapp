class_name MovementInput
extends RefCounted

const MANUAL_DEADZONE := 0.08


static func select_manual(keyboard: Vector2, touch: Vector2) -> Vector2:
	var selected := touch if touch.length_squared() >= keyboard.length_squared() else keyboard
	if selected.length() <= MANUAL_DEADZONE:
		return Vector2.ZERO
	return selected.limit_length(1.0)


static func animation_for(direction: Vector2, fallback: StringName = &"walk_down") -> StringName:
	if direction.length_squared() <= 0.0001:
		return fallback
	if absf(direction.x) > absf(direction.y):
		return &"walk_right" if direction.x > 0.0 else &"walk_left"
	return &"walk_down" if direction.y > 0.0 else &"walk_up"
