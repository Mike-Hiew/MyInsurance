# -*- coding: utf-8 -*-
"""Sinh bộ chứng từ D2/D3 (HTML -> PNG bằng Chromium), bám mẫu đã chạy được với BnK IDP."""
import asyncio, json, os, shutil, sys
from playwright.async_api import async_playwright

OUT = sys.argv[1] if len(sys.argv) > 1 else "out"

PATIENT = dict(name="NGUYỄN VĂN AN", sex="Nam", card="MB-2026-0001", dob="14/05/1990",
               addr="Số 12 Đường Thử Nghiệm, Phường Demo, TP. Hà Nội", cccd="000090000001")
VM = "BỆNH VIỆN ĐA KHOA QUỐC TẾ VINMEC"
FOOT = "MẪU MINH HOẠ CHO KIỂM THỬ — KHÔNG CÓ GIÁ TRỊ THANH TOÁN"
FOOT2 = "CHỨNG TỪ GIẢ LẬP DÙNG ĐỂ KIỂM THỬ — KHÔNG CÓ GIÁ TRỊ PHÁP LÝ"

def dots(n):
    return f"{n:,}".replace(",", ".")

def ddmmyyyy_words(d):
    dd, mm, yy = d.split("/")
    return f"Hà Nội, ngày {dd} tháng {mm} năm {yy}"

BASE_CSS = """
*{box-sizing:border-box} body{margin:0;font-family:'DejaVu Sans';color:#000;background:#fff}
.pg{position:relative;overflow:hidden;background:#fff}
b{font-weight:bold}
table{border-collapse:collapse}
.foot{position:absolute;left:34px;bottom:30px;font-weight:bold;color:#9a9a9a}
"""

