# -*- coding: utf-8 -*-
"""Create / update Dish.xlsx (菜品配置) and register Luban schema."""
from __future__ import annotations

import json
from pathlib import Path

from openpyxl import Workbook, load_workbook

from luban_schema_io import patch_beans

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"
CONFIG = ROOT / "Assets" / "Res" / "Resources" / "Config"

LIST_STRING = "(list#sep=,),string"

# id, name, materials[], flavor[], icon, summary
ROWS = [
    (1, "辣子鸡", ["鸡肉"], ["香辣"], "", "外酥里嫩，香辣开胃"),
    (2, "糖醋里脊", ["猪里脊"], ["酸甜"], "", "酸甜酥脆，老少皆宜"),
    (3, "鱼香肉丝", ["猪肉丝", "木耳", "胡萝卜"], ["咸甜", "酸辣", "鱼香味"], "", "经典川味，咸甜酸辣"),
    (4, "麻婆豆腐", ["嫩豆腐", "牛肉末"], ["麻辣", "咸鲜"], "", "麻辣鲜香，下饭首选"),
    (5, "酸辣土豆丝", ["土豆"], ["酸辣"], "", "爽脆酸辣，清爽解腻"),
    (6, "回锅肉", ["五花肉", "青蒜苗"], ["咸香", "微辣"], "", "肥而不腻，蒜香浓郁"),
    (7, "番茄炒蛋", ["番茄", "鸡蛋"], ["酸甜", "鲜"], "", "家常经典，酸甜鲜香"),
    (8, "小炒黄牛肉", ["黄牛肉"], ["鲜辣"], "", "鲜嫩爆辣，锅气十足"),
    (9, "宫保鸡丁", ["鸡肉", "花生米"], ["咸甜", "微辣"], "", "花生脆香，微辣回甜"),
    (10, "干煸豆角", ["四季豆", "肉末"], ["咸香", "干辣"], "", "干香有嚼头，微辣下饭"),
    (11, "青椒肉丝", ["猪瘦肉", "青椒"], ["咸鲜"], "", "青椒脆爽，肉丝滑嫩"),
    (12, "京酱肉丝", ["猪里脊丝"], ["酱香", "微甜"], "", "酱香浓郁，配饼绝佳"),
    (13, "木须肉", ["猪肉", "鸡蛋", "木耳", "黄瓜"], ["咸鲜", "清淡"], "", "清爽合口，营养均衡"),
    (14, "韭菜炒鸡蛋", ["韭菜", "鸡蛋"], ["鲜咸"], "", "韭香扑鼻，简单鲜美"),
    (15, "地三鲜", ["土豆", "茄子", "青椒"], ["咸鲜", "酱香"], "", "东北经典，咸鲜油润"),
    (16, "水煮肉片", ["猪里脊", "豆芽"], ["麻辣", "咸鲜"], "", "重麻重辣，肉片滑嫩"),
    (17, "蒜苔炒肉", ["五花肉", "蒜苔"], ["咸香"], "", "蒜苔爽脆，肉香四溢"),
    (18, "可乐鸡翅", ["鸡翅中"], ["甜咸"], "", "甜咸入味，软烂脱骨"),
    (19, "红烧排骨", ["猪肋排"], ["咸甜", "酱香"], "", "酱香浓郁，骨肉分离"),
    (20, "清炒丝瓜", ["丝瓜"], ["清淡", "鲜咸"], "", "清淡鲜甜，夏日清爽"),
]

DISH_FIELDS = [
    ("id", "int", "菜品Id"),
    ("name", "string", "菜名"),
    ("materials", LIST_STRING, "材料标签"),
    ("flavor", LIST_STRING, "口味标签"),
    ("icon", "string", "图标Resources路径"),
    ("summary", "string", "简单描述"),
]


def list_to_cell(values: list[str]) -> str:
    return ",".join(values)


def patch_dish_bean() -> None:
    def mutator(ws) -> None:
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

    try:
        patch_beans(mutator)
        print("patched Dish bean in __beans__.xlsx")
    except PermissionError as error:
        print(f"skip bean save (locked): {error}")


def ensure_dish_table() -> None:
    path = DATAS / "__tables__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "TbDish":
            print(f"TbDish already in {path}")
            return

    row = ws.max_row + 1
    ws.cell(row, 2).value = "TbDish"
    ws.cell(row, 3).value = "Dish"
    ws.cell(row, 5).value = "Dish.xlsx"
    ws.cell(row, 6).value = "id"
    ws.cell(row, 7).value = "map"
    ws.cell(row, 9).value = "菜品配置"
    wb.save(path)
    print(f"patched tables {path}")


def write_dish_xlsx() -> None:
    path = DATAS / "Dish.xlsx"
    wb = Workbook()
    ws = wb.active
    ws.title = "Sheet1"
    headers = [
        ("##var", "id", "name", "materials", "flavor", "icon", "summary"),
        ("##type", "int", "string", LIST_STRING, LIST_STRING, "string", "string"),
        ("##group", "c", "c", "c", "c", "c", "c"),
        ("##", "菜品Id", "菜名", "材料标签", "口味标签", "图标路径", "简单描述"),
    ]
    for row_index, row in enumerate(headers, start=1):
        for col_index, value in enumerate(row, start=1):
            ws.cell(row_index, col_index).value = value

    for index, row in enumerate(ROWS):
        excel_row = 5 + index
        ws.cell(excel_row, 1).value = None
        ws.cell(excel_row, 2).value = row[0]
        ws.cell(excel_row, 3).value = row[1]
        ws.cell(excel_row, 4).value = list_to_cell(row[2])
        ws.cell(excel_row, 5).value = list_to_cell(row[3])
        ws.cell(excel_row, 6).value = row[4]
        ws.cell(excel_row, 7).value = row[5]
    wb.save(path)
    print(f"saved {path}")


def write_dish_json() -> None:
    rows = [
        {
            "id": dish_id,
            "name": name,
            "materials": materials,
            "flavor": flavor,
            "icon": icon,
            "summary": summary,
        }
        for dish_id, name, materials, flavor, icon, summary in ROWS
    ]
    out = CONFIG / "tbdish.json"
    out.write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"saved {out}")


def main() -> None:
    patch_dish_bean()
    ensure_dish_table()
    write_dish_xlsx()
    write_dish_json()


if __name__ == "__main__":
    main()
