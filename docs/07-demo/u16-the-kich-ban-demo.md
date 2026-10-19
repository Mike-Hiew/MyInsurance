# U16 — Thẻ kịch bản demo 9 bước (dùng chung MyInsurance + ClaimMVP) — 05/10/2026

## Cách dùng (người trình bày)
- Bật: mở bất kỳ trang MyInsurance (hoặc ClaimMVP khi đã gắn) kèm `?demo=1`, ví dụ `https://bnk-dev.outsystems.app/MyInsurance/PortalHome?demo=1`. Khách thường không thấy gì.
- Tắt hẳn: thêm `?demo=0` vào URL. "Thoát" chỉ thu gọn về nút tím và đưa về bước 1.
- Tiếp/Trước hoặc phím → / ← (khi không gõ trong ô nhập). Nút "Mở màn này" khi đang ở màn khác.
- Trạng thái (bước, mở/thu gọn) lưu trong localStorage cùng origin `bnk-dev.outsystems.app` → **dùng chung giữa 2 app**: nhảy từ MyInsurance sang ClaimMVP vẫn giữ bước.
- Câu dẫn không có số minh hoạ; số liệu do màn thật hiển thị.

## 9 bước
| # | App | Màn | Nội dung |
|---|---|---|---|
| 1 | MyInsurance | PortalHome | Khách mở cổng — quyền lợi còn lại, hồ sơ gần nhất |
| 2 | MyInsurance | PortalNewClaim | Nộp bằng ảnh chụp; BnK IDP đọc; khách thấy tiến trình (không cần bấm Gửi) |
| 3 | ClaimMVP | WorkList | Hồ sơ vào hàng đợi đã có số tiền do chuỗi 14 bước |
| 4 | ClaimMVP | ClaimDetail?ClaimId=17 (CLM-2026-0001) | Thẻ "AI đã làm gì" |
| 5 | ClaimMVP | ClaimDetail?ClaimId=17 | Đề xuất AG-3 — người quyết |
| 6 | ClaimMVP | ClaimDetail?ClaimId=440 | Thác tiền (tab Calculation & ledger) |
| 7 | ClaimMVP | ClaimDetail?ClaimId=445 | Cổng tự duyệt 4 điều kiện |
| 8 | MyInsurance | PortalClaimDetail?ClaimId=440&OpenLetters=false | Biên nhận + thư kết quả (viền sáng `.v7-receipt`) |
| 9 | ClaimMVP | AgentTrace?ClaimId=440 | Dấu vết kỹ thuật |

## Trạng thái
- MyInsurance: đã gắn ở rev 120–121 (JS node `V7StoryInit` ở LayoutSideMenu OnReady, tham số Lang = Client.Lang; CSS `#v7-story`, `.v7-story-*`, `.v7-spot` trong theme).
- **ClaimMVP: CHƯA gắn** — phiên ClaimMVP dán đúng JS + CSS dưới đây vào layout chính (OnReady, Lang = Client.Lang) và theme của ClaimMVP; không cần sửa gì khác. Nếu ClaimMVP muốn viền sáng ở bước 4–7 thì điền `spot` (vd `[id$="V7AiWorkCard"]`) cho bước tương ứng ở CẢ HAI app để giữ một nguồn.

## Đã kiểm (rev 121)
- Không có `?demo=1` → không hiện gì (khách thường). `?demo=1` → thẻ bước 1; Tiếp → sang PortalNewClaim bước 2; nhảy bước 8 → mở PortalClaimDetail 0440, viền sáng biên nhận; Thu gọn → nút "✦ Kịch bản demo · 8/9", mở lại được; EN đúng; 390 px: thẻ nằm trên nút chat, không tràn ngang.
- Khi Tiếp sang bước ClaimMVP (3–7, 9): màn ClaimMVP mở đúng nhưng **chưa có thẻ** cho tới khi phiên ClaimMVP gắn; quay lại MyInsurance thẻ vẫn nhớ bước.

