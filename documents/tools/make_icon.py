"""生成 YoYo.Toolkit 的主题图标（无第三方依赖）。

设计：
- 圆角方形底 + 对角线渐变（强调色 #17AFA2 → 深青 #0A5557），顶部一层很淡的高光；
- 白色 "Y" 字（三段圆头胶囊），与标题栏里 "YY" 徽标同源；
- 用 SDF 做抗锯齿，16px 下笔画仍清晰。

输出：
- YoYo.Toolkit.Sample/Assets/app.ico    多尺寸（16/20/24/32/40/48/64/128/256）
- YoYo.Toolkit/Assets/icon.png         512px，给 NuGet PackageIcon
- documents/icon/preview.png           浅/深底对比预览
"""

import math
import os
import struct
import zlib

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))

TOP = (0x17, 0xAF, 0xA2)
BOTTOM = (0x0A, 0x55, 0x57)
GLYPH = (255, 255, 255)

RADIUS_RATIO = 0.215
STROKE_RATIO = 0.115
# Y 的三段（归一化坐标）
ARM_L = ((0.278, 0.252), (0.500, 0.500))
ARM_R = ((0.722, 0.252), (0.500, 0.500))
STEM = ((0.500, 0.500), (0.500, 0.760))


def clamp(v, lo, hi):
    return lo if v < lo else hi if v > hi else v


def sd_round_rect(px, py, size, radius):
    half = size / 2.0
    r = radius
    qx = abs(px - half) - (half - r)
    qy = abs(py - half) - (half - r)
    ax, ay = max(qx, 0.0), max(qy, 0.0)
    outside = math.hypot(ax, ay)
    return outside + min(max(qx, qy), 0.0) - r


def sd_segment(px, py, a, b, radius):
    ax, ay = a
    bx, by = b
    pax, pay = px - ax, py - ay
    bax, bay = bx - ax, by - ay
    denom = bax * bax + bay * bay
    h = clamp((pax * bax + pay * bay) / denom, 0.0, 1.0) if denom else 0.0
    dx = pax - bax * h
    dy = pay - bay * h
    return math.hypot(dx, dy) - radius


def coverage(distance):
    return clamp(0.5 - distance, 0.0, 1.0)


def render(size):
    """返回 RGBA 字节（straight alpha），尺寸 size×size。"""
    radius = size * RADIUS_RATIO
    stroke = size * STROKE_RATIO
    arm_l = tuple((x * size, y * size) for x, y in ARM_L)
    arm_r = tuple((x * size, y * size) for x, y in ARM_R)
    stem = tuple((x * size, y * size) for x, y in STEM)

    pixels = bytearray()
    for y in range(size):
        py = y + 0.5
        for x in range(size):
            px = x + 0.5

            base = coverage(sd_round_rect(px, py, size, radius))
            if base <= 0.0:
                pixels += b"\x00\x00\x00\x00"
                continue

            # 对角线渐变
            t = clamp((px + py) / (2.0 * size), 0.0, 1.0)
            r = TOP[0] + (BOTTOM[0] - TOP[0]) * t
            g = TOP[1] + (BOTTOM[1] - TOP[1]) * t
            b = TOP[2] + (BOTTOM[2] - TOP[2]) * t

            # 顶部高光：只在向上 55% 区域内淡出
            gloss = max(0.0, 1.0 - py / (size * 0.55)) ** 1.6 * 0.16
            r += (255 - r) * gloss
            g += (255 - g) * gloss
            b += (255 - b) * gloss

            # 白色 Y
            glyph = max(
                coverage(sd_segment(px, py, *arm_l, stroke / 2.0)),
                coverage(sd_segment(px, py, *arm_r, stroke / 2.0)),
                coverage(sd_segment(px, py, *stem, stroke / 2.0)),
            )
            if glyph > 0.0:
                r += (GLYPH[0] - r) * glyph
                g += (GLYPH[1] - g) * glyph
                b += (GLYPH[2] - b) * glyph

            pixels += bytes((int(round(r)), int(round(g)), int(round(b)), int(round(base * 255))))

    return bytes(pixels)


