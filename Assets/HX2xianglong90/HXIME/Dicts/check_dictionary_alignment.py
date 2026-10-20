#!/usr/bin/env python3
"""Check dictionary formatting and row alignment between simplified and traditional files."""

from pathlib import Path
import sys

HERE = Path(__file__).resolve().parent
SIMP = HERE / "pinyin_simp.dict.yaml.txt"
TRAD = HERE / "OPENCCpinyin_trad.dict.yaml.txt"


def load_rows(path: Path) -> tuple[list[list[str]], list[str]]:
    lines = path.read_text(encoding="utf-8-sig").splitlines()
    rows: list[list[str]] = []
    errors: list[str] = []

    for line_no, line in enumerate(lines, 1):
        fields = line.split("\t")
        if len(fields) != 3:
            errors.append(f"{path.name}:{line_no}: 需要恰好 3 列（字、拼音、权重），实际 {len(fields)} 列")
            continue
        if any(not field for field in fields):
            errors.append(f"{path.name}:{line_no}: 存在空字段")
        if any(field != field.strip() for field in fields):
            errors.append(f"{path.name}:{line_no}: 字段首尾有空白，可能存在错误缩进")
        if not fields[2].isdigit():
            errors.append(f"{path.name}:{line_no}: 权重不是非负整数：{fields[2]!r}")
        rows.append(fields)

    return rows, errors


def main() -> int:
    simp_rows, errors = load_rows(SIMP)
    trad_rows, trad_errors = load_rows(TRAD)
    errors.extend(trad_errors)

    if len(simp_rows) != len(trad_rows):
        errors.append(f"行数不一致：简体 {len(simp_rows)} 行，繁体 {len(trad_rows)} 行")

    for line_no, (simp, trad) in enumerate(zip(simp_rows, trad_rows), 1):
        if simp[1:] != trad[1:]:
            errors.append(
                f"第 {line_no} 行拼音/权重不一致："
                f"简体({simp[1]}, {simp[2]})，繁体({trad[1]}, {trad[2]})"
            )

    if errors:
        print(f"检查失败：发现 {len(errors)} 个问题")
        for error in errors[:100]:
            print(f"- {error}")
        if len(errors) > 100:
            print(f"- 另有 {len(errors) - 100} 个问题未显示")
        return 1

    print(f"检查通过：两份词库均为 {len(simp_rows)} 行，格式正确，拼音和权重逐行一致。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