# ---------- Bảng kê ngoại/nội trú 13 cột (mẫu 01/KBCB) — A4 ngang ----------
def bang_ke_13(d):
    rows = ""
    tot = 0
    for (name, unit, qty, price) in d["lines"]:
        amt = qty * price; tot += amt
        rows += (f"<tr><td class=l>{name}</td><td>{unit}</td><td>{qty}</td><td>{price}</td><td>0</td><td>100</td>"
                 f"<td>{amt}</td><td>0</td><td>0</td><td>0</td><td>0</td><td>0</td><td>{amt}</td></tr>")
    assert tot == d["total"], (d["id"], tot, d["total"])
    rows += (f"<tr class=t><td class=l>Tổng cộng</td><td></td><td></td><td></td><td></td><td></td>"
             f"<td>{tot}</td><td></td><td>0</td><td>0</td><td>0</td><td>0</td><td>{tot}</td></tr>")
    left2 = d.get("left2") or [("Ngày khám", d["date"]), ("Chẩn đoán", d["dx"])]
    right2 = d.get("right2") or [("Ngày kết thúc điều trị", d["date_end"]), ("Mã bệnh (ICD-10)", d["icd"])]
    info = ""
    L = [("Họ tên người bệnh", PATIENT["name"]), ("Mã thẻ bảo hiểm", PATIENT["card"])] + left2
    R = [("Giới tính", PATIENT["sex"]), ("Nơi đăng ký KCB", VM)] + right2
    for (a, b_), (c, e) in zip(L, R):
        info += f"<div class=r><span>{a}: {b_}</span><span>{c}: {e}</span></div>"
    html = f"""<html><head><meta charset=utf-8><style>{BASE_CSS}
.pg{{width:1169px;height:827px;padding:26px 34px}}
.hd{{display:flex;justify-content:space-between;font-size:10.5px;line-height:16px}}
.hd .o b{{font-size:11px}}
h1{{text-align:center;font-size:17px;margin:20px 0 0}} .sub{{text-align:center;font-size:11px;margin:4px 0 12px}}
.r{{display:flex;font-size:11px;line-height:20px}} .r span{{flex:0 0 50%}}
table{{width:100%;margin-top:12px;font-size:9px}}
th{{border:1px solid #000;font-size:8px;padding:4px 3px;height:48px;font-weight:bold}}
td{{border:1px solid #000;text-align:right;padding:0 5px;height:30px}} td.l{{text-align:left}}
tr.t td{{font-weight:bold}}
.sum{{font-size:11px;line-height:19px;margin-top:10px}} .sum .big{{font-weight:bold;font-size:11.5px}}
.sg{{display:flex;justify-content:space-around;margin-top:22px;font-size:11px;text-align:center}} .sg i{{font-style:normal;font-size:8.5px;display:block;margin-top:6px}}
.foot{{font-size:10px}}</style></head><body><div class=pg>
<div class=hd><div class=o><b>SỞ Y TẾ HÀ NỘI</b><br><b>{VM}</b><br>Khoa: {d['khoa']}</div>
<div>Mẫu số: 01/KBCB<br>Mã số người bệnh: {d['pid']}<br>Số bảng kê: {d['bk']}</div></div>
<h1>BẢNG KÊ CHI PHÍ KHÁM BỆNH, CHỮA BỆNH</h1><div class=sub>({d['kind']})</div>
{info}
<table style="table-layout:fixed"><colgroup><col style="width:19%"><col style="width:5.4%"><col style="width:5%"><col style="width:7.7%"><col style="width:6.8%"><col style="width:7.3%"><col style="width:8.2%"><col style="width:7.3%"><col style="width:6.8%"><col style="width:6.4%"><col style="width:7.3%"><col style="width:5%"><col style="width:7.8%"></colgroup><tr><th>Nội dung</th><th>Đơn vị tính</th><th>Số lượng</th><th>Đơn giá BV</th><th>Đơn giá BH</th>
<th>Tỷ lệ thanh toán theo dịch vụ (%)</th><th>Thành tiền BV</th><th>Tỷ lệ thanh toán BHYT (%)</th><th>Thành tiền BH</th>
<th>Quỹ BHYT</th><th>Người bệnh cùng chi trả</th><th>Khác</th><th>Người bệnh tự trả</th></tr>{rows}</table>
<div class=sum><div class=big>Tổng chi phí khám bệnh, chữa bệnh: {tot} đồng</div>
<div>Số tiền quỹ BHYT thanh toán: 0 đồng &nbsp;&nbsp;&nbsp;&nbsp; Số tiền người bệnh trả: {tot} đồng</div>
<div>(Bằng chữ: {d['words']})</div></div>
<div class=sg><div><b>NGƯỜI LẬP BẢNG KÊ</b><i>(Ký, ghi rõ họ tên)</i></div><div><b>KẾ TOÁN VIỆN PHÍ</b><i>(Ký, ghi rõ họ tên)</i></div><div><b>XÁC NHẬN CỦA NGƯỜI BỆNH</b><i>(Ký, ghi rõ họ tên)</i></div></div>
<div class=foot>{FOOT}</div></div></body></html>"""
    return html, (1169, 827, 2)

# ---------- Phiếu khám bệnh (mẫu ca 1) — A4 dọc ----------
def phieu_kham(d):
    items = "".join(f"<p><b>{k}:</b> {v}</p>" for k, v in d["fields"])
    html = f"""<html><head><meta charset=utf-8><style>{BASE_CSS}
.pg{{width:827px;height:1169px;padding:30px 34px}}
.hd{{display:flex;justify-content:space-between;font-size:10.5px;line-height:18px}}
h1{{text-align:center;font-size:19px;margin:34px 0 26px}}
.f{{padding:0 9px}} .f p{{font-size:11.3px;margin:0 0 13.5px}}
.sg{{margin:40px 0 0 433px;font-size:11px;line-height:22px}} .sg i{{font-style:normal;font-size:9.5px}}
.foot{{font-size:10px}}</style></head><body><div class=pg>
<div class=hd><div><b>SỞ Y TẾ HÀ NỘI</b><br><b>{VM}</b></div><div style="margin-right:85px">Số: {d['so']}</div></div>
<h1>PHIẾU KHÁM BỆNH</h1><div class=f>
<p><b>Họ và tên người bệnh:</b> {PATIENT['name']}</p><p><b>Giới tính:</b> {PATIENT['sex']}</p>
<p><b>Mã thẻ bảo hiểm:</b> {PATIENT['card']}</p><p><b>Ngày khám:</b> {d['date']}</p>{items}</div>
<div class=sg>{ddmmyyyy_words(d['date'])}<br><span style="margin-left:34px"><b>BÁC SĨ KHÁM BỆNH</b></span><br>
<span style="margin-left:34px"><i>(Ký, ghi rõ họ tên)</i></span><br><br><br><span style="margin-left:34px">{d['doctor']}</span></div>
<div class=foot>{FOOT}</div></div></body></html>"""
    return html, (827, 1169, 2)