def png_bytes(size, rgba):
    raw = bytearray()
    stride = size * 4
    for y in range(size):
        raw.append(0)
        raw += rgba[y * stride:(y + 1) * stride]

    def chunk(tag, data):
        return (struct.pack(">I", len(data)) + tag + data
                + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF))

    header = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)
    return (b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header)
            + chunk(b"IDAT", zlib.compress(bytes(raw), 9)) + chunk(b"IEND", b""))


def bmp_bytes(size, rgba):
    """ICO 内的 32bpp DIB：BITMAPINFOHEADER + 自底向上的 BGRA + 全 0 的 AND 掩码。"""
    header = struct.pack("<IiiHHIIiiII", 40, size, size * 2, 1, 32, 0, size * size * 4, 0, 0, 0, 0)
    rows = bytearray()
    for y in range(size - 1, -1, -1):
        row = rgba[y * size * 4:(y + 1) * size * 4]
        for x in range(0, len(row), 4):
            rows += bytes((row[x + 2], row[x + 1], row[x], row[x + 3]))
    mask_stride = ((size + 31) // 32) * 4
    return header + bytes(rows) + b"\x00" * (mask_stride * size)


def ico_bytes(images):
    entries, payloads, offset = b"", b"", 6 + 16 * len(images)
    for size, data in images:
        entries += struct.pack("<BBBBHHII", size if size < 256 else 0, size if size < 256 else 0,
                               0, 0, 1, 32, len(data), offset + len(payloads))
        payloads += data
    return struct.pack("<HHH", 0, 1, len(images)) + entries + payloads


def write(path, data):
    full = os.path.join(ROOT, path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "wb") as handle:
        handle.write(data)
    print(f"  {path}  ({len(data) / 1024:.1f} KB)")


def compose_preview(sizes, dark):
    """浅底/深底各一条尺寸对照图。"""
    gap, margin = 18, 16
    width = margin * 2 + sum(sizes) + gap * (len(sizes) - 1)
    height = max(sizes) + margin * 2
    bg = (13, 16, 17) if dark else (242, 243, 244)

    canvas = bytearray()
    for _ in range(width * height):
        canvas += bytes((bg[0], bg[1], bg[2], 255))

    xx = margin
    for size in sizes:
        rgba = render(size)
        top = (height - size) // 2
        for y in range(size):
            for x in range(size):
                i = (y * size + x) * 4
                a = rgba[i + 3] / 255.0
                if a <= 0:
                    continue
                px = ((top + y) * width + xx + x) * 4
                for c in range(3):
                    canvas[px + c] = int(canvas[px + c] * (1 - a) + rgba[i + c] * a)
        xx += size + gap
    return width, height, bytes(canvas)


def main():
    print("生成图标：")

    ico_sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
    cache = {}
    images = []
    for size in ico_sizes:
        rgba = render(size)
        cache[size] = rgba
        images.append((size, bmp_bytes(size, rgba) if size < 256 else png_bytes(size, rgba)))
    write("YoYo.Toolkit.Sample/Assets/app.ico", ico_bytes(images))

    write("YoYo.Toolkit/Assets/icon.png", png_bytes(512, render(512)))

    small = [16, 24, 32, 48, 64, 128, 256]
    for dark, name in ((False, "preview-light.png"), (True, "preview-dark.png")):
        width, height, rgba = compose_preview(small, dark)
        write(f"documents/icon/{name}", png_bytes_raw(width, height, rgba))

    write("documents/icon/icon-256.png", png_bytes(256, cache[256] if 256 in cache else render(256)))


def png_bytes_raw(width, height, rgba):
    raw = bytearray()
    stride = width * 4
    for y in range(height):
        raw.append(0)
        raw += rgba[y * stride:(y + 1) * stride]

    def chunk(tag, data):
        return (struct.pack(">I", len(data)) + tag + data
                + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF))

    header = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    return (b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header)
            + chunk(b"IDAT", zlib.compress(bytes(raw), 9)) + chunk(b"IEND", b""))


if __name__ == "__main__":
    main()
