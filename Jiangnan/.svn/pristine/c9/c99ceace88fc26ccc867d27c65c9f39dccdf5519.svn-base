# -*- coding: utf-8 -*-
"""Create Achievement.xlsx and register Luban schema; write tbachievement.json."""
from __future__ import annotations

import json
from pathlib import Path

from openpyxl import Workbook, load_workbook

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"
CONFIG = ROOT / "Assets" / "Res" / "Resources" / "Config"

ACHIEVEMENT_TYPE = {
    "ServeCustomers": 1,
    "EarnIncome": 2,
    "CookDishes": 3,
    "OpenBusiness": 4,
}

# id, name, desc, type, param, rewardCoin, sortOrder, remark
ROWS = [
    (1, "初出茅庐", "累计招待 10 位客人", "ServeCustomers", "10", 50, 10, ""),
    (2, "宾客盈门", "累计招待 50 位客人", "ServeCustomers", "50", 200, 20, ""),
    (3, "小有积蓄", "生涯累计赚钱 500", "EarnIncome", "500", 80, 30, ""),
    (4, "日进斗金", "生涯累计赚钱 3000", "EarnIncome", "3000", 300, 40, ""),
    (5, "初试厨技", "累计做出 20 道菜", "CookDishes", "20", 60, 50, ""),
    (6, "灶火连天", "累计做出 100 道菜", "CookDishes", "100", 250, 60, ""),
    (7, "新店开张", "累计开业 1 次", "OpenBusiness", "1", 30, 5, ""),
    (8, "生意兴隆", "累计开业 5 次", "OpenBusiness", "5", 150, 70, ""),
]


def parse_int_list(text: str) -> list[int]:
    text = (text or "").strip()
    if not text:
        return []
    return [int(p.strip()) for p in text.split(",") if p.strip()]


def ensure_achievement_enum() -> None:
    path = DATAS / "__enums__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "AchievementType":
            print(f"AchievementType already in {path}")
            return
    # append after last row
    start = ws.max_row + 1
    items = [
        ("ServeCustomers", "招待客人", 1, None),
        ("EarnIncome", "累计赚钱", 2, None),
        ("CookDishes", "做出菜品", 3, None),
        ("OpenBusiness", "开业次数", 4, None),
    ]
    for i, (name, alias, value, comment) in enumerate(items):
        r = start + i
        ws.cell(r, 2).value = "AchievementType" if i == 0 else None
        if i == 0:
            ws.cell(r, 4).value = True
            ws.cell(r, 6).value = "成就任务类型"
        ws.cell(r, 8).value = name
        ws.cell(r, 9).value = alias
        ws.cell(r, 10).value = value
        ws.cell(r, 11).value = comment
    wb.save(path)
    print(f"patched enum {path}")


def ensure_achievement_bean() -> None:
    path = DATAS / "__beans__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "Achievement":
            print(f"Achievement bean already in {path}")
            return
    fields = [
        ("id", "int", "成就Id"),
        ("name", "string", "名称"),
        ("desc", "string", "描述"),
        ("achievementType", "AchievementType", "成就类型"),
        ("param", "(list#sep=,),int", "目标参数"),
        ("rewardCoin", "int", "领取铜钱奖励"),
        ("sortOrder", "int", "排序"),
        ("remark", "string", "备注"),
    ]
    start = ws.max_row + 1
    for i, (fname, ftype, comment) in enumerate(fields):
        r = start + i
        ws.cell(r, 2).value = "Achievement" if i == 0 else None
        if i == 0:
            ws.cell(r, 7).value = "经营成就任务"
        ws.cell(r, 10).value = fname
        ws.cell(r, 12).value = ftype
        ws.cell(r, 14).value = comment
    wb.save(path)
    print(f"patched beans {path}")


def ensure_achievement_table() -> None:
    path = DATAS / "__tables__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "TbAchievement":
            print(f"TbAchievement already in {path}")
            return
    r = ws.max_row + 1
    ws.cell(r, 2).value = "TbAchievement"
    ws.cell(r, 3).value = "Achievement"
    ws.cell(r, 5).value = "Achievement.xlsx"
    ws.cell(r, 6).value = "id"
    ws.cell(r, 7).value = "map"
    ws.cell(r, 9).value = "经营成就任务"
    wb.save(path)
    print(f"patched tables {path}")


def write_achievement_xlsx() -> None:
    path = DATAS / "Achievement.xlsx"
    wb = Workbook()
    ws = wb.active
    ws.title = "Sheet1"
    headers = [
        ("##var", "id", "name", "desc", "achievementType", "param", "rewardCoin", "sortOrder", "remark"),
        ("##type", "int", "string", "string", "AchievementType", "(list#sep=,),int", "int", "int", "string"),
        ("##group",) + ("c",) * 8,
        ("##", "成就Id", "名称", "描述", "类型", "参数", "铜钱奖励", "排序", "备注"),
    ]
    for r, row in enumerate(headers, start=1):
        for c, val in enumerate(row, start=1):
            ws.cell(r, c).value = val
    for i, row in enumerate(ROWS):
        r = 5 + i
        ws.cell(r, 1).value = None
        for c, val in enumerate(row, start=2):
            ws.cell(r, c).value = val
    wb.save(path)
    print(f"saved {path}")


def write_achievement_json() -> None:
    rows = []
    for row in ROWS:
        rows.append(
            {
                "id": row[0],
                "name": row[1],
                "desc": row[2],
                "achievementType": ACHIEVEMENT_TYPE[row[3]],
                "param": parse_int_list(row[4]),
                "rewardCoin": row[5],
                "sortOrder": row[6],
                "remark": row[7],
            }
        )
    out = CONFIG / "tbachievement.json"
    out.write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"saved {out}")


def main() -> None:
    ensure_achievement_enum()
    ensure_achievement_bean()
    ensure_achievement_table()
    write_achievement_xlsx()
    write_achievement_json()


if __name__ == "__main__":
    main()