# ---------- Kiểu đơn giản (đơn thuốc, giấy ra viện) — A4 dọc 150dpi ----------
def simple(title, head2, fields, table_head, table_rows, after, sign_title, sign_name, date):
    f = "".join(f"<p><b>{k}:</b> {v}</p>" for k, v in fields)
    th = "".join(f"<th>{h}</th>" for h in table_head)
    tr = "".join("<tr>" + "".join(f"<td>{c}</td>" for c in r) + "</tr>" for r in table_rows)
    html = f"""<html><head><meta charset=utf-8><style>{BASE_CSS}
.pg{{width:827px;height:1169px;padding:42px 47px}}
.org{{font-size:14px;font-weight:bold}} .o2{{font-size:11.5px;margin-top:4px}}
h1{{text-align:center;font-size:21px;margin:22px 0 22px}}
p{{font-size:12.5px;margin:0 0 4.5px}}
table{{width:100%;margin-top:10px;font-size:11.5px}} th,td{{border:1px solid #000;text-align:left;padding:5px 4px;height:29px}}
.aft{{font-size:12.5px;margin-top:12px}} .aft div{{margin-left:16px;margin-top:4px}}
.sg{{position:absolute;right:47px;top:942px;font-size:12.5px;width:245px}} .sg .n{{margin-top:66px}}
.sg i{{font-size:10px}}
.foot{{left:47px;bottom:34px;font-style:italic;font-weight:normal;color:#000;font-size:10.5px}}</style></head><body><div class=pg>
<div class=org>{VM}</div><div class=o2>{head2}</div><h1>{title}</h1>{f}
<table><tr>{th}</tr>{tr}</table>{after}
<div class=sg><div style="text-align:right;width:360px;margin-left:-115px">{ddmmyyyy_words(date)}</div>
<div style="margin-top:22px"><b>{sign_title}</b><br><i>(Ký, ghi rõ họ tên)</i></div><div class=n>{sign_name}</div></div>
<div class=foot>{FOOT2}</div></div></body></html>"""
    return html, (827, 1169, 1.5)

def don_thuoc(d):
    rows = [[i + 1, n, u, q, c] for i, (n, u, q, c) in enumerate(d["drugs"])]
    after = f"<div class=aft><b>Lời dặn của bác sĩ:</b><div>{d['advice']}</div></div>"
    fields = [("Số đơn thuốc", d["so"]), ("Họ và tên", PATIENT["name"]), ("Ngày sinh", PATIENT["dob"]),
              ("Giới tính", PATIENT["sex"]), ("Địa chỉ", PATIENT["addr"]), ("Mã thẻ bảo hiểm", PATIENT["card"]),
              ("Chẩn đoán", d["dx_full"]), ("Khoa", d["khoa"])]
    return simple("ĐƠN THUỐC", "Mã cơ sở KCB: 01-VMC", fields, ["STT", "Tên thuốc, hàm lượng", "ĐVT", "SL", "Cách dùng"],
                  rows, after, "Bác sĩ kê đơn", d["doctor"], d["date"])

