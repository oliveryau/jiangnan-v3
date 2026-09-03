# -*- coding: utf-8 -*-
"""Register StaffTalent Luban schema (enum/bean/table)."""
from __future__ import annotations

from pathlib import Path

from openpyxl import load_workbook

ROOT = Path(__file__).resolve().parents[2]
DATAS = ROOT / "DataTables" / "Datas"

STAFF_TALENT_TYPES = [
    ("NoEffect", "无效果", 0),
    ("MoveSpeedBonusPercent", "移速加成", 1),
    ("OrderSpeedBonusPercent", "点单加速", 2),
    ("ServeSpeedBonusPercent", "上菜加速", 3),
    ("CheckoutSpeedBonusPercent", "收账加速", 4),
    ("CleanSpeedBonusPercent", "清扫加速", 5),
    ("AllServiceSpeedBonusPercent", "全服务加速", 6),
    ("StaminaDrainReductionPercent", "体力消耗降低", 7),
    ("CustomerPatienceBonusSeconds", "顾客耐心", 8),
    ("TipChanceBonusPercent", "小费概率", 9),
    ("SolicitationSpeedBonusPercent", "揽客加速", 10),
    ("SatisfactionBonusPercent", "满意度加成", 11),
    ("VipServiceSpeedBonusPercent", "贵客服务加速", 12),
    ("HighPriceDishChanceBonusPercent", "高价菜概率", 13),
    ("WineSaleChanceBonusPercent", "酒水销售概率", 14),
    ("CarryDishCapacitySet", "端菜容量", 15),
    ("OrderBatchCountSet", "批量点单", 16),
    ("AutoCleanNearbyAfterServe", "上菜后顺路清扫", 17),
    ("AutoSupportMostBackloggedTask", "自动支援", 18),
    ("PrioritizeLongestWaitingCustomer", "优先久等顾客", 19),
    ("PrioritizeLowPatienceCustomer", "优先低耐心顾客", 20),
    ("OccupancyEfficiencyBonus", "高客座效率", 21),
    ("AutoCleanAfterCheckout", "结账后清扫", 22),
    ("WaiterPostActionDelayReductionPercent", "动作后摇降低", 23),
    ("Management", "管理", 24),
    ("WaiterAllServiceSpeedBonusPercent", "全员小二全服务加速", 25),
    ("ChefAllCookingSpeedBonusPercent", "全员厨师烹饪加速", 26),
    ("TeamOccupancyEfficiencyBonus", "团队高客座效率", 27),
    ("TeamCleanSpeedBonusPercent", "团队清扫加速", 28),
    ("TeamServeSpeedBonusPercent", "团队上菜加速", 29),
    ("TeamCheckoutSpeedBonusPercent", "团队收账加速", 30),
    ("TeamOrderSpeedBonusPercent", "团队点单加速", 31),
    ("RecruitmentCostReductionPercent", "招聘费用降低", 32),
    ("DailyWageReductionPercent", "日薪支出降低", 33),
    ("VipAttractionBonusPercent", "贵客吸引力", 34),
    ("TavernAllWorkSpeedBonusPercent", "全店工作效率", 35),
    ("ChefPrepSpeedBonusPercent", "厨师备菜加速", 36),
    ("ExtraDishChancePercent", "额外出菜概率", 37),
    ("IngredientCostReductionPercent", "食材消耗降低", 38),
]


def ensure_staff_talent_enum() -> None:
    path = DATAS / "__enums__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "StaffTalentType":
            print(f"StaffTalentType already in {path}")
            return

    start = ws.max_row + 1
    for index, (name, alias, value) in enumerate(STAFF_TALENT_TYPES):
        row = start + index
        ws.cell(row, 2).value = "StaffTalentType" if index == 0 else None
        if index == 0:
            ws.cell(row, 4).value = True
            ws.cell(row, 6).value = "员工天赋效果类型"
        ws.cell(row, 8).value = name
        ws.cell(row, 9).value = alias
        ws.cell(row, 10).value = value

    wb.save(path)
    print(f"patched enum {path}")


def ensure_staff_talent_bean() -> None:
    path = DATAS / "__beans__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "StaffTalent":
            print(f"StaffTalent bean already in {path}")
            return

    fields = [
        ("id", "int", "天赋Id"),
        ("name", "string", "天赋名称"),
        ("desc", "string", "天赋描述"),
        ("position", "StaffPosition", "适用职位"),
        ("talentType", "StaffTalentType", "效果类型"),
        ("param", "(list#sep=,),int", "效果参数"),
        ("icon", "string", "图标Key"),
        ("sortOrder", "int", "排序"),
        ("remark", "string", "备注"),
    ]
    start = ws.max_row + 1
    for index, (field_name, field_type, comment) in enumerate(fields):
        row = start + index
        ws.cell(row, 2).value = "StaffTalent" if index == 0 else None
        if index == 0:
            ws.cell(row, 7).value = "员工天赋"
        ws.cell(row, 10).value = field_name
        ws.cell(row, 12).value = field_type
        ws.cell(row, 14).value = comment
    wb.save(path)
    print(f"patched beans {path}")


def ensure_staff_talent_table() -> None:
    path = DATAS / "__tables__.xlsx"
    wb = load_workbook(path)
    ws = wb.active
    for row in range(1, ws.max_row + 1):
        if ws.cell(row, 2).value == "TbStaffTalent":
            print(f"TbStaffTalent already in {path}")
            return

    row = ws.max_row + 1
    ws.cell(row, 2).value = "TbStaffTalent"
    ws.cell(row, 3).value = "StaffTalent"
    ws.cell(row, 5).value = "StaffTalent.xlsx"
    ws.cell(row, 6).value = "id"
    ws.cell(row, 7).value = "map"
    ws.cell(row, 9).value = "员工天赋"
    wb.save(path)
    print(f"patched tables {path}")


def main() -> None:
    ensure_staff_talent_enum()
    ensure_staff_talent_bean()
    ensure_staff_talent_table()


if __name__ == "__main__":
    main()
