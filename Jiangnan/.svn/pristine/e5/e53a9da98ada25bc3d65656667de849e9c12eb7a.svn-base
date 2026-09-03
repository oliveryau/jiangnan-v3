# -*- coding: utf-8 -*-
"""Remove canOrder/canServe/canCheckout/quality from Staff Luban schema and data."""
from __future__ import annotations

import json
from pathlib import Path

from openpyxl import Workbook, load_workbook

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"
CONFIG = ROOT / "Assets" / "Res" / "Resources" / "Config"

REMOVE_STAFF_FIELDS = {"canOrder", "canServe", "canCheckout", "quality"}
POSITION_NAMES = {1: "Shopkeeper", 2: "Chef", 3: "Waiter"}


def strip_staff_bean_fields() -> None:
    path = DATAS / "__beans__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    rows_to_delete = []
    for row in range(1, ws.max_row + 1):
        field_name = ws.cell(row, 10).value
        if field_name in REMOVE_STAFF_FIELDS:
            rows_to_delete.append(row)

    for row in reversed(rows_to_delete):
        ws.delete_rows(row, 1)
    wb.save(path)
    print(f"removed {len(rows_to_delete)} Staff fields from {path}")


def strip_staff_quality_enum() -> None:
    path = DATAS / "__enums__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    rows_to_delete = []
    in_staff_quality = False
    for row in range(1, ws.max_row + 1):
        full_name = ws.cell(row, 2).value
        item_name = ws.cell(row, 8).value
        if full_name == "StaffQuality":
            in_staff_quality = True
            rows_to_delete.append(row)
            continue
        if in_staff_quality:
            if full_name is not None and full_name != "StaffQuality":
                break
            if item_name in {"Common", "Good", "Rare", "Epic"}:
                rows_to_delete.append(row)
                continue
            break

    for row in reversed(rows_to_delete):
        ws.delete_rows(row, 1)
    wb.save(path)
    print(f"removed StaffQuality enum from {path}")


def rebuild_staff_xlsx() -> None:
    json_path = CONFIG / "tbstaff.json"
    rows = json.loads(json_path.read_text(encoding="utf-8"))

    path = DATAS / "Staff.xlsx"
    wb = Workbook()
    ws = wb.active
    ws.title = "Sheet1"
    headers = [
        ("##var", "id", "name", "position", "moveSpeed", "learningAbility", "recruitmentCosts", "salary", "visual", "remark"),
        ("##type", "int", "string", "StaffPosition", "float", "int", "int", "int", "string", "string"),
        ("##group", "c", "c", "c", "c", "c", "c", "c", "c", "c"),
        ("##", "员工Id", "姓名", "职位", "移动速度", "学习力", "招聘费用", "工资", "形象Key", "备注"),
    ]
    for r, row in enumerate(headers, start=1):
        for c, val in enumerate(row, start=1):
            ws.cell(r, c).value = val

    for i, item in enumerate(rows):
        r = 5 + i
        ws.cell(r, 1).value = None
        ws.cell(r, 2).value = item["id"]
        ws.cell(r, 3).value = item["name"]
        ws.cell(r, 4).value = POSITION_NAMES.get(item["position"], "Waiter")
        ws.cell(r, 5).value = item["moveSpeed"]
        ws.cell(r, 6).value = item.get("learningAbility", 1)
        ws.cell(r, 7).value = item["recruitmentCosts"]
        ws.cell(r, 8).value = item["salary"]
        ws.cell(r, 9).value = item["visual"]
        ws.cell(r, 10).value = item.get("remark", "")

    wb.save(path)
    print(f"rebuilt {path} ({len(rows)} rows)")


def write_staff_json_preview() -> None:
    """Strip removed keys from tbstaff.json until gen.bat runs."""
    json_path = CONFIG / "tbstaff.json"
    rows = json.loads(json_path.read_text(encoding="utf-8"))
    cleaned = []
    for item in rows:
        cleaned.append(
            {
                "id": item["id"],
                "name": item["name"],
                "position": item["position"],
                "moveSpeed": item["moveSpeed"],
                "learningAbility": item.get("learningAbility", 1),
                "recruitmentCosts": item["recruitmentCosts"],
                "salary": item["salary"],
                "visual": item["visual"],
                "remark": item.get("remark", ""),
            }
        )
    json_path.write_text(json.dumps(cleaned, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"cleaned {json_path}")


def main() -> None:
    strip_staff_bean_fields()
    strip_staff_quality_enum()
    rebuild_staff_xlsx()
    write_staff_json_preview()


if __name__ == "__main__":
    main()
