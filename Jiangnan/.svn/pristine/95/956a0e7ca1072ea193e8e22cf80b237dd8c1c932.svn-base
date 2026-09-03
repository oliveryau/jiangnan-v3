#!/usr/bin/env python3
"""Append table facilities 7-12 to Facility.xlsx (ids 7-9, 13-15 for 10-12)."""

from __future__ import annotations

from pathlib import Path

from openpyxl import load_workbook

ROOT = Path(__file__).resolve().parents[1]
PATH = ROOT / "Datas" / "Facility.xlsx"

NEW_ROWS = [
    [None, 7, "桌子7", "Table", "TableArea_7", "table_7", 2, "6", 10000, 900, "2", 7, None],
    [None, 8, "桌子8", "Table", "TableArea_8", "table_8", 2, "7", 13000, 900, "2", 8, None],
    [None, 9, "桌子9", "Table", "TableArea_9", "table_9", 2, "8", 16000, 900, "2", 9, None],
    # 10-12 use facility id 13-15 to avoid counter/cabinet/wine ids 10-12
    [None, 13, "桌子10", "Table", "TableArea_10", "table_10", 2, "9", 20000, 900, "2", 10, None],
    [None, 14, "桌子11", "Table", "TableArea_11", "table_11", 2, "13", 25000, 900, "2", 11, None],
    [None, 15, "桌子12", "Table", "TableArea_12", "table_12", 2, "14", 30000, 900, "2", 12, None],
]


def main() -> None:
    wb = load_workbook(PATH)
    ws = wb.active
    existing_ids = set()
    for row in ws.iter_rows(min_row=5, values_only=True):
        if row[1] is not None:
            existing_ids.add(int(row[1]))

    appended = 0
    for row in NEW_ROWS:
        facility_id = int(row[1])
        if facility_id in existing_ids:
            continue
        ws.append(row)
        appended += 1

    wb.save(PATH)
    print(f"updated {PATH}, appended {appended} rows")


if __name__ == "__main__":
    main()
