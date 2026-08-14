class_name GameHud
extends CanvasLayer

signal camera_mode_requested(mode: StringName)
signal interaction_requested

const CYAN := Color("4fd2dc")
const CYAN_DARK := Color("1a6b74")
const INK := Color("10191f")
const PANEL := Color(0.035, 0.065, 0.08, 0.93)
const PANEL_LIGHT := Color(0.08, 0.13, 0.15, 0.95)
const MUTED := Color("9bb0b6")
const TEXT := Color("eef6f5")
const GOLD := Color("d6b66c")

var virtual_joystick: FeatherfallVirtualJoystick

var _safe_root: Control
var _header: PanelContainer
var _identity: VBoxContainer
var _room_state: VBoxContainer
var _movement_box: VBoxContainer
var _movement_title: Label
var _movement_label: Label
var _interaction_label: Label
var _action_button: Button
var _lock_button: Button
var _free_button: Button
var _toast_panel: PanelContainer
var _toast_label: Label
var _toast_timer: Timer


func _ready() -> void:
	layer = 20
	_build_safe_root()
	_build_header()
	_build_joystick()
	_build_action_area()
	_build_toast()
	get_viewport().size_changed.connect(_apply_safe_area)
	_apply_safe_area()
	set_camera_mode(BoardroomCamera.LOCKED)
	set_interaction("", false)


func set_camera_mode(mode: StringName) -> void:
	var locked := mode == BoardroomCamera.LOCKED
	_lock_button.button_pressed = locked
	_free_button.button_pressed = not locked
	_lock_button.add_theme_stylebox_override("normal", _mode_style(locked))
	_free_button.add_theme_stylebox_override("normal", _mode_style(not locked))


func set_movement_state(state: StringName) -> void:
	match state:
		&"manual":
			_movement_label.text = "DIRECT"
			_movement_label.add_theme_color_override("font_color", CYAN)
		&"path":
			_movement_label.text = "ROUTE"
			_movement_label.add_theme_color_override("font_color", GOLD)
		&"seating", &"standing":
			_movement_label.text = "SEATING" if state == &"seating" else "STANDING"
			_movement_label.add_theme_color_override("font_color", GOLD)
		&"seated":
			_movement_label.text = "SEATED"
			_movement_label.add_theme_color_override("font_color", Color("45d49b"))
		_:
			_movement_label.text = "READY"
			_movement_label.add_theme_color_override("font_color", TEXT)


func set_interaction(context: String, available: bool, action_text: String = "USE") -> void:
	_interaction_label.text = context if available else "NO ACTION"
	if not available and context != "":
		_interaction_label.text = context
	_action_button.text = action_text
	_action_button.disabled = not available
	_action_button.modulate = Color.WHITE if available else Color(0.6, 0.64, 0.65, 0.72)


func action_text() -> String:
	return _action_button.text


func interaction_available() -> bool:
	return not _action_button.disabled


func movement_text() -> String:
	return _movement_label.text


func show_toast(message: String) -> void:
	_toast_label.text = message
	_toast_panel.show()
	_toast_timer.start()


func _build_safe_root() -> void:
	_safe_root = Control.new()
	_safe_root.name = "SafeArea"
	_safe_root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_safe_root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_safe_root)


