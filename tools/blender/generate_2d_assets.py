"""Regenerate the lightweight Blender-made 2D sprites used by Block Sort.

Run from the repository root:
    blender --background --python tools/blender/generate_2d_assets.py
"""

from pathlib import Path
import bpy
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Assets" / "_Project" / "Art" / "Generated"
OUT.mkdir(parents=True, exist_ok=True)

COLORS = {
    "red": "#E9414C",
    "amber": "#FFB517",
    "violet": "#A44AE3",
    "teal": "#25BBB4",
    "lime": "#8DCC36",
    "orange": "#F07B35",
}
ICONS = ["diamond", "star", "triangle", "coin", "heart", "moon"]


def rgb(hex_value):
    return tuple(int(hex_value[i : i + 2], 16) / 255 for i in (1, 3, 5)) + (1,)


def material(name, color, metallic=0.0, roughness=0.3):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    node = mat.node_tree.nodes.get("Principled BSDF")
    node.inputs["Base Color"].default_value = color
    node.inputs["Roughness"].default_value = roughness
    node.inputs["Metallic"].default_value = metallic
    return mat


def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.materials, bpy.data.meshes, bpy.data.curves):
        for item in datablocks:
            if item.users == 0:
                datablocks.remove(item)


def rounded_cube(name, color):
    bpy.ops.mesh.primitive_cube_add(location=(0, 0, 0.52))
    block = bpy.context.object
    block.name = name
    block.dimensions = (1, 0.68, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel = block.modifiers.new("Soft candy bevel", "BEVEL")
    bevel.width, bevel.segments, bevel.limit_method = 0.11, 3, "ANGLE"
    block.data.materials.append(material(f"Cube_{name}", color, roughness=0.23))
    bpy.context.view_layer.objects.active = block
    bpy.ops.object.shade_smooth()
    return block


def icon(name):
    """A thin cream icon plate made as a beveled 2D curve."""
    points = {
        "diamond": [(-.20, 0), (0, .25), (.20, 0), (0, -.25), (-.20, 0)],
        "triangle": [(-.23, -.19), (0, .23), (.23, -.19), (-.23, -.19)],
        "star": [(0, .25), (.06, .08), (.23, .08), (.10, -.03), (.15, -.22), (0, -.12), (-.15, -.22), (-.1, -.03), (-.23, .08), (-.06, .08), (0, .25)],
        "coin": [(0.25, 0), (.17, .17), (0, .25), (-.17, .17), (-.25, 0), (-.17, -.17), (0, -.25), (.17, -.17), (.25, 0)],
        "heart": [(0, -.22), (-.23, .02), (-.17, .20), (0, .08), (.17, .20), (.23, .02), (0, -.22)],
        "moon": [(.15, .24), (-.10, .22), (-.24, .06), (-.22, -.12), (-.06, -.24), (.12, -.16), (.02, -.10), (-.07, 0), (-.02, .12), (.15, .24)],
    }[name]
    curve = bpy.data.curves.new(f"{name}_curve", "CURVE")
    curve.dimensions, curve.resolution_u = "2D", 3
    spline = curve.splines.new("POLY")
    spline.points.add(len(points) - 1)
    for p, co in zip(spline.points, points):
        p.co = (co[0], co[1], 0, 1)
    curve.bevel_depth, curve.bevel_resolution, curve.resolution_u = .034, 2, 12
    obj = bpy.data.objects.new(f"{name}_icon", curve)
    bpy.context.collection.objects.link(obj)
    obj.rotation_euler = (1.5708, 0, 0)
    obj.location = (0, -0.355, .52)
    obj.data.materials.append(material("Icon_Cream", rgb("#F5E6C8"), roughness=.35))
    return obj


def wooden_slot():
    wood = material("Oak_slot", rgb("#9B532D"), roughness=.55)
    dark = material("Slot_interior", rgb("#3A1E16"), roughness=.7)
    bpy.ops.mesh.primitive_cube_add(location=(0, .05, 1.9))
    frame = bpy.context.object
    frame.name = "Wooden slot frame"
    frame.dimensions = (1.34, .45, 3.8)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel = frame.modifiers.new("Oak edge bevel", "BEVEL")
    bevel.width, bevel.segments = .12, 3
    frame.data.materials.append(wood)
    bpy.ops.mesh.primitive_cube_add(location=(0, -.21, 1.92))
    cavity = bpy.context.object
    cavity.name = "Dark tube cavity"
    cavity.dimensions = (.92, .07, 3.42)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel = cavity.modifiers.new("Cavity bevel", "BEVEL")
    bevel.width, bevel.segments = .09, 3
    cavity.data.materials.append(dark)
    bpy.ops.mesh.primitive_cube_add(location=(0, .03, .12))
    base = bpy.context.object
    base.dimensions = (1.52, .55, .25)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    base.data.materials.append(wood)


def ui_button():
    amber = material("Button amber", rgb("#FFB517"), roughness=.28)
    rim = material("Button rim", rgb("#7B361D"), roughness=.42)
    for radius, y, z, mat in [(0.58, 0.05, .12, rim), (.48, -.05, .18, amber)]:
        bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=16, location=(0, y, z))
        button = bpy.context.object
        button.scale = (1.45, .45, 1)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        button.data.materials.append(mat)


def set_camera(ortho_scale=5.1, target_height=1.7):
    """Frame each exported sprite tightly enough to use directly as a UI texture."""
    bpy.ops.object.camera_add(location=(0, -9, target_height))
    camera = bpy.context.object
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = ortho_scale
    camera.rotation_euler = (1.5708, 0, 0)
    bpy.context.scene.camera = camera
    bpy.ops.object.light_add(type="AREA", location=(-3, -4, 6))
    bpy.context.object.data.energy, bpy.context.object.data.shape = 850, "DISK"
    bpy.context.object.data.size = 5
    bpy.ops.object.light_add(type="AREA", location=(3, -3, 2))
    bpy.context.object.data.energy, bpy.context.object.data.color = 400, (1, .55, .3)
    # Blender 4.2 renamed EEVEE; 4.0 (available in the build image) uses the old id.
    engine_items = {item.identifier for item in bpy.types.RenderSettings.bl_rna.properties["engine"].enum_items}
    bpy.context.scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in engine_items else "BLENDER_EEVEE"
    bpy.context.scene.render.resolution_x = bpy.context.scene.render.resolution_y = 512
    bpy.context.scene.render.resolution_percentage = 100
    bpy.context.scene.render.image_settings.file_format = "PNG"
    bpy.context.scene.render.film_transparent = True
    bpy.context.scene.view_settings.look = "AgX - Medium High Contrast"


def render(name):
    bpy.context.scene.render.filepath = str(OUT / f"{name}.png")
    bpy.ops.render.render(write_still=True)


def export_active(name):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.context.scene.objects:
        if obj.type == "MESH" or obj.type == "CURVE":
            obj.select_set(True)
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    bpy.ops.export_scene.gltf(filepath=str(OUT / f"{name}.glb"), export_format="GLB", use_selection=True)


for color_name, hex_value in COLORS.items():
    clear()
    rounded_cube(color_name, rgb(hex_value))
    icon(ICONS[list(COLORS).index(color_name)])
    set_camera(1.35, .52)
    render(f"block_{color_name}")
    export_active(f"block_{color_name}")

clear()
wooden_slot()
set_camera(4.75, 1.9)
render("wooden_slot")
export_active("wooden_slot")

clear()
ui_button()
set_camera(1.9, .22)
render("ui_button")
export_active("ui_button")

print(f"Generated {len(COLORS)} blocks, a slot, and a UI button in {OUT}")
