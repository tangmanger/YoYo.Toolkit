"""PNG 裁剪/放大工具（无第三方依赖），用于放大检查截图细节。

用法：
  python documents/tools/crop_png.py <输入> <输出> <x> <y> <w> <h> [放大倍数]
"""

import struct
import sys
import zlib


def read_png(path):
    data = open(path, "rb").read()
    pos, width, height, color_type, depth = 8, 0, 0, 6, 8
    idat = b""

    while pos < len(data):
        length = struct.unpack(">I", data[pos:pos + 4])[0]
        tag = data[pos + 4:pos + 8]
        chunk = data[pos + 8:pos + 8 + length]
        pos += 12 + length

        if tag == b"IHDR":
            width, height, depth, color_type = struct.unpack(">IIBB", chunk[:10])
        elif tag == b"IDAT":
            idat += chunk
        elif tag == b"IEND":
            break

    if depth != 8:
        raise SystemExit("只支持 8 位色深")

    channels = {0: 1, 2: 3, 4: 2, 6: 4}[color_type]
    raw = zlib.decompress(idat)
    stride = width * channels
    rows = []
    prev = bytearray(stride)

    for y in range(height):
        offset = y * (stride + 1)
        kind = raw[offset]
        line = bytearray(raw[offset + 1:offset + 1 + stride])

        if kind == 1:
            for i in range(channels, stride):
                line[i] = (line[i] + line[i - channels]) & 0xFF
        elif kind == 2:
            for i in range(stride):
                line[i] = (line[i] + prev[i]) & 0xFF
        elif kind == 3:
            for i in range(stride):
                left = line[i - channels] if i >= channels else 0
                line[i] = (line[i] + ((left + prev[i]) >> 1)) & 0xFF
        elif kind == 4:
            for i in range(stride):
                a = line[i - channels] if i >= channels else 0
                b = prev[i]
                c = prev[i - channels] if i >= channels else 0
                p = a + b - c
                pa, pb, pc = abs(p - a), abs(p - b), abs(p - c)
                pr = a if (pa <= pb and pa <= pc) else (b if pb <= pc else c)
                line[i] = (line[i] + pr) & 0xFF

        prev = line
        rows.append(bytes(line))

    if channels == 4:
        pixels = b"".join(rows)
    elif channels == 3:
        pixels = b"".join(bytes(row[i:i + 3]) + b"\xff" for row in rows for i in range(0, stride, 3))
    else:
        raise SystemExit("不支持的通道数")

    return width, height, pixels


def write_png(path, width, height, pixels):
    raw = bytearray()
    stride = width * 4
    for y in range(height):
        raw.append(0)
        raw += pixels[y * stride:(y + 1) * stride]

    def chunk(tag, data):
        return (struct.pack(">I", len(data)) + tag + data
                + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF))

    header = struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0)
    png = (b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header)
           + chunk(b"IDAT", zlib.compress(bytes(raw), 9)) + chunk(b"IEND", b""))
    open(path, "wb").write(png)


def main():
    source, target = sys.argv[1], sys.argv[2]
    x, y, w, h = (int(v) for v in sys.argv[3:7])
    scale = int(sys.argv[7]) if len(sys.argv) > 7 else 1

    width, height, pixels = read_png(source)
    x, y = max(0, x), max(0, y)
    w, h = min(w, width - x), min(h, height - y)

    out = bytearray()
    for row in range(h * scale):
        sy = y + row // scale
        for col in range(w * scale):
            sx = x + col // scale
            i = (sy * width + sx) * 4
            out += pixels[i:i + 4]

    write_png(target, w * scale, h * scale, bytes(out))
    print(f"{target}: {w * scale}x{h * scale}")


if __name__ == "__main__":
    main()
