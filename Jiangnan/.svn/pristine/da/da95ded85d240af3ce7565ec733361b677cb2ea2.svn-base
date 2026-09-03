# -*- coding: utf-8 -*-
"""Patch __beans__ / __tables__ for Dish list fields and VipGuestDemandHint."""
from __future__ import annotations

from openpyxl.worksheet.worksheet import Worksheet

from luban_schema_io import BEANS_FILE, DATAS, cleanup_schema_temp_files, patch_beans, patch_workbook

LIST_STRING = "(list#sep=,),string"

DISH_FIELDS = [
    ("id", "int", "菜品Id"),
    ("name", "string", "菜名"),
    ("materials", LIST_STRING, "材料标签"),
    ("flavor", LIST_STRING, "口味标签"),
    ("icon", "string", "图标Resources路径"),
    ("summary", "string", "简单描述"),
]

HINT_FIELDS = [
    ("id", "int", "模板Id"),
    ("text", "string", "提示语模板"),
]


def patch_beans_sheet(ws: Worksheet) -> None:
    start_row = None
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "Dish":
            start_row = row
            break
    if start_row is None:
        start_row = ws.max_row + 1

    for index, (fname, ftype, comment) in enumerate(DISH_FIELDS):
        row = start_row + index
        ws.cell(row, 2).value = "Dish" if index == 0 else None
        if index == 0:
            ws.cell(row, 7).value = "菜品配置"
        ws.cell(row, 10).value = fname
        ws.cell(row, 12).value = ftype
        ws.cell(row, 14).value = comment

    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "VipGuestDemandHint":
            return

    start = ws.max_row + 1
    for index, (fname, ftype, comment) in enumerate(HINT_FIELDS):
        row = start + index
        ws.cell(row, 2).value = "VipGuestDemandHint" if index == 0 else None
        if index == 0:
            ws.cell(row, 7).value = "贵客猜菜提示语"
        ws.cell(row, 10).value = fname
        ws.cell(row, 12).value = ftype
        ws.cell(row, 14).value = comment


def patch_tables_sheet(ws: Worksheet) -> None:
    existing = {ws.cell(row, 2).value for row in range(1, ws.max_row + 1)}
    if "TbVipGuestDemandHint" in existing:
        return

    row = ws.max_row + 1
    ws.cell(row, 2).value = "TbVipGuestDemandHint"
    ws.cell(row, 3).value = "VipGuestDemandHint"
    ws.cell(row, 5).value = "VipGuestDemandHint.xlsx"
    ws.cell(row, 6).value = "id"
    ws.cell(row, 7).value = "map"
    ws.cell(row, 9).value = "贵客猜菜提示语"


def main() -> None:
    patch_beans(patch_beans_sheet)
    print(f"patched {BEANS_FILE}")
    tables = DATAS / "__tables__.xlsx"
    patch_workbook(tables, patch_tables_sheet)
    print(f"patched {tables}")
    removed = cleanup_schema_temp_files()
    if removed:
        print("removed temp files:", ", ".join(path.name for path in removed))


if __name__ == "__main__":
    main()
