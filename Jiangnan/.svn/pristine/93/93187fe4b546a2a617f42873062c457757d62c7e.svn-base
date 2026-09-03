# -*- coding: utf-8 -*-
"""Atomic staff tech tree + shopkeeper node 121 → Excel only (run gen.bat after)."""
from __future__ import annotations

from pathlib import Path

from openpyxl import load_workbook

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"

TECH_TYPE = {
    "Custom": 99,
    "CustomerRefreshSecondsBonus": 12,
    "BusinessHoursMul": 13,
    "EnableVipCustomer": 16,
    "TipGlobalBonus": 10,
    "PriceProfitBonus": 7,
}

STAFF_EFFECT = {
    "None": 0,
    "GrantCanOrder": 1,
    "GrantCanServe": 2,
    "GrantCanCheckout": 3,
    "ImproveMoveSpeed": 4,
    "ImproveOrderTime": 5,
    "ImproveServeTime": 6,
    "ImproveCheckoutTime": 7,
    "ImproveCleanTime": 8,
    "ImproveCookSpeed": 9,
    "EnableCounterRandomReward": 10,
}

# id, name, desc, techType, param, unlock, cost, researchServeCustomers, sortOrder, remark,
# staffPosition, staffEffect, staffEffectValue
TECH_ROWS = [
    (101, "会点单", "小二学会点单", "Custom", "", "", 200, 20, 101, "小二", "Waiter", "GrantCanOrder", 1),
    (102, "快点单", "点单耗时×0.95", "Custom", "", "101", 150, 15, 102, "小二", "Waiter", "ImproveOrderTime", 950),
    (103, "会上菜", "小二学会上菜", "Custom", "", "102", 200, 18, 103, "小二", "Waiter", "GrantCanServe", 1),
    (104, "快上菜", "上菜耗时×0.90", "Custom", "", "103", 150, 15, 104, "小二", "Waiter", "ImproveServeTime", 900),
    (105, "会收账", "小二学会收账", "Custom", "", "104", 250, 20, 105, "小二", "Waiter", "GrantCanCheckout", 1),
    (106, "快收账", "收账耗时×0.80", "Custom", "", "105", 150, 15, 106, "小二", "Waiter", "ImproveCheckoutTime", 800),
    (107, "疾行", "移速×1.30", "Custom", "", "106", 300, 25, 107, "小二", "Waiter", "ImproveMoveSpeed", 1300),
    (111, "掌勺学徒", "做菜速度×1.12；研发包子", "Custom", "", "", 300, 25, 111, "厨师", "Chef", "ImproveCookSpeed", 1120),
    (112, "熟练厨师", "做菜速度×1.22；研发鱼", "Custom", "", "111", 350, 28, 112, "厨师", "Chef", "ImproveCookSpeed", 1220),
    (113, "快手厨", "做菜速度×1.35；研发面条", "Custom", "", "112", 400, 30, 113, "厨师", "Chef", "ImproveCookSpeed", 1350),
    (114, "金牌大厨", "做菜速度×1.48；研发酒", "Custom", "", "113", 500, 35, 114, "厨师", "Chef", "ImproveCookSpeed", 1480),
    (115, "镇店厨神", "做菜速度×1.60；研发麻婆豆腐", "Custom", "", "114", 600, 40, 115, "厨师", "Chef", "ImproveCookSpeed", 1600),
    (121, "柜台进账", "开启掌柜柜台间隔随机金币", "Custom", "", "107", 800, 40, 121, "掌柜", "Shopkeeper", "EnableCounterRandomReward", 1),
    (201, "刷客间隔缩短", "刷客间隔缩短4秒", "CustomerRefreshSecondsBonus", "4", "103", 600, 35, 201, "客流", "", "None", 0),
    (202, "延时打烊", "营业时间翻倍", "BusinessHoursMul", "2000", "111", 900, 40, 202, "客流", "", "None", 0),
    (203, "拉客贵客", "解锁贵客判定玩法", "EnableVipCustomer", "", "202", 1500, 55, 203, "客流", "", "None", 0),
    (301, "小费", "全店结账小费+5%", "TipGlobalBonus", "5", "201", 700, 35, 301, "经营", "", "None", 0),
    (302, "涨价", "菜品收入永久+5%", "PriceProfitBonus", "5", "301", 1200, 45, 302, "经营", "", "None", 0),
    (303, "开启二楼", "暂未开放", "Custom", "", "", 0, 0, 303, "经营·锁定", "", "None", 0),
]

TECH_HEADERS = [
    (
        "##var",
        "id",
        "name",
        "desc",
        "techType",
        "param",
        "unlock",
        "cost",
        "researchServeCustomers",
        "sortOrder",
        "remark",
        "staffPosition",
        "staffEffect",
        "staffEffectValue",
    ),
    (
        "##type",
        "int",
        "string",
        "string",
        "TavernTechType",
        "(list#sep=,),int",
        "(list#sep=,),int",
        "int",
        "int",
        "int",
        "string",
        "StaffPosition?",
        "StaffTechEffect",
        "int",
    ),
    ("##group",) + ("c",) * 13,
    (
        "##",
        "科技Id",
        "名称",
        "描述",
        "效果类型",
        "参数",
        "前置科技",
        "花费",
        "招待顾客(结算)数量",
        "排序",
        "备注",
        "员工职位",
        "员工原子效果",
        "效果值(千分比/0-1)",
    ),
]