def giay_ra_vien(d):
    rows = [[n, u, q, dots(p), dots(q * p), "0"] for (n, u, q, p) in d["groups"]]
    after = f"<div class=aft><b>Lời dặn của bác sĩ:</b><div>{d['advice']}</div></div>"
    fields = [("Họ và tên", PATIENT["name"]), ("Tuổi", "36 — Giới tính: Nam"), ("Ngày sinh", PATIENT["dob"]),
              ("Địa chỉ", PATIENT["addr"]), ("Mã thẻ bảo hiểm", PATIENT["card"]), ("Khoa phòng", d["khoa"]),
              ("Vào viện lúc", d["in"]), ("Ra viện lúc", d["out"]), ("Chẩn đoán", d["dx_full"]),
              ("Phương pháp điều trị", d["method"])]
    html, size = simple("GIẤY RA VIỆN", f"Số vào viện: {d['so']}", fields, [], [], after,
                        "Trưởng khoa", d["doctor"], d["sign_date"])
    html = html.replace("<table><tr></tr></table>", "")
    return html, size

# ------------------------------ DỮ LIỆU ------------------------------
def ca1(date, so, pid, bk):
    lines = [("Khám bệnh", "Lần", 1, 200000), ("Xét nghiệm nhanh liên cầu", "Lần", 1, 150000),
             ("Paracetamol 500mg", "Viên", 10, 5000)]
    return {
        "01_phieu-kham-benh.png": phieu_kham(dict(so=so, date=date, doctor="BS. Trần Minh Đức", fields=[
            ("Lý do đến khám", "Đau họng, sốt nhẹ 2 ngày"),
            ("Khám lâm sàng", "Họng đỏ, amidan sưng nhẹ, không có giả mạc. Nhiệt độ 37,8°C."),
            ("Cận lâm sàng", "Xét nghiệm nhanh liên cầu: âm tính"), ("Chẩn đoán", "Viêm họng cấp"),
            ("Mã bệnh (ICD-10)", "J02.9"), ("Hướng điều trị", "Paracetamol 500mg x 10 viên, uống khi sốt; súc họng nước muối"),
            ("Hẹn tái khám", "Sau 5 ngày nếu không đỡ")])),
        "02_bang-ke-chi-phi.png": bang_ke_13(dict(id="ca1", khoa="Khám bệnh", pid=pid, bk=bk, kind="Ngoại trú",
            date=date, date_end=date, dx="Viêm họng cấp", icd="J02.9", lines=lines, total=400000,
            words="Bốn trăm nghìn đồng")),
    }

def ca2(date, so, pid, bk):
    lines = [("Khám răng hàm mặt", "Lần", 1, 150000), ("Chụp phim răng toàn cảnh", "Lần", 1, 250000),
             ("Điều trị tuỷ răng 36", "Răng", 1, 1800000), ("Trám composite răng 36", "Răng", 1, 500000),
             ("Lấy cao răng 2 hàm", "Lần", 1, 300000), ("Tẩy trắng răng tại phòng khám", "Lần", 1, 3500000)]
    return {
        "01_phieu-kham-benh.png": phieu_kham(dict(so=so, date=date, doctor="BS. Vũ Thị Hà", fields=[
            ("Lý do đến khám", "Đau nhức răng hàm dưới bên trái 3 ngày, chảy máu lợi khi chải răng"),
            ("Khám lâm sàng", "Răng 36 sâu lớn mặt nhai, gõ đau, thử tuỷ dương tính kéo dài. Lợi viền đỏ, cao răng nhiều."),
            ("Cận lâm sàng", "Phim răng toàn cảnh: tổn thương sâu sát tuỷ răng 36"),
            ("Chẩn đoán", "Viêm tuỷ răng 36; Viêm lợi"), ("Mã bệnh (ICD-10)", "K04.0; K05.1"),
            ("Hướng điều trị", "Điều trị tuỷ và trám composite răng 36; lấy cao răng 2 hàm. Tẩy trắng răng theo yêu cầu của người bệnh"),
            ("Hẹn tái khám", "Sau 7 ngày")])),
        "02_bang-ke-chi-phi.png": bang_ke_13(dict(id="ca2", khoa="Răng hàm mặt", pid=pid, bk=bk, kind="Ngoại trú",
            date=date, date_end=date, dx="Viêm tuỷ răng 36; Viêm lợi", icd="K04.0; K05.1", lines=lines,
            total=6500000, words="Sáu triệu năm trăm nghìn đồng")),
    }