## JS (JavaScript node, input Lang: Text = Client.Lang)
```js
window.v7Lang = String($parameters.Lang || 'vi').toLowerCase().indexOf('en') === 0 ? 'en' : 'vi';
try {
  var q = new URLSearchParams(location.search).get('demo');
  if (q === '1') { localStorage.setItem('v7demo', '1'); localStorage.setItem('v7open', '1'); }
  if (q === '0') { localStorage.removeItem('v7demo'); localStorage.removeItem('v7step'); localStorage.removeItem('v7open'); }
} catch (e) {}
if (!window.v7StoryInit) {
  window.v7StoryInit = true;
  var S = [
    { app: 'MyInsurance', url: '/MyInsurance/PortalHome', spot: '',
      vi: ['Khách mở cổng bảo hiểm', '“Anh An thấy ngay quyền lợi còn lại và hồ sơ gần nhất của mình.”', ''],
      en: ['The customer opens the portal', '“An sees his remaining benefits and his latest claim right away.”', ''] },
    { app: 'MyInsurance', url: '/MyInsurance/PortalNewClaim', spot: '',
      vi: ['Nộp hồ sơ chỉ bằng ảnh chụp', '“Khách chỉ chụp chứng từ. BnK IDP phân loại và đọc — khách thấy tiến trình đọc từng chứng từ.”', 'Không cần bấm Gửi khi demo nếu không muốn tạo hồ sơ mới.'],
      en: ['Filing with photos only', '“The customer only photographs the paperwork. BnK IDP classifies and reads it — the customer sees the reading progress.”', 'No need to press Submit in the demo unless you want a new claim.'] },
    { app: 'ClaimMVP', url: '/ClaimMVP/WorkList', spot: '',
      vi: ['Hồ sơ vào hàng đợi cán bộ', '“Hồ sơ đến hàng đợi đã có số tiền do chuỗi 14 bước tính sẵn.”', ''],
      en: ["The claim lands in the officer's queue", '“Claims arrive already priced by the 14-step chain.”', ''] },
    { app: 'ClaimMVP', url: '/ClaimMVP/ClaimDetail?ClaimId=17', spot: '',
      vi: ['AI đã làm gì cho hồ sơ này', '“Mỗi bước ghi rõ ai làm và ra kết quả gì — máy đọc và tính, người quyết.”', 'Thẻ “AI đã làm gì” ở đầu hồ sơ CLM-2026-0001.'],
      en: ['What AI did on this claim', '“Each step names who did it and what came out — machines read and calculate, people decide.”', 'The “What AI did” card at the top of CLM-2026-0001.'] },
    { app: 'ClaimMVP', url: '/ClaimMVP/ClaimDetail?ClaimId=17', spot: '',
      vi: ['AI đề xuất — con người quyết', '“Kiểm tra lâm sàng đề xuất về một dòng thuốc, kèm bằng chứng. Tiền chỉ đổi khi cán bộ bấm xác nhận.”', 'Mở thẻ đề xuất AG-3 trong hồ sơ.'],
      en: ['AI proposes — a person decides', '“The clinical check proposes a change on one medicine line, with evidence. Money only changes when the officer confirms.”', 'Open the AG-3 proposal card on the claim.'] },
    { app: 'ClaimMVP', url: '/ClaimMVP/ClaimDetail?ClaimId=440', spot: '',
      vi: ['Thác tiền của chuỗi 14 bước', '“Mỗi cột là một bước của chuỗi có làm đổi số tiền, kèm lý do bằng lời.”', 'Tab Calculation & ledger của CLM-2026-0440.'],
      en: ['The 14-step chain waterfall', '“Each bar is a chain step that changed the amount, with a plain reason.”', 'Calculation & ledger tab of CLM-2026-0440.'] },
    { app: 'ClaimMVP', url: '/ClaimMVP/ClaimDetail?ClaimId=445', spot: '',
      vi: ['Cổng tự duyệt 4 điều kiện', '“Mỗi điều kiện có lý do bằng lời. Hồ sơ này không qua cổng nên chuyển người duyệt.”', 'Khối cổng tự duyệt của CLM-2026-0445.'],
      en: ['4-condition auto-approval gate', '“Every condition carries a plain reason. This claim fails the gate, so it goes to an approver.”', 'Auto-approval gate block of CLM-2026-0445.'] },
    { app: 'MyInsurance', url: '/MyInsurance/PortalClaimDetail?ClaimId=440&OpenLetters=false', spot: '.v7-receipt',
      vi: ['Khách nhận kết quả rõ ràng', '“Bảo hiểm trả bao nhiêu, Quý khách tự chịu bao nhiêu và vì sao — thư kết quả nằm ngay trong hồ sơ.”', ''],
      en: ['The customer gets a clear result', '“What the insurer pays, what the customer bears and why — the result letter sits right in the claim.”', ''] },
    { app: 'ClaimMVP', url: '/ClaimMVP/AgentTrace?ClaimId=440', spot: '',
      vi: ['Dấu vết kỹ thuật', '“Mọi lượt agent và lần gọi IDP đều được ghi lại — ai gọi công cụ gì, khi nào.”', ''],
      en: ['Technical trace', '“Every agent turn and IDP call is logged — which tool was called, and when.”', ''] }
  ];
  var get = function (k, d) { try { var v = localStorage.getItem(k); return v === null ? d : v; } catch (e) { return d; } };
  var set = function (k, v) { try { localStorage.setItem(k, v); } catch (e) {} };
  var cur = function () { var i = parseInt(get('v7step', '0'), 10) || 0; return (i < 0 || i >= S.length) ? 0 : i; };
  var isHere = function (u) {
    var a = document.createElement('a'); a.href = u;
    if (location.pathname.toLowerCase() !== a.pathname.toLowerCase()) return false;
    var want = new URLSearchParams(a.search), have = new URLSearchParams(location.search), ok = true;
    want.forEach(function (v, k) { if ((have.get(k) || '').toLowerCase() !== v.toLowerCase()) ok = false; });
    return ok;
  };
  var go = function (i) {
    i = Math.max(0, Math.min(S.length - 1, i));
    set('v7step', String(i)); set('v7open', '1');
    if (isHere(S[i].url)) { window.v7StoryRender(); } else { location.href = S[i].url; }
  };
  window.v7StoryRender = function () {
    var host = document.getElementById('v7-story');
    if (get('v7demo', '0') !== '1') { if (host) host.parentNode.removeChild(host); return; }
    if (!host) {
      host = document.createElement('div');
      host.id = 'v7-story';
      document.body.appendChild(host);
      host.addEventListener('click', function (e) {
        var b = e.target.closest ? e.target.closest('[data-a]') : null;
        if (!b || b.disabled) return;
        var a = b.getAttribute('data-a'), i = cur();
        if (a === 'open') { set('v7open', '1'); window.v7StoryRender(); }
        else if (a === 'min') { set('v7open', '0'); window.v7StoryRender(); }
        else if (a === 'exit') { set('v7open', '0'); set('v7step', '0'); window.v7StoryRender(); }
        else if (a === 'prev') { go(i - 1); }
        else if (a === 'here') { go(i); }
        else if (a === 'next') { if (i === S.length - 1) { set('v7open', '0'); set('v7step', '0'); window.v7StoryRender(); } else { go(i + 1); } }
      });
    }
    var en = window.v7Lang === 'en', i = cur(), s = S[i], t = en ? s.en : s.vi, last = S.length - 1;
    if (get('v7open', '0') !== '1') {
      host.innerHTML = '<button type="button" class="v7-story-pill" data-a="open">✦ ' + (en ? 'Demo story' : 'Kịch bản demo') + ' · ' + (i + 1) + '/' + S.length + '</button>';
    } else {
      var dots = '';
      for (var k = 0; k < S.length; k++) { dots += '<i class="' + (k === i ? 'on' : '') + '"></i>'; }
      host.innerHTML =
        '<div class="v7-story-card" role="dialog" aria-label="' + (en ? 'Demo story' : 'Kịch bản demo') + '">' +
          '<div class="v7-story-top"><span class="v7-story-n">' + (i + 1) + '</span><b class="v7-story-h">' + t[0] + '</b>' +
          '<button type="button" class="v7-story-x" data-a="min" aria-label="' + (en ? 'Minimise' : 'Thu gọn') + '">–</button></div>' +
          '<div class="v7-story-say">' + t[1] + '</div>' +
          (t[2] ? '<div class="v7-story-hint">' + t[2] + '</div>' : '') +
          '<div class="v7-story-app">' + s.app + (isHere(s.url) ? '' : (en ? ' · not on this screen yet' : ' · chưa ở màn này')) + '</div>' +
          '<div class="v7-story-dots">' + dots + '</div>' +
          '<div class="v7-story-btns">' +
            '<button type="button" data-a="exit">' + (en ? 'Exit' : 'Thoát') + '</button>' +
            '<button type="button" data-a="prev"' + (i === 0 ? ' disabled' : '') + '>← ' + (en ? 'Back' : 'Trước') + '</button>' +
            (isHere(s.url) ? '' : '<button type="button" data-a="here">' + (en ? 'Open screen' : 'Mở màn này') + '</button>') +
            '<button type="button" class="pri" data-a="next">' + (i === last ? (en ? 'Finish' : 'Kết thúc') : (en ? 'Next' : 'Tiếp') + ' →') + '</button>' +
          '</div>' +
        '</div>';
    }
    var olds = document.querySelectorAll('.v7-spot');
    for (var m = 0; m < olds.length; m++) { olds[m].classList.remove('v7-spot'); }
    if (get('v7open', '0') === '1' && s.spot && isHere(s.url)) {
      var n = 0, tm = setInterval(function () {
        var el = document.querySelector(s.spot); n++;
        if (el || n > 25) { clearInterval(tm); if (el) el.classList.add('v7-spot'); }
      }, 200);
    }
  };
  document.addEventListener('keydown', function (e) {
    if (get('v7demo', '0') !== '1' || get('v7open', '0') !== '1') return;
    var tg = e.target, tn = tg && tg.tagName ? tg.tagName.toLowerCase() : '';
    if (tn === 'input' || tn === 'textarea' || tn === 'select' || (tg && tg.isContentEditable)) return;
    if (e.key === 'ArrowRight') { var i = cur(); if (i < S.length - 1) go(i + 1); }
    if (e.key === 'ArrowLeft') { var j = cur(); if (j > 0) go(j - 1); }
  });
}
window.v7StoryRender();
```