def parse_int_list(text: str) -> list[int]:
    text = (text or "").strip()
    if not text:
        return []
    return [int(p.strip()) for p in text.split(",") if p.strip()]


def _rewrite_data_sheet(path: Path, headers: list[tuple], data_rows: list[tuple]) -> None:
    wb = load_workbook(path)
    ws = wb.active
    if ws.max_row > 0:
        ws.delete_rows(1, ws.max_row)
    for r, row in enumerate(headers, start=1):
        for c, val in enumerate(row, start=1):
            ws.cell(r, c).value = val
    for i, row in enumerate(data_rows):
        r = len(headers) + 1 + i
        ws.cell(r, 1).value = None
        for c, val in enumerate(row, start=2):
            cell_val = None if val == "" and c == 12 else val  # staffPosition 留空须为 null，不能写 ""
            ws.cell(r, c).value = cell_val
    wb.save(path)
    print(f"saved {path} rows={len(data_rows)}")


def patch_enums() -> None:
    path = DATAS / "__enums__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    # Remove StaffTechEffect block if re-running
    rows_to_delete = []
    in_staff_effect = False
    for row in range(1, ws.max_row + 1):
        full_name = ws.cell(row, 2).value
        if full_name == "StaffTechEffect":
            in_staff_effect = True
            rows_to_delete.append(row)
            continue
        if in_staff_effect:
            if full_name not in (None, ""):
                break
            rows_to_delete.append(row)
    for row in reversed(rows_to_delete):
        ws.delete_rows(row, 1)

    insert_at = ws.max_row + 1
    ws.cell(insert_at, 2).value = "StaffTechEffect"
    ws.cell(insert_at, 4).value = True
    ws.cell(insert_at, 6).value = "员工科技原子效果"
    insert_at += 1
    for name, alias, value in [
        ("None", "无", 0),
        ("GrantCanOrder", "会点单", 1),
        ("GrantCanServe", "会上菜", 2),
        ("GrantCanCheckout", "会收账", 3),
        ("ImproveMoveSpeed", "移速", 4),
        ("ImproveOrderTime", "点单耗时", 5),
        ("ImproveServeTime", "上菜耗时", 6),
        ("ImproveCheckoutTime", "收账耗时", 7),
        ("ImproveCleanTime", "清扫耗时", 8),
        ("ImproveCookSpeed", "做菜速度", 9),
        ("EnableCounterRandomReward", "柜台随机金币", 10),
    ]:
        ws.insert_rows(insert_at)
        ws.cell(insert_at, 8).value = name
        ws.cell(insert_at, 9).value = alias
        ws.cell(insert_at, 10).value = value
        insert_at += 1
    wb.save(path)
    print(f"patched {path} StaffTechEffect")


def patch_beans() -> None:
    path = DATAS / "__beans__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    # Remove StaffLevel bean block (rows where col2 == StaffLevel until next bean)
    start = None
    end = None
    for row in range(1, ws.max_row + 1):
        full_name = ws.cell(row, 2).value
        if full_name == "StaffLevel":
            start = row
            continue
        if start is not None and full_name not in (None, "") and full_name != "StaffLevel":
            end = row
            break
    if start is not None:
        delete_end = (end - 1) if end else ws.max_row
        ws.delete_rows(start, delete_end - start + 1)
        print(f"removed StaffLevel bean rows {start}-{delete_end}")

    # Append TavernTech staff fields if missing
    has_staff_position = False
    tavern_start = None
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 10).value == "staffPosition":
            has_staff_position = True
        if ws.cell(row, 2).value == "TavernTech":
            tavern_start = row
    if not has_staff_position and tavern_start is not None:
        # find last TavernTech field row
        row = tavern_start
        while row <= ws.max_row and ws.cell(row, 2).value in (None, "TavernTech"):
            row += 1
        insert_at = row
        for field_name, field_type, comment in [
            ("staffPosition", "StaffPosition?", "员工职位(非员工科技留空)"),
            ("staffEffect", "StaffTechEffect", "员工原子效果"),
            ("staffEffectValue", "int", "效果值(千分比/开关0-1)"),
        ]:
            ws.insert_rows(insert_at)
            ws.cell(insert_at, 10).value = field_name
            ws.cell(insert_at, 12).value = field_type
            ws.cell(insert_at, 14).value = comment
            insert_at += 1
        print("added TavernTech staff fields")
    wb.save(path)
    print(f"patched {path}")


def patch_tables() -> None:
    path = DATAS / "__tables__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "TbStaffLevel":
            ws.delete_rows(row, 1)
            print(f"removed TbStaffLevel from {path}")
            break
    wb.save(path)


def write_tech_excel() -> None:
    _rewrite_data_sheet(DATAS / "TavernTech.xlsx", TECH_HEADERS, TECH_ROWS)


def main() -> None:
    patch_enums()
    patch_beans()
    patch_tables()
    write_tech_excel()
    print("Excel updated. Run DataTables\\gen.bat to regenerate JSON and LubanCode.")


if __name__ == "__main__":
    main()