def ca3(date, so, pid, bk, dt):
    lines = [("Khám tai mũi họng", "Lần", 1, 300000), ("Nội soi tai mũi họng", "Lần", 1, 250000),
             ("Fexofenadin 180mg", "Viên", 10, 6500), ("Mometason xịt mũi 50mcg", "Lọ", 1, 185000),
             ("Natri clorid 0,9% rửa mũi", "Chai", 2, 15000), ("Glucosamin 1500mg", "Viên", 30, 12000),
             ("Collagen + Vitamin C (TPCN)", "Hộp", 1, 650000), ("Vitamin tổng hợp", "Viên", 30, 5000)]
    return {
        "01_phieu-kham-benh.png": phieu_kham(dict(so=so, date=date, doctor="BS. Ngô Thị Lan", fields=[
            ("Lý do đến khám", "Hắt hơi, ngứa mũi, chảy nước mũi trong 2 tuần"),
            ("Khám lâm sàng", "Niêm mạc mũi nhợt, phù nề cuốn dưới hai bên, dịch mũi trong."),
            ("Cận lâm sàng", "Nội soi tai mũi họng: viêm mũi dị ứng, không polyp"),
            ("Chẩn đoán", "Viêm mũi dị ứng"), ("Mã bệnh (ICD-10)", "J30.4"),
            ("Hướng điều trị", "Fexofenadin, Mometason xịt mũi, rửa mũi nước muối. Tránh bụi, lông thú"),
            ("Hẹn tái khám", "Sau 10 ngày")])),
        "02_bang-ke-chi-phi.png": bang_ke_13(dict(id="ca3", khoa="Tai mũi họng", pid=pid, bk=bk, kind="Ngoại trú",
            date=date, date_end=date, dx="Viêm mũi dị ứng", icd="J30.4", lines=lines, total=1990000,
            words="Một triệu chín trăm chín mươi nghìn đồng")),
        "03_don-thuoc.png": don_thuoc(dict(so=dt, date=date, dx_full="Viêm mũi dị ứng — Mã ICD: J30.4",
            khoa="Tai mũi họng", doctor="BS. Ngô Thị Lan", advice="Tránh bụi, lông thú, khói thuốc. Tái khám sau 10 ngày.",
            drugs=[("Fexofenadin 180mg", "Viên", 10, "Uống 1 viên buổi tối"),
                   ("Mometason xịt mũi 50mcg", "Lọ", 1, "Xịt mỗi bên mũi 2 nhát buổi sáng"),
                   ("Natri clorid 0,9% rửa mũi", "Chai", 2, "Rửa mũi sáng, tối"),
                   ("Glucosamin 1500mg", "Viên", 30, "Uống 1 viên/ngày sau ăn"),
                   ("Collagen + Vitamin C (TPCN)", "Hộp", 1, "Uống 1 gói/ngày"),
                   ("Vitamin tổng hợp", "Viên", 30, "Uống 1 viên/ngày")])),
    }

