# -*- coding: utf-8 -*-
"""Apply staff-level / tech unlock experience redesign to Excel + JSON."""
from __future__ import annotations

import json
from pathlib import Path

from openpyxl import load_workbook

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"
CONFIG = ROOT / "Assets" / "Res" / "Resources" / "Config"

POS = {"Shopkeeper": 1, "Chef": 2, "Waiter": 3}
TECH = {
    "ExtraWaiterCap": 1,
    "ExtraChefCap": 2,
    "QueueCap": 4,
    "BusinessHoursBonus": 5,
    "CustomerRefreshMul": 6,
    "PriceProfitBonus": 7,
    "InspireBonus": 8,
    "ResearchSpeedMul": 9,
    "TipGlobalBonus": 10,
}

# Excel row: id, position, level, name, move, cook, order, serve, checkout, clean,
# tip, stamina, canOrder, canServe, canCheckout, attitude+, personality+, cost, remark
STAFF_LEVEL_ROWS = [
    # Waiter 8
    (101, "Waiter", 1, "见习小二", 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0, 1.0, True, False, False, 0, 0, 0, "入职仅点单"),
    (102, "Waiter", 2, "跑堂小二", 1.05, 1.0, 0.95, 1.0, 1.0, 0.95, 0, 1.0, True, True, False, 3, 0, 200, "开放上菜"),
    (103, "Waiter", 3, "熟练小二", 1.1, 1.0, 0.9, 0.95, 1.0, 0.9, 2, 0.95, True, True, True, 6, 2, 400, "开放收账"),
    (104, "Waiter", 4, "利落小二", 1.15, 1.0, 0.85, 0.9, 0.9, 0.85, 4, 0.95, True, True, True, 8, 4, 700, "耗时再压"),
    (105, "Waiter", 5, "老练小二", 1.2, 1.0, 0.8, 0.85, 0.85, 0.8, 6, 0.9, True, True, True, 12, 6, 1100, "小费与抗压"),
    (106, "Waiter", 6, "金牌小二", 1.25, 1.0, 0.75, 0.8, 0.8, 0.75, 8, 0.9, True, True, True, 15, 8, 1600, "小费与抗压"),
    (107, "Waiter", 7, "领班小二", 1.3, 1.0, 0.7, 0.75, 0.75, 0.7, 12, 0.85, True, True, True, 18, 10, 2200, "小费与抗压"),
    (108, "Waiter", 8, "店中翘楚", 1.35, 1.0, 0.65, 0.7, 0.7, 0.65, 15, 0.8, True, True, True, 22, 12, 3000, "顶级倍率"),
    # Chef 6
    (201, "Chef", 1, "见习厨师", 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0, 1.0, False, False, False, 0, 0, 0, "出菜慢"),
    (202, "Chef", 2, "掌勺学徒", 1.03, 1.12, 1.0, 1.0, 1.0, 1.0, 0, 0.97, False, False, False, 0, 3, 300, "缓解1~2桌"),
    (203, "Chef", 3, "熟练厨师", 1.06, 1.22, 1.0, 1.0, 1.0, 1.0, 0, 0.94, False, False, False, 2, 5, 550, "跟上2~3桌"),
    (204, "Chef", 4, "快手厨", 1.1, 1.35, 1.0, 1.0, 1.0, 1.0, 0, 0.9, False, False, False, 4, 8, 900, "高峰不堵灶"),
    (205, "Chef", 5, "金牌大厨", 1.12, 1.48, 1.0, 1.0, 1.0, 1.0, 0, 0.86, False, False, False, 6, 10, 1400, "多桌仍稳"),
    (206, "Chef", 6, "镇店厨神", 1.15, 1.6, 1.0, 1.0, 1.0, 1.0, 0, 0.82, False, False, False, 8, 12, 2100, "满编制缓冲"),
    # Shopkeeper 3
    (301, "Shopkeeper", 1, "见习掌柜", 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 0, 1.0, False, False, True, 0, 0, 0, "默认可收账"),
    (302, "Shopkeeper", 2, "熟练掌柜", 1.05, 1.0, 1.0, 1.0, 0.9, 1.0, 3, 0.95, False, False, True, 5, 5, 500, ""),
    (303, "Shopkeeper", 3, "金牌掌柜", 1.1, 1.0, 1.0, 1.0, 0.8, 1.0, 6, 0.9, False, False, True, 10, 10, 1200, ""),
]

