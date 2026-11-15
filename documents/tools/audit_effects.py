"""审计 XAML 里 Effect 有没有挂在「有内容」的控件上（可重复运行）。

规则：
- `<X.Effect>` 属性元素 / `Effect="..."` 特性都算挂了 Effect；
- 宿主元素只要还有别的子元素，或带 Text/Content 之类内容、或带非空白文本，就判定为「有内容」；
- 只有「无子元素、无文字」的纯装饰元素挂 Effect 才算合规。

用法：python documents/tools/audit_effects.py
输出同时打印到控制台并写入 documents/effect-audit.txt。
"""

import os
import xml.etree.ElementTree as ET

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SCAN_DIRS = ["YoYo.Toolkit", "YoYo.Toolkit.Sample", "YoYo.ToolkitSample"]
CONTENT_KEYS = ("Text", "Content", "Header", "ToolTip", "ItemsSource")


def local(tag):
    return tag.split("}")[-1] if "}" in tag else tag


def xaml_files():
    for folder in SCAN_DIRS:
        base = os.path.join(ROOT, folder)
        for current, dirs, files in os.walk(base):
            dirs[:] = [d for d in dirs if d not in ("bin", "obj", ".git")]
            for name in files:
                if name.endswith(".xaml"):
                    yield os.path.join(current, name)


def has_content(owner, excluded):
    for child in owner:
        if child is not excluded:
            return True, f"含子元素 <{local(child.tag)}>"
    bare = {local(k): v for k, v in owner.attrib.items()}
    for key in CONTENT_KEYS:
        if key in bare and bare[key].strip():
            return True, f"带内容属性 {key}"
    if (owner.text or "").strip():
        return True, "带文本内容"
    return False, "无子元素、无文字（纯装饰）"


def main():
    lines = ["Effect 挂载审计（合规 = Effect 只挂在纯装饰元素上）", ""]
    findings = 0
    violations = 0

    for path in sorted(xaml_files()):
        try:
            tree = ET.parse(path)
        except ET.ParseError as error:
            lines.append(f"[跳过] {os.path.relpath(path, ROOT)}: XML 解析失败 {error}")
            continue

        for owner in tree.iter():
            children = list(owner)
            effect_child = next((c for c in children
                                 if local(c.tag).endswith(".Effect") or local(c.tag) == "Effect"), None)
            effect_attr = next((k for k in owner.attrib if local(k) == "Effect"), None)

            if effect_child is None and effect_attr is None:
                continue

            findings += 1
            kind = "属性元素 <X.Effect>" if effect_child is not None else f"特性 {effect_attr}"
            content, why = has_content(owner, effect_child)
            verdict = "违规：有内容的控件挂了 Effect" if content else "合规"
            violations += int(content)

            rel = os.path.relpath(path, ROOT).replace("\\", "/")
            lines.append(f"[{verdict}] {rel}")
            lines.append(f"        载体 <{local(owner.tag)}> · {kind} · {why}")

    lines.append("")
    lines.append(f"合计：Effect {findings} 处，违规 {violations} 处。")
    lines.append("附注：OpacityMask / VisualBrush 属于另一类栅格化机制，已人工核对：")
    lines.append("      Progress.Templates.xaml 的 OpacityMask 全在 Ellipse/Rectangle 纯图形上；")
    lines.append("      PlaceholderTextBox / PasswordHelper 的 VisualBrush 只用于画水印。")

    text = "\n".join(lines)
    print(text)
    with open(os.path.join(ROOT, "documents", "effect-audit.txt"), "w", encoding="utf-8") as handle:
        handle.write(text + "\n")


if __name__ == "__main__":
    main()