def kho1():  # bệnh có trước khi mua hợp đồng
    date = "07/10/2026"
    lines = [("Khám nội tiết", "Lần", 1, 250000), ("Định lượng HbA1c", "Lần", 1, 180000),
             ("Định lượng Glucose máu", "Lần", 1, 50000), ("Metformin 850mg", "Viên", 60, 2500)]
    return {
        "01_phieu-kham-benh.png": phieu_kham(dict(so="PK-261007-033", date=date, doctor="BS. Đặng Quốc Huy", fields=[
            ("Lý do đến khám", "Tái khám định kỳ bệnh đái tháo đường, lĩnh thuốc"),
            ("Tiền sử", "Đái tháo đường type 2 phát hiện năm 2019, điều trị Metformin liên tục từ đó"),
            ("Khám lâm sàng", "Tỉnh, thể trạng trung bình, BMI 26. Không phù. Huyết áp 130/80 mmHg."),
            ("Cận lâm sàng", "HbA1c 7,4%; Glucose máu lúc đói 8,1 mmol/l"),
            ("Chẩn đoán", "Đái tháo đường type 2 không biến chứng (phát hiện năm 2019, đang điều trị)"), ("Mã bệnh (ICD-10)", "E11.9"),
            ("Hướng điều trị", "Tiếp tục Metformin 850mg x 2 viên/ngày; chế độ ăn, vận động"),
            ("Hẹn tái khám", "Sau 1 tháng")])),
        "02_bang-ke-chi-phi.png": bang_ke_13(dict(id="kho1", khoa="Nội tiết", pid="VM-261007", bk="BK-2026-100733",
            kind="Ngoại trú", date=date, date_end=date, dx="Đái tháo đường type 2 (phát hiện năm 2019)", icd="E11.9",
            lines=lines, total=630000, words="Sáu trăm ba mươi nghìn đồng")),
    }

def kho2():  # MRI với chẩn đoán viêm họng
    date = "03/10/2026"
    lines = [("Khám bệnh", "Lần", 1, 200000), ("Chụp cộng hưởng từ (MRI) sọ não", "Lần", 1, 2800000),
             ("Paracetamol 500mg", "Viên", 10, 5000)]
    return {
        "01_phieu-kham-benh.png": phieu_kham(dict(so="PK-261003-052", date=date, doctor="BS. Phạm Thu Trang", fields=[
            ("Lý do đến khám", "Đau họng, nuốt vướng 3 ngày, sốt nhẹ"),
            ("Khám lâm sàng", "Họng đỏ, amidan hai bên sưng nhẹ. Không dấu hiệu thần kinh khu trú. Nhiệt độ 37,6°C."),
            ("Cận lâm sàng", "Chụp MRI sọ não theo yêu cầu người bệnh: không thấy bất thường"),
            ("Chẩn đoán", "Viêm họng cấp"), ("Mã bệnh (ICD-10)", "J02.9"),
            ("Hướng điều trị", "Paracetamol 500mg x 10 viên khi sốt; súc họng nước muối"),
            ("Hẹn tái khám", "Sau 5 ngày nếu không đỡ")])),
        "02_bang-ke-chi-phi.png": bang_ke_13(dict(id="kho2", khoa="Khám bệnh", pid="VM-261003", bk="BK-2026-100352",
            kind="Ngoại trú", date=date, date_end=date, dx="Viêm họng cấp", icd="J02.9", lines=lines,
            total=3050000, words="Ba triệu không trăm năm mươi nghìn đồng")),
    }

