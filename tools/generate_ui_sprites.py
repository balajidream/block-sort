"""Generate flat 2D UI sprites for Block Sort (tubes, candy, buttons).

    python3 tools/generate_ui_sprites.py
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import math

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Assets" / "_Project" / "Resources" / "Art"


def rgb(h, a=255):
    h = h.lstrip("#")
    return tuple(int(h[i : i + 2], 16) for i in (0, 2, 4)) + (a,)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(4))


def rounded_mask(size, radius):
    img = Image.new("L", size, 0)
    ImageDraw.Draw(img).rounded_rectangle((1, 1, size[0] - 2, size[1] - 2), radius=radius, fill=255)
    return img


def vertical_gradient(size, top, bottom, radius):
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    pix = img.load()
    for y in range(size[1]):
        c = lerp(top, bottom, y / max(1, size[1] - 1))
        for x in range(size[0]):
            pix[x, y] = c
    img.putalpha(rounded_mask(size, radius))
    return img


def highlight(size, radius):
    h = Image.new("RGBA", size, (0, 0, 0, 0))
    ImageDraw.Draw(h).rounded_rectangle((8, 6, size[0] - 8, int(size[1] * 0.38)), radius=radius, fill=(255, 255, 255, 55))
    return h


def write_tube():
    w, h = 256, 720
    tube = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    outer = vertical_gradient((w, h), rgb("#C48A58"), rgb("#6B3A1F"), 48)
    well = vertical_gradient((w - 48, h - 90), rgb("#4A2414"), rgb("#2A120A"), 32)
    foot = Image.new("RGBA", (w - 20, 48), (0, 0, 0, 0))
    ImageDraw.Draw(foot).rounded_rectangle((0, 0, w - 21, 47), 16, fill=rgb("#E0A06A"))
    rim = Image.new("RGBA", (w - 36, 28), (0, 0, 0, 0))
    ImageDraw.Draw(rim).rounded_rectangle((0, 0, w - 37, 27), 12, fill=rgb("#E8B888"))
    tube.alpha_composite(outer, (0, 0))
    tube.alpha_composite(well, (24, 28))
    tube.alpha_composite(rim, (18, 14))
    tube.alpha_composite(foot, (10, h - 52))
    shine = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    ImageDraw.Draw(shine).rounded_rectangle((30, 36, 70, h - 70), 18, fill=(255, 255, 255, 28))
    tube.alpha_composite(shine)
    tube.save(OUT / "wooden_slot.png")


def write_button():
    img = vertical_gradient((256, 128), rgb("#FF8A3D"), rgb("#D94A12"), 36)
    img = Image.alpha_composite(img, highlight((256, 128), 28))
    ImageDraw.Draw(img).rounded_rectangle((2, 2, 253, 125), 36, outline=(255, 210, 140, 180), width=3)
    img.save(OUT / "ui_button.png")


def write_panel():
    vertical_gradient((256, 256), rgb("#5A2E1C"), rgb("#2B140C"), 40).save(OUT / "ui_panel.png")


def icon_diamond(d, cx, cy, s, fill):
    d.polygon([(cx, cy - s), (cx + s, cy), (cx, cy + s), (cx - s, cy)], fill=fill)


def icon_star(d, cx, cy, s, fill):
    pts = []
    for i in range(10):
        ang = -math.pi / 2 + i * math.pi / 5
        r = s if i % 2 == 0 else s * 0.42
        pts.append((cx + r * math.cos(ang), cy + r * math.sin(ang)))
    d.polygon(pts, fill=fill)


def icon_triangle(d, cx, cy, s, fill):
    d.polygon([(cx, cy - s), (cx + s, cy + s * 0.7), (cx - s, cy + s * 0.7)], fill=fill)


def icon_coin(d, cx, cy, s, fill):
    d.ellipse((cx - s, cy - s, cx + s, cy + s), outline=fill, width=8)


def icon_heart(d, cx, cy, s, fill):
    d.ellipse((cx - s, cy - s * 0.6, cx, cy + s * 0.4), fill=fill)
    d.ellipse((cx, cy - s * 0.6, cx + s, cy + s * 0.4), fill=fill)
    d.polygon([(cx - s * 0.95, cy), (cx + s * 0.95, cy), (cx, cy + s)], fill=fill)


ICONS = {
    "diamond": icon_diamond,
    "star": icon_star,
    "triangle": icon_triangle,
    "coin": icon_coin,
    "heart": icon_heart,
}

BLOCKS = {
    "red": ("#E9414C", "diamond"),
    "amber": ("#FFB517", "star"),
    "violet": ("#A44AE3", "triangle"),
    "teal": ("#25BBB4", "coin"),
    "lime": ("#8DCC36", "diamond"),
    "orange": ("#F07B35", "heart"),
}


def write_blocks():
    cream = (255, 245, 220, 235)
    for name, (hexcol, icon) in BLOCKS.items():
        s = 256
        base = vertical_gradient((s, s), rgb(hexcol), lerp(rgb(hexcol), (40, 20, 10, 255), 0.28), 48)
        base = Image.alpha_composite(base, highlight((s, s), 40))
        d = ImageDraw.Draw(base)
        d.rounded_rectangle((4, 4, s - 5, s - 5), 48, outline=(255, 255, 255, 70), width=4)
        ICONS[icon](d, s / 2, s / 2 + 4, 46, cream)
        base.save(OUT / f"block_{name}.png")


def write_dot():
    dot = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    ImageDraw.Draw(dot).ellipse((4, 4, 60, 60), fill=(255, 255, 255, 255))
    dot.filter(ImageFilter.GaussianBlur(1.2)).save(OUT / "particle_dot.png")


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    write_tube()
    write_button()
    write_panel()
    write_blocks()
    write_dot()
    print("Wrote sprites to", OUT)