func _build_header() -> void:
	_header = PanelContainer.new()
	_header.name = "Header"
	_header.set_anchors_preset(Control.PRESET_TOP_WIDE)
	_header.offset_left = 18.0
	_header.offset_top = 16.0
	_header.offset_right = -18.0
	_header.offset_bottom = 82.0
	_header.add_theme_stylebox_override("panel", _panel_style(PANEL, Color(0.24, 0.48, 0.52, 0.65), 1))
	_safe_root.add_child(_header)

	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 14)
	_header.add_child(row)

	_identity = VBoxContainer.new()
	_identity.custom_minimum_size = Vector2(230.0, 0.0)
	_identity.add_theme_constant_override("separation", 0)
	row.add_child(_identity)

	var brand := Label.new()
	brand.text = "FEATHERFALL"
	brand.add_theme_font_size_override("font_size", 21)
	brand.add_theme_color_override("font_color", TEXT)
	_identity.add_child(brand)

	var location := Label.new()
	location.text = "PLATO BOARDROOM"
	location.add_theme_font_size_override("font_size", 12)
	location.add_theme_color_override("font_color", CYAN)
	_identity.add_child(location)

	var separator := VSeparator.new()
	separator.modulate = Color(0.35, 0.66, 0.68, 0.42)
	row.add_child(separator)

	_room_state = VBoxContainer.new()
	_room_state.custom_minimum_size = Vector2(130.0, 0.0)
	_room_state.add_theme_constant_override("separation", 0)
	row.add_child(_room_state)

	var session_label := Label.new()
	session_label.text = "PRIVATE ROOM"
	session_label.add_theme_font_size_override("font_size", 11)
	session_label.add_theme_color_override("font_color", MUTED)
	_room_state.add_child(session_label)

	var live_row := HBoxContainer.new()
	live_row.add_theme_constant_override("separation", 7)
	_room_state.add_child(live_row)
	var live_dot := ColorRect.new()
	live_dot.color = Color("45d49b")
	live_dot.custom_minimum_size = Vector2(7.0, 7.0)
	live_row.add_child(live_dot)
	var live_label := Label.new()
	live_label.text = "LOCAL"
	live_label.add_theme_font_size_override("font_size", 13)
	live_label.add_theme_color_override("font_color", TEXT)
	live_row.add_child(live_label)

	var spacer := Control.new()
	spacer.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	spacer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	row.add_child(spacer)

	_movement_box = VBoxContainer.new()
	_movement_box.custom_minimum_size = Vector2(112.0, 0.0)
	_movement_box.add_theme_constant_override("separation", 0)
	row.add_child(_movement_box)
	_movement_title = Label.new()
	_movement_title.text = "MOVEMENT"
	_movement_title.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	_movement_title.add_theme_font_size_override("font_size", 10)
	_movement_title.add_theme_color_override("font_color", MUTED)
	_movement_box.add_child(_movement_title)
	_movement_label = Label.new()
	_movement_label.text = "READY"
	_movement_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	_movement_label.add_theme_font_size_override("font_size", 15)
	_movement_label.add_theme_color_override("font_color", TEXT)
	_movement_box.add_child(_movement_label)

	var modes := HBoxContainer.new()
	modes.custom_minimum_size = Vector2(184.0, 44.0)
	modes.add_theme_constant_override("separation", 4)
	row.add_child(modes)
	_lock_button = _make_mode_button("LOCK")
	_free_button = _make_mode_button("FREE")
	modes.add_child(_lock_button)
	modes.add_child(_free_button)
	_lock_button.pressed.connect(func() -> void: camera_mode_requested.emit(BoardroomCamera.LOCKED))
	_free_button.pressed.connect(func() -> void: camera_mode_requested.emit(BoardroomCamera.FREE))


func _build_joystick() -> void:
	var joystick_scene := preload("res://addons/virtual_joystick/virtual_joystick_scene.tscn")
	virtual_joystick = joystick_scene.instantiate() as FeatherfallVirtualJoystick
	virtual_joystick.name = "MoveJoystick"
	virtual_joystick.joystick_mode = FeatherfallVirtualJoystick.Joystick_mode.FIXED
	virtual_joystick.visibility_mode = FeatherfallVirtualJoystick.Visibility_mode.ALWAYS
	virtual_joystick.use_input_actions = false
	virtual_joystick.deadzone_size = 18.0
	virtual_joystick.clampzone_size = 78.0
	virtual_joystick.pressed_color = Color("7be8ef")
	virtual_joystick.set_anchors_preset(Control.PRESET_BOTTOM_LEFT)
	virtual_joystick.offset_left = 10.0
	virtual_joystick.offset_top = -310.0
	virtual_joystick.offset_right = 310.0
	virtual_joystick.offset_bottom = -10.0
	_safe_root.add_child(virtual_joystick)
	_set_controls_to_ignore_mouse(virtual_joystick)
	virtual_joystick.get_node("Base").modulate = Color(0.27, 0.74, 0.78, 0.82)
	virtual_joystick.get_node("Base/Tip").modulate = Color(0.92, 1.0, 1.0, 0.96)


func _build_action_area() -> void:
	var action_stack := VBoxContainer.new()
	action_stack.name = "ActionArea"
	action_stack.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT)
	action_stack.offset_left = -172.0
	action_stack.offset_top = -178.0
	action_stack.offset_right = -22.0
	action_stack.offset_bottom = -22.0
	action_stack.alignment = BoxContainer.ALIGNMENT_END
	action_stack.add_theme_constant_override("separation", 6)
	_safe_root.add_child(action_stack)

	_interaction_label = Label.new()
	_interaction_label.text = "NO ACTION"
	_interaction_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_interaction_label.add_theme_font_size_override("font_size", 11)
	_interaction_label.add_theme_color_override("font_color", MUTED)
	action_stack.add_child(_interaction_label)

	_action_button = Button.new()
	_action_button.text = "USE"
	_action_button.custom_minimum_size = Vector2(150.0, 92.0)
	_action_button.focus_mode = Control.FOCUS_NONE
	_action_button.add_theme_font_size_override("font_size", 22)
	_action_button.add_theme_color_override("font_color", TEXT)
	_action_button.add_theme_color_override("font_pressed_color", TEXT)
	_action_button.add_theme_color_override("font_disabled_color", MUTED)
	_action_button.add_theme_stylebox_override("normal", _panel_style(CYAN_DARK, CYAN, 2))
	_action_button.add_theme_stylebox_override("hover", _panel_style(Color("247d86"), Color("93f3f6"), 2))
	_action_button.add_theme_stylebox_override("pressed", _panel_style(Color("144e55"), TEXT, 2))
	_action_button.add_theme_stylebox_override("disabled", _panel_style(Color(0.035, 0.055, 0.065, 0.94), Color(0.19, 0.27, 0.29, 0.92), 1))
	_action_button.pressed.connect(func() -> void: interaction_requested.emit())
	action_stack.add_child(_action_button)