def kho3():  # nội trú lệch ngày: bảng kê 28/09–30/09 nhưng giường 4 ngày; đơn thuốc + giấy ra viện ngày 02/10
    lines = [("Khám bệnh", "Lần", 1, 150000), ("Giường Nội khoa phòng 2 người", "Ngày", 4, 650000),
             ("Công thức máu 18 thông số", "Lần", 2, 150000), ("Chụp X-quang ngực thẳng", "Lần", 1, 200000),
             ("Ceftriaxon 1g", "Lọ", 8, 45000), ("Natri clorid 0,9% 500ml", "Chai", 6, 12000)]
    return {
        "01_bang-ke-chi-phi-noi-tru.png": bang_ke_13(dict(id="kho3", khoa="Nội hô hấp", pid="VM-260928",
            bk="BK-2026-092841", kind="Nội trú", lines=lines, total=3682000,
            words="Ba triệu sáu trăm tám mươi hai nghìn đồng",
            left2=[("Ngày vào viện", "28/09/2026"), ("Chẩn đoán", "Viêm phổi")],
            right2=[("Ngày ra viện", "30/09/2026"), ("Mã bệnh (ICD-10)", "J18.9")])),
        "02_don-thuoc-ra-vien.png": don_thuoc(dict(so="DT-2026-100214", date="02/10/2026",
            dx_full="Viêm phổi — Mã ICD: J18.9", khoa="Nội hô hấp", doctor="BS. Lê Văn Hùng",
            advice="Uống thuốc đủ 5 ngày, tái khám sau 7 ngày.",
            drugs=[("Amoxicillin + Acid clavulanic 875/125mg", "Viên", 10, "Uống 1 viên x 2 lần/ngày"),
                   ("Acetylcystein 200mg", "Gói", 10, "Uống 1 gói x 2 lần/ngày")])),
        "03_giay-ra-vien.png": giay_ra_vien(dict(so="VV-2026-9281", khoa="Nội hô hấp",
            **{"in": "08:10 ngày 28/09/2026", "out": "09:30 ngày 02/10/2026"},
            dx_full="Viêm phổi — Mã ICD: J18.9", method="Kháng sinh đường tĩnh mạch, bù dịch",
            groups=[("Giường bệnh", "Ngày", 4, 650000), ("Khám, xét nghiệm, chẩn đoán hình ảnh", "Đợt", 1, 650000),
                    ("Thuốc, dịch truyền", "Đợt", 1, 432000)],
            advice="Uống thuốc đủ 05 ngày tiếp theo. Tái khám sau 07 ngày.", doctor="BS. Lê Văn Hùng",
            sign_date="02/10/2026")),
    }

SETS = {
    "D2_ca1_tu-duyet/A_dien-tap": ca1("06/10/2026", "PK-261006-014", "VM-261006", "BK-2026-100614"),
    "D2_ca1_tu-duyet/B_buoi-that": ca1("10/10/2026", "PK-261010-027", "VM-261010", "BK-2026-101027"),
    "D2_ca2_nha-khoa/A_dien-tap": ca2("05/10/2026", "PK-261005-041", "VM-261005", "BK-2026-100541"),
    "D2_ca2_nha-khoa/B_buoi-that": ca2("09/10/2026", "PK-261009-008", "VM-261009", "BK-2026-100908"),
    "D2_ca3_thuoc-khong-lien-quan/A_dien-tap": ca3("04/10/2026", "PK-261004-019", "VM-261004", "BK-2026-100419", "DT-2026-100419"),
    "D2_ca3_thuoc-khong-lien-quan/B_buoi-that": ca3("08/10/2026", "PK-261008-036", "VM-261008", "BK-2026-100836", "DT-2026-100836"),
    "D3_kho1_benh-co-truoc-hop-dong": kho1(),
    "D3_kho2_MRI-viem-hong": kho2(),
    "D3_kho3_noi-tru-lech-ngay": kho3(),
}

async def main():
    async with async_playwright() as p:
        br = await p.chromium.launch()
        for folder, files in SETS.items():
            os.makedirs(os.path.join(OUT, folder), exist_ok=True)
            for fn, (html, (w, h, sc)) in files.items():
                pg = await br.new_page(viewport={"width": w, "height": h}, device_scale_factor=sc)
                await pg.set_content(html)
                await pg.screenshot(path=os.path.join(OUT, folder, fn), full_page=False)
                await pg.close()
        await br.close()
    # ca trùng = bản sao y hệt ca 1 (cùng bộ A / B)
    for s in ("A_dien-tap", "B_buoi-that"):
        src = os.path.join(OUT, "D2_ca1_tu-duyet", s); dst = os.path.join(OUT, "D2_ca-trung", s)
        os.makedirs(dst, exist_ok=True)
        for fn in os.listdir(src):
            shutil.copy(os.path.join(src, fn), os.path.join(dst, fn))
    print("ok")

asyncio.run(main())
