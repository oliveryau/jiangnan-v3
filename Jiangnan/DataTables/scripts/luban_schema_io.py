# -*- coding: utf-8 -*-
"""Shared helpers for patching Luban schema workbooks and cleaning temp files."""
from __future__ import annotations

import shutil
from pathlib import Path
from typing import Callable

from openpyxl import load_workbook
from openpyxl.worksheet.worksheet import Worksheet

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"
BEANS_FILE = DATAS / "__beans__.xlsx"

# Legacy / intermediate files that must not remain after a successful patch.
SCHEMA_TEMP_GLOBS = (
    "__beans_gen__.xlsx",
    "__beans__.xlsx.new",
    "__beans___patch.xlsx",
    "_beans_verify.xlsx",
    "*_patch.xlsx",
    "*.xlsx.new",
)


def cleanup_schema_temp_files(datas_dir: Path | None = None) -> list[Path]:
    """Remove intermediate schema workbooks created when Excel had files locked."""
    base = datas_dir or DATAS
    removed: list[Path] = []
    for pattern in SCHEMA_TEMP_GLOBS:
        for path in base.glob(pattern):
            if not path.is_file():
                continue
            path.unlink()
            removed.append(path)
    return removed


def patch_workbook(path: Path, mutator: Callable[[Worksheet], None]) -> None:
    """
    Patch a schema workbook in place via a temp copy.
    Raises PermissionError with a clear message when the target is locked by Excel.
    """
    if not path.exists():
        raise FileNotFoundError(f"schema workbook not found: {path}")

    temp = path.with_name(f"{path.stem}_patch{path.suffix}")
    if temp.exists():
        temp.unlink()

    shutil.copy2(path, temp)
    wb = load_workbook(temp)
    mutator(wb.active)
    wb.save(temp)
    wb.close()

    try:
        temp.replace(path)
    except PermissionError as error:
        fallback = path.with_suffix(path.suffix + ".new")
        if fallback.exists():
            fallback.unlink()
        temp.replace(fallback)
        raise PermissionError(
            f"{path.name} is locked (close Excel), wrote {fallback.name} instead"
        ) from error
    finally:
        if temp.exists():
            temp.unlink()


def patch_beans(mutator: Callable[[Worksheet], None]) -> None:
    """Patch DataTables/Datas/__beans__.xlsx and remove stale temp schema files."""
    patch_workbook(BEANS_FILE, mutator)
    cleanup_schema_temp_files()


if __name__ == "__main__":
    removed = cleanup_schema_temp_files()
    if removed:
        print("removed:", ", ".join(path.name for path in removed))
    else:
        print("no schema temp files to remove")