func _build_toast() -> void:
	_toast_panel = PanelContainer.new()
	_toast_panel.name = "Toast"
	_toast_panel.set_anchors_preset(Control.PRESET_CENTER_TOP)
	_toast_panel.offset_left = -170.0
	_toast_panel.offset_top = 104.0
	_toast_panel.offset_right = 170.0
	_toast_panel.offset_bottom = 150.0
	_toast_panel.add_theme_stylebox_override("panel", _panel_style(PANEL_LIGHT, CYAN, 1))
	_safe_root.add_child(_toast_panel)

	_toast_label = Label.new()
	_toast_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	_toast_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	_toast_label.add_theme_font_size_override("font_size", 14)
	_toast_label.add_theme_color_override("font_color", TEXT)
	_toast_panel.add_child(_toast_label)
	_toast_panel.hide()

	_toast_timer = Timer.new()
	_toast_timer.one_shot = true
	_toast_timer.wait_time = 2.2
	_toast_timer.timeout.connect(_toast_panel.hide)
	add_child(_toast_timer)


func _make_mode_button(label_text: String) -> Button:
	var button := Button.new()
	button.text = label_text
	button.toggle_mode = true
	button.focus_mode = Control.FOCUS_NONE
	button.custom_minimum_size = Vector2(88.0, 44.0)
	button.add_theme_font_size_override("font_size", 13)
	button.add_theme_color_override("font_color", MUTED)
	button.add_theme_color_override("font_pressed_color", TEXT)
	button.add_theme_stylebox_override("hover", _panel_style(PANEL_LIGHT, Color(0.4, 0.68, 0.7, 0.7), 1))
	button.add_theme_stylebox_override("pressed", _mode_style(true))
	return button


func _mode_style(active: bool) -> StyleBoxFlat:
	return _panel_style(
		Color("1b6670") if active else Color(0.025, 0.045, 0.055, 0.8),
		CYAN if active else Color(0.26, 0.4, 0.43, 0.8),
		2 if active else 1
	)


func _panel_style(background: Color, border: Color, border_width: int) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = background
	style.border_color = border
	style.set_border_width_all(border_width)
	style.set_corner_radius_all(6)
	style.content_margin_left = 14.0
	style.content_margin_right = 14.0
	style.content_margin_top = 8.0
	style.content_margin_bottom = 8.0
	return style


func _set_controls_to_ignore_mouse(node: Node) -> void:
	if node is Control:
		(node as Control).mouse_filter = Control.MOUSE_FILTER_IGNORE
	for child in node.get_children():
		_set_controls_to_ignore_mouse(child)


func _apply_safe_area() -> void:
	if not is_instance_valid(_safe_root):
		return
	var viewport_size := get_viewport().get_visible_rect().size
	var screen_size := Vector2(DisplayServer.screen_get_size())
	var safe_area := Rect2(DisplayServer.get_display_safe_area())
	if screen_size.x <= 0.0 or screen_size.y <= 0.0 or safe_area.size.x <= 0.0 or safe_area.size.y <= 0.0:
		_safe_root.offset_left = 0.0
		_safe_root.offset_top = 0.0
		_safe_root.offset_right = 0.0
		_safe_root.offset_bottom = 0.0
	else:
		_safe_root.offset_left = viewport_size.x * safe_area.position.x / screen_size.x
		_safe_root.offset_top = viewport_size.y * safe_area.position.y / screen_size.y
		_safe_root.offset_right = -viewport_size.x * (screen_size.x - safe_area.end.x) / screen_size.x
		_safe_root.offset_bottom = -viewport_size.y * (screen_size.y - safe_area.end.y) / screen_size.y
	_apply_responsive_layout()


func _apply_responsive_layout() -> void:
	if not is_instance_valid(_header):
		return
	var compact := DisplayServer.window_get_size().x < 1100
	_room_state.visible = not compact
	_movement_title.visible = not compact
	_identity.custom_minimum_size.x = 184.0 if compact else 230.0
	_movement_box.custom_minimum_size.x = 72.0 if compact else 112.0
	_header.offset_left = 12.0 if compact else 18.0
	_header.offset_right = -12.0 if compact else -18.0
