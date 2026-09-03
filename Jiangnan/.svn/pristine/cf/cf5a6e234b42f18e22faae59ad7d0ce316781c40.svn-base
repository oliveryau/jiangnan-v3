# -*- coding: utf-8 -*-
"""DEPRECATED: use _apply_new_tech_tree_v2.py for the 8-tech layout."""
from __future__ import annotations

import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
if __name__ == "__main__":
    print("This script is deprecated. Run DataTables/scripts/_apply_new_tech_tree_v2.py instead.")
    sys.exit(1)

# Legacy 28-tech merger kept below for reference only.

import json
from pathlib import Path

from openpyxl import load_workbook

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"
CONFIG = ROOT / "Assets" / "Res" / "Resources" / "Config"

TECH_TYPE = {
    "ExtraWaiterCap": 1,
    "ExtraChefCap": 2,
    "ExtraShopkeeperCap": 3,
    "QueueCap": 4,
    "BusinessHoursBonus": 5,
    "CustomerRefreshMul": 6,
    "PriceProfitBonus": 7,
    "InspireBonus": 8,
    "ResearchSpeedMul": 9,
    "TipGlobalBonus": 10,
    "UnlockStaffLevel": 11,
    "Custom": 99,
}

# StaffPosition: Shopkeeper=1 Chef=2 Waiter=3
# id, name, desc, techType, param, unlock, cost, researchSeconds, sortOrder, remark
TECH_ROWS = [
    # —— 员工成长（合并原 StaffLevel 升级）——
    (100, "跑堂训练", "全体小二可升至2级：会上菜", "UnlockStaffLevel", "3,2", "", 200, 20, 100, "员工·小二L2"),
    (101, "收账训练", "全体小二可升至3级：会收账", "UnlockStaffLevel", "3,3", "100", 400, 25, 101, "员工·小二L3"),
    (102, "利落训练", "全体小二可升至4级：服务更快", "UnlockStaffLevel", "3,4", "101", 700, 30, 102, "员工·小二L4"),
    (103, "老练训练", "全体小二可升至5级", "UnlockStaffLevel", "3,5", "102", 1100, 35, 103, "员工·小二L5"),
    (104, "金牌小二术", "全体小二可升至6级", "UnlockStaffLevel", "3,6", "103", 1600, 40, 104, "员工·小二L6"),
    (105, "领班训练", "全体小二可升至7级", "UnlockStaffLevel", "3,7", "104", 2200, 45, 105, "员工·小二L7"),
    (106, "店中翘楚", "全体小二可升至8级", "UnlockStaffLevel", "3,8", "105", 3000, 50, 106, "员工·小二L8"),
    # —— 编制扩招（无桌子解锁）——
    (107, "扩招小二I", "小二招聘上限+1", "ExtraWaiterCap", "1", "200,101", 500, 30, 107, "编制·需客流+收账训练"),
    (108, "扩招小二II", "小二招聘上限再+1", "ExtraWaiterCap", "1", "107", 1000, 45, 108, "编制"),
    (109, "扩招小二III", "小二招聘上限再+1", "ExtraWaiterCap", "1", "108", 1800, 60, 109, "编制"),
    # —— 厨师成长 ——
    (110, "掌勺学徒", "全体厨师可升至2级：出菜加快", "UnlockStaffLevel", "2,2", "107", 300, 25, 110, "员工·厨师L2"),
    (111, "熟练厨技", "全体厨师可升至3级", "UnlockStaffLevel", "2,3", "110", 550, 30, 111, "员工·厨师L3"),
    (112, "快手厨技", "全体厨师可升至4级", "UnlockStaffLevel", "2,4", "111", 900, 35, 112, "员工·厨师L4"),
    (113, "金牌厨技", "全体厨师可升至5级", "UnlockStaffLevel", "2,5", "112", 1400, 40, 113, "员工·厨师L5"),
    (114, "镇店厨神", "全体厨师可升至6级", "UnlockStaffLevel", "2,6", "113", 2100, 50, 114, "员工·厨师L6"),
    (115, "扩招厨师I", "厨师招聘上限+1", "ExtraChefCap", "1", "110", 800, 40, 115, "编制·服务跟上后"),
    (116, "扩招厨师II", "厨师招聘上限再+1", "ExtraChefCap", "1", "115", 1500, 60, 116, "编制"),
    # —— 掌柜成长 ——
    (117, "熟练掌柜术", "全体掌柜可升至2级", "UnlockStaffLevel", "1,2", "101", 500, 30, 117, "员工·掌柜L2"),
    (118, "金牌掌柜术", "全体掌柜可升至3级", "UnlockStaffLevel", "1,3", "117", 1200, 40, 118, "员工·掌柜L3"),
    # —— 客流（无桌子位置解锁）——
    (200, "门庭开阔", "排队容量+5", "QueueCap", "5", "", 600, 35, 200, "客流·首研"),
    (201, "客似云来I", "刷客间隔×0.9", "CustomerRefreshMul", "900", "200", 1100, 50, 201, "客流"),
    (202, "延时打烊I", "营业时长+60秒", "BusinessHoursBonus", "60", "200", 900, 40, 202, "客流"),
    (203, "客似云来II", "刷客间隔×0.85", "CustomerRefreshMul", "850", "201", 2000, 70, 203, "客流"),
    (204, "延时打烊II", "营业时长再+90秒", "BusinessHoursBonus", "90", "202", 1600, 55, 204, "客流"),
    # —— 经营 ——
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
    return [int(p.strip()) for p in text.split(",") if p.strip()]


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


def write_tech_json() -> None:
    rows = []
    for row in TECH_ROWS:
        rows.append(
            {
                "id": row[0],
                "name": row[1],
                "desc": row[2],
                "techType": TECH_TYPE[row[3]],
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


def zero_staff_level_upgrade_costs() -> None:
    """升级花费改由科技树承担；StaffLevel 仅保留能力数值。"""
    path = DATAS / "StaffLevel.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    # find upgradeCost / remark columns
    headers = [ws.cell(1, c).value for c in range(1, ws.max_column + 1)]
    cost_col = headers.index("upgradeCost") + 1 if "upgradeCost" in headers else 19
    remark_col = headers.index("remark") + 1 if "remark" in headers else 20
    for r in range(5, ws.max_row + 1):
        level = ws.cell(r, 4).value  # level column
        if level is None:
            continue
        if int(level) <= 1:
            ws.cell(r, cost_col).value = 0
        else:
            ws.cell(r, cost_col).value = 0
            remark = ws.cell(r, remark_col).value or ""
            if "科技树" not in str(remark):
                ws.cell(r, remark_col).value = (str(remark) + " ·升级改由科技树").strip(" ·")
    wb.save(path)
    print(f"zeroed upgradeCost in {path}")

    json_path = CONFIG / "tbstafflevel.json"
    rows = json.loads(json_path.read_text(encoding="utf-8"))
    for row in rows:
        if row.get("level", 1) > 1:
            row["upgradeCost"] = 0
            remark = row.get("remark") or ""
            if "科技树" not in remark:
                row["remark"] = (remark + " ·升级改由科技树").strip(" ·")
        else:
            row["upgradeCost"] = 0
    json_path.write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"zeroed upgradeCost in {json_path}")


def main() -> None:
    write_tech_excel()
    write_tech_json()
    zero_staff_level_upgrade_costs()


if __name__ == "__main__":
    main()