# id, name, desc, techType, param(str), unlock(str), cost, researchSeconds, sortOrder, remark
TECH_ROWS = [
    (100, "扩招小二I", "小二招聘上限+1", "ExtraWaiterCap", "1", "200", 500, 30, 100, "编制·需门庭开阔"),
    (101, "扩招小二II", "小二招聘上限再+1", "ExtraWaiterCap", "1", "100", 1000, 45, 101, "编制"),
    (102, "扩招小二III", "小二招聘上限再+1", "ExtraWaiterCap", "1", "101", 1800, 60, 102, "编制"),
    (103, "扩招厨师I", "厨师招聘上限+1", "ExtraChefCap", "1", "100", 800, 40, 103, "编制·需扩招小二I"),
    (104, "扩招厨师II", "厨师招聘上限再+1", "ExtraChefCap", "1", "103", 1500, 60, 104, "编制"),
    (200, "门庭开阔", "排队容量+5", "QueueCap", "5", "", 600, 35, 200, "客流·首研"),
    (201, "客似云来I", "刷客间隔×0.9", "CustomerRefreshMul", "900", "200", 1100, 50, 201, "客流·先制造压力"),
    (202, "延时打烊I", "营业时长+60秒", "BusinessHoursBonus", "60", "200", 900, 40, 202, "客流"),
    (203, "客似云来II", "刷客间隔×0.85", "CustomerRefreshMul", "850", "201", 2000, 70, 203, "客流"),
    (204, "延时打烊II", "营业时长再+90秒", "BusinessHoursBonus", "90", "202", 1600, 55, 204, "客流"),
    (300, "精打细算", "全店小费+5%", "TipGlobalBonus", "5", "", 700, 35, 300, "经营"),
    (301, "时来运转", "研究耗时×0.85", "ResearchSpeedMul", "850", "300", 1000, 40, 301, "经营"),
    (302, "大胆涨价", "涨价额外利润+25%", "PriceProfitBonus", "25", "300", 1200, 45, 302, "经营"),
    (303, "锣鼓喧天", "鼓舞客流额外+25%", "InspireBonus", "25", "302", 1500, 55, 303, "经营"),
    (304, "财源广进", "全店小费再+10%", "TipGlobalBonus", "10", "301,302", 2200, 75, 304, "需301+302"),
]


def parse_int_list(text: str) -> list[int]:
    text = (text or "").strip()
    if not text:
        return []
    return [int(part.strip()) for part in text.split(",") if part.strip()]


def write_staff_level_excel() -> None:
    path = DATAS / "StaffLevel.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    if ws.max_row > 4:
        ws.delete_rows(5, ws.max_row - 4)

    # Ensure remark column exists (col 20)
    headers = [
        ("##var", "id", "position", "level", "name", "moveSpeedMul", "cookSpeedMul",
         "orderTimeMul", "serveTimeMul", "checkoutTimeMul", "cleanTimeMul", "tipBonusPercent",
         "staminaDrainMul", "canOrder", "canServe", "canCheckout", "serviceAttitudeBonus",
         "personalityBonus", "upgradeCost", "remark"),
        ("##type", "int", "StaffPosition", "int", "string", "float", "float",
         "float", "float", "float", "float", "int",
         "float", "bool", "bool", "bool", "int",
         "int", "int", "string"),
        ("##group",) + ("c",) * 19,
        ("##", "等级配置Id", "所属职位", "等级", "等级显示名", "移速倍率", "做菜倍率",
         "点单耗时", "上菜耗时", "收账耗时", "清扫耗时", "小费%",
         "体力消耗", "点单", "上菜", "收账", "态度+",
         "性格+", "升级花费", "备注"),
    ]
    for r, row in enumerate(headers, start=1):
        for c, val in enumerate(row, start=1):
            ws.cell(r, c).value = val

    for i, row in enumerate(STAFF_LEVEL_ROWS):
        r = 5 + i
        ws.cell(r, 1).value = None
        for c, val in enumerate(row, start=2):
            ws.cell(r, c).value = val

    wb.save(path)
    print(f"saved {path} rows={len(STAFF_LEVEL_ROWS)}")


def write_tech_excel() -> None:
    path = DATAS / "TavernTech.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    if ws.max_row > 4:
        ws.delete_rows(5, ws.max_row - 4)

    for i, row in enumerate(TECH_ROWS):
        r = 5 + i
        ws.cell(r, 1).value = None
        for c, val in enumerate(row, start=2):
            ws.cell(r, c).value = val

    wb.save(path)
    print(f"saved {path} rows={len(TECH_ROWS)}")


def write_staff_level_json() -> None:
    rows = []
    for row in STAFF_LEVEL_ROWS:
        rows.append(
            {
                "id": row[0],
                "position": POS[row[1]],
                "level": row[2],
                "name": row[3],
                "moveSpeedMul": row[4],
                "cookSpeedMul": row[5],
                "orderTimeMul": row[6],
                "serveTimeMul": row[7],
                "checkoutTimeMul": row[8],
                "cleanTimeMul": row[9],
                "tipBonusPercent": row[10],
                "staminaDrainMul": row[11],
                "canOrder": row[12],
                "canServe": row[13],
                "canCheckout": row[14],
                "serviceAttitudeBonus": row[15],
                "personalityBonus": row[16],
                "upgradeCost": row[17],
                "remark": row[18],
            }
        )
    out = CONFIG / "tbstafflevel.json"
    out.write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"saved {out} count={len(rows)}")


def write_tech_json() -> None:
    rows = []
    for row in TECH_ROWS:
        rows.append(
            {
                "id": row[0],
                "name": row[1],
                "desc": row[2],
                "techType": TECH[row[3]],
                "param": parse_int_list(row[4]),
                "unlock": parse_int_list(row[5]),
                "cost": row[6],
                "researchSeconds": row[7],
                "sortOrder": row[8],
                "remark": row[9],
            }
        )
    out = CONFIG / "tbtaverntech.json"
    out.write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"saved {out} count={len(rows)}")


def main() -> None:
    write_staff_level_excel()
    write_tech_excel()
    write_staff_level_json()
    write_tech_json()


if __name__ == "__main__":
    main()