## CSS
```css
#v7-story{position:fixed;left:16px;bottom:16px;z-index:9990;font-size:14px;}
.v7-story-pill{border:0;border-radius:999px;padding:8px 14px;color:#fff;background:linear-gradient(135deg,#6d4aff,#8b5cf6);box-shadow:0 6px 18px rgba(109,74,255,.35);cursor:pointer;font-weight:600;font-size:13px;}
.v7-story-card{width:340px;max-width:calc(100vw - 32px);box-sizing:border-box;background:#fff;color:#0f172a;border:1px solid rgba(109,74,255,.25);border-radius:16px;padding:14px 16px;box-shadow:0 12px 32px rgba(15,23,42,.18);animation:v7storyIn .25s ease-out;}
@keyframes v7storyIn{from{opacity:0;transform:translateY(10px);}to{opacity:1;transform:none;}}
.v7-story-top{display:flex;align-items:center;gap:10px;}
.v7-story-n{flex:none;width:26px;height:26px;border-radius:50%;display:grid;place-items:center;color:#fff;font-weight:700;font-size:13px;background:linear-gradient(135deg,#6d4aff,#8b5cf6);}
.v7-story-h{flex:1;font-size:15px;line-height:1.3;}
.v7-story-x{border:0;background:transparent;font-size:18px;line-height:1;cursor:pointer;color:#64748b;padding:2px 6px;}
.v7-story-say{margin:10px 0 6px;font-style:italic;line-height:1.45;}
.v7-story-hint{font-size:13px;color:#475569;background:#f5f3ff;border-radius:8px;padding:6px 8px;margin-bottom:6px;}
.v7-story-app{font-size:12px;color:#64748b;margin-bottom:8px;}
.v7-story-dots{display:flex;gap:5px;margin-bottom:10px;}
.v7-story-dots i{width:6px;height:6px;border-radius:3px;background:#e2e8f0;}
.v7-story-dots i.on{background:#6d4aff;width:16px;}
.v7-story-btns{display:flex;gap:6px;justify-content:flex-end;flex-wrap:wrap;}
.v7-story-btns button{border:1px solid #e2e8f0;background:#fff;border-radius:8px;padding:6px 10px;font-size:13px;cursor:pointer;color:#0f172a;}
.v7-story-btns button[disabled]{opacity:.4;cursor:default;}
.v7-story-btns button.pri{border:0;color:#fff;background:linear-gradient(135deg,#6d4aff,#8b5cf6);}
.v7-spot{outline:3px solid rgba(124,92,255,.55);outline-offset:4px;border-radius:12px;}
@media (max-width:600px){#v7-story{left:12px;right:12px;bottom:88px;}.v7-story-card{width:auto;max-width:none;}}
@media (prefers-reduced-motion: reduce){.v7-story-card{animation:none;}}
```
