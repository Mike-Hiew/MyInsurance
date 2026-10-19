# request-response-json — mục lục

Mỗi file là một endpoint: request đã gửi và response nhận về của từng vai, **dữ liệu thật** gọi ngày 03/09/2026 lên môi trường staging. Token và mật khẩu đã thay bằng `<token>` / `<redacted>`.

Tên file đọc theo dạng `METHOD__đường-dẫn.json`. Thư mục `00-` là chuỗi gọi end-to-end theo thứ tự thời gian.

| Thư mục | Số file |
|---|---|
| `00-chuoi-end-to-end/` | 37 |
| `01-xac-thuc-va-danh-tinh/` | 20 |
| `02-template-va-bo-chung-tu/` | 16 |
| `03-anh-xa-mbal/` | 10 |
| `04-nop-chung-tu-va-trich-xuat/` | 19 |
| `05-danh-gia/` | 14 |
| `06-master-data/` | 32 |
| `07-van-hanh/` | 1 |

**Tổng: 149 file.**

## `00-chuoi-end-to-end/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `01__POST__auth_login.json` | `POST /auth/login` | — `201` | A1 — lấy access_token. Body và token đều được che trước khi ghi ra đĩa. |
| `02__GET__user_me.json` | `GET /user/me` | — `200` | Xem role và tenantId của tài khoản đang dùng |
| `03__GET__template_compact.json` | `GET /template/compact` | — `200` | A2 — danh sách template, để lấy templateId nộp kèm chứng từ |
| `04__GET__template_document-set.json` | `GET /template/document-set` | — `200` | A2 — document set: đây là thứ gần nhất với templateSetCode của D3 |
| `05__GET__mbal_docset-map.json` | `GET /mbal/docset-map` | — `200` | Bảng claim_type + document_type -> bộ chứng từ |
| `06__POST__request_group.json` | `POST /request/group` | — `201` | B1 — gom chứng từ của một claim vào một nhóm |
| `07__POST__request.json` | `POST /request` | — `201` | B2 — nộp chứng từ. Lưu ý IDP nhận BYTES; D3 I-07 muốn nộp theo tham chiếu. |
| `08__GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | — `200` | C2 — poll trạng thái cả nhóm. status: 0 gửi, 1 xử lý, 2 xong, 3 lỗi |
| `09__GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | — `200` | C2 — poll trạng thái cả nhóm. status: 0 gửi, 1 xử lý, 2 xong, 3 lỗi |
| `10__GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | — `200` | C2 — poll trạng thái cả nhóm. status: 0 gửi, 1 xử lý, 2 xong, 3 lỗi |
| `11__GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | — `200` | C2 — poll trạng thái cả nhóm. status: 0 gửi, 1 xử lý, 2 xong, 3 lỗi |
| `12__GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | — `200` | C2 — poll trạng thái cả nhóm. status: 0 gửi, 1 xử lý, 2 xong, 3 lỗi |
| `13__GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | — `200` | C2 — poll trạng thái cả nhóm. status: 0 gửi, 1 xử lý, 2 xong, 3 lỗi |
| `14__GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | — `200` | C2 — poll trạng thái cả nhóm. status: 0 gửi, 1 xử lý, 2 xong, 3 lỗi |
| `15__GET__workflow_forwarder_api_v1_flows.json` | `GET /workflow/forwarder/api/v1/flows` | — `200` | Lấy workflowId — D3 dùng nó thay cho knowledgeBaseVersion |
| `21__GET__request_21376_check-status.json` | `GET /request/21376/check-status` | — `200` | C2 — trạng thái một request đơn lẻ |
| `22__GET__request_21376_result.json` | `GET /request/21376/result` | — `200` | C3 — payload để map sang A4 §8 |
| `23__GET__request_21376_get-ocr.json` | `GET /request/21376/get-ocr` | — `200` | C3 — bản kết quả dạng khác |
| `24__GET__request_21376_check-workflow-request.json` | `GET /request/21376/check-workflow-request` | — `200` | Kiểm tra request đã gắn workflow nào chưa |
| `25__PUT__workflow_request.json` | `PUT /workflow/request` | — `200` | D1 — nộp yêu cầu đánh giá (I-15). Đường duy nhất, nằm dưới /workflow |
| `31__GET__workflow_run-workflow-request_1402.json` | `GET /workflow/run-workflow-request/1402` | — `400` |  |
| `32__GET__workflow_request_1402.json` | `GET /workflow/request/1402` | — `200` | D3 — poll tới status 3 (Finished) hoặc 4 (Error) |
| `33__GET__workflow_request_1402.json` | `GET /workflow/request/1402` | — `200` | D3 — poll tới status 3 (Finished) hoặc 4 (Error) |
| `34__GET__workflow_request_1402.json` | `GET /workflow/request/1402` | — `200` | D3 — poll tới status 3 (Finished) hoặc 4 (Error) |
| `35__GET__workflow_request_1402.json` | `GET /workflow/request/1402` | — `200` | D3 — poll tới status 3 (Finished) hoặc 4 (Error) |
| `36__GET__workflow_request_1402.json` | `GET /workflow/request/1402` | — `200` | D3 — poll tới status 3 (Finished) hoặc 4 (Error) |
| `37__GET__workflow_request_1402_result_json.json` | `GET /workflow/request/1402/result/json` | — `400` | D4 — payload đối chiếu I-16 |
| `41__GET__workflow_get-workflow-components.json` | `GET /workflow/get-workflow-components?flowId=8d873709-4cac-4f26-82f1-b1f9d28c6e73` | — `200` | Xem flow có component OCRResult không |
| `42__PUT__workflow_request.json` | `PUT /workflow/request` | — `200` |  |
| `43__GET__workflow_run-workflow-request_1403.json` | `GET /workflow/run-workflow-request/1403` | — `200` | Kích hoạt chạy |
| `44__GET__workflow_request_1403.json` | `GET /workflow/request/1403` | — `200` |  |
| `45__GET__workflow_request_1403.json` | `GET /workflow/request/1403` | — `200` |  |
| `46__GET__workflow_request_1403.json` | `GET /workflow/request/1403` | — `200` |  |
| `47__GET__workflow_request_1403.json` | `GET /workflow/request/1403` | — `200` |  |
| `48__GET__workflow_request_1403_result_json.json` | `GET /workflow/request/1403/result/json` | — `400` | Payload đối chiếu I-16 |
| `49__GET__workflow_request_1403_result_json.json` | `GET /workflow/request/1403/result/json` | — `200` | D4 — payload đối chiếu I-16 (flow Workflow Claim 25/08) |
| `50__GET__workflow_request_1403.json` | `GET /workflow/request/1403` | — `200` | D3 — bản ghi cuối cùng |

## `01-xac-thuc-va-danh-tinh/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `GET__encryption_public-key.json` | `GET /encryption/public-key` | FO Admin `200` · FO EndUser `200` |  |
| `GET__partner_permissions.json` | `GET /partner/permissions` | FO Admin `200` · FO EndUser `200` |  |
| `GET__root.json` | `GET /` | FO Admin `200` · FO EndUser `200` | Health check. |
| `GET__tenant.json` | `GET /tenant` | FO Admin `200` · FO EndUser `403` |  |
| `GET__tenant_176.json` | `GET /tenant/176` | FO Admin `200` · FO EndUser `403` |  |
| `GET__tenant_assigned-tenant.json` | `GET /tenant/assigned-tenant` | FO Admin `200` · FO EndUser `403` |  |
| `GET__tenant_compact.json` | `GET /tenant/compact` | FO Admin `200` · FO EndUser `403` |  |
| `GET__tenant_export-excel-usage-detail.json` | `GET /tenant/export-excel-usage-detail` | FO Admin `200` · FO EndUser `200` |  |
| `GET__tenant_export-pdf-usage-detail.json` | `GET /tenant/export-pdf-usage-detail` | FO Admin `200` · FO EndUser `200` |  |
| `GET__tenant_my-tenant.json` | `GET /tenant/my-tenant` | FO Admin `403` · FO EndUser `200` |  |
| `GET__tenant_usage-detail.json` | `GET /tenant/usage-detail` | FO Admin `200` · FO EndUser `403` |  |
| `GET__usage.json` | `GET /usage` | FO Admin `200` · FO EndUser `200` |  |
| `GET__usage_current.json` | `GET /usage/current` | FO Admin `200` · FO EndUser `200` |  |
| `GET__usage_export-pdf.json` | `GET /usage/export-pdf` | FO Admin `200` · FO EndUser `200` |  |
| `GET__usage_export.json` | `GET /usage/export` | FO Admin `200` · FO EndUser `200` |  |
| `GET__user.json` | `GET /user` | FO Admin `200` · FO EndUser `403` |  |
| `GET__user_203.json` | `GET /user/203` | FO Admin `200` · FO EndUser `403` |  |
| `GET__user_compact-list-to-share.json` | `GET /user/compact-list-to-share` | FO Admin `403` · FO EndUser `200` | Có bộ lọc `name` và `email`. Trả `[]` vì tenant test chỉ có một người dùng — không phải endpoint hỏng. |
| `GET__user_me.json` | `GET /user/me` | FO Admin `200` · FO EndUser `200` |  |
| `POST__auth_logout.json` | `POST /auth/logout` | FO Admin `201` | Huỷ phiên. Gọi bằng tài khoản Admin để không làm hỏng token End User đang dùng. |

## `02-template-va-bo-chung-tu/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `GET__template.json` | `GET /template?page=1&limit=5` | FO Admin `403` · FO EndUser `200` |  |
| `GET__template_5383.json` | `GET /template/5383` | FO EndUser `200` | Định nghĩa cột của bảng kê chi phí — đây là cấu trúc lines[] mà IDP sẽ trả. |
| `GET__template_5384.json` | `GET /template/5384` | FO Admin `200` · FO EndUser `200` |  |
| `GET__template_assigned-template.json` | `GET /template/assigned-template` | FO Admin `200` · FO EndUser `200` |  |
| `GET__template_compact-for-partner.json` | `GET /template/compact-for-partner` | FO Admin `200` · FO EndUser `403` |  |
| `GET__template_compact.json` | `GET /template/compact` | FO Admin `403` · FO EndUser `200` |  |
| `GET__template_created-by-compact.json` | `GET /template/created-by-compact` | FO Admin `200` · FO EndUser `200` |  |
| `GET__template_document-set-list__KHONG-co-tham-so-sort-LOI-400.json` | `GET /template/document-set-list` | FO Admin `403` · FO EndUser `400` |  |
| `GET__template_document-set-list__co-tham-so-sort-OK.json` | `GET /template/document-set-list?page=1&limit=10&sort=-id` | FO EndUser `200` | Bắt buộc phải có tham số `sort`, nếu không sẽ dính lỗi SQL của IDP. |
| `GET__template_document-set.json` | `GET /template/document-set` | FO Admin `403` · FO EndUser `200` |  |
| `GET__template_document-set_193.json` | `GET /template/document-set/193` | FO Admin `403` · FO EndUser `200` |  |
| `GET__template_fill-owner.json` | `GET /template/fill-owner` | FO Admin `200` · FO EndUser `200` |  |
| `GET__template_get-sync-templates-bo.json` | `GET /template/get-sync-templates-bo` | FO Admin `200` · FO EndUser `403` |  |
| `GET__template_partner-templates.json` | `GET /template/partner-templates` | FO Admin `200` · FO EndUser `403` |  |
| `GET__template_sync-bo-templates.json` | `GET /template/sync-bo-templates?override=false` | FO Admin `200` · FO EndUser `403` | Tham số `override` là BẮT BUỘC — thiếu nó thì trả chuỗi rỗng chứ không báo lỗi. |
| `POST__template_document-set.json` | `POST /template/document-set` | FO EndUser `201` | Tạo một bộ chứng từ CÓ TÊN — đây là cách hiện thực `templateSetCode` của D3 I-07 bằng cơ chế sẵn có của IDP. |

## `03-anh-xa-mbal/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `GET__mbal_docset-map.json` | `GET /mbal/docset-map` | FO Admin `200` · FO EndUser `200` |  |
| `GET__mbal_docset-map_77.json` | `GET /mbal/docset-map/77` | FO Admin `200` · FO EndUser `200` |  |
| `GET__mbal_docsets.json` | `GET /mbal/docsets` | FO Admin `200` · FO EndUser `200` |  |
| `GET__mbal_statistics.json` | `GET /mbal/statistics` | FO Admin `200` · FO EndUser `200` |  |
| `GET__mbal_template-map.json` | `GET /mbal/template-map` | FO Admin `200` · FO EndUser `200` |  |
| `GET__mbal_template-map_15.json` | `GET /mbal/template-map/15` | FO Admin `200` · FO EndUser `200` |  |
| `GET__mbal_templates.json` | `GET /mbal/templates` | FO Admin `200` · FO EndUser `200` |  |
| `POST__mbal_create-request.json` | `POST /mbal/create-request` | FO EndUser `201` | Nộp bằng claim_type + document_type vừa cấu hình ở bước trên — backend tự tra ra bộ HEALTH_CLAIM_V1. |
| `POST__mbal_docset-map.json` | `POST /mbal/docset-map` | FO EndUser `201` | Nối một loại claim với một bộ chứng từ. Sau bước này, `POST /mbal/create-request` chỉ cần `claim_type` + `document_type` là đủ. |
| `POST__mbal_template-map.json` | `POST /mbal/template-map` | FO EndUser `201` | Nạp ánh xạ cho template 5384 — chính là template khớp trong lần trích xuất thật, và là lý do `subDocumentType` trả `null`. |

## `04-nop-chung-tu-va-trich-xuat/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `GET__request.json` | `GET /request?page=1&limit=3` | FO Admin `403` · FO EndUser `200` |  |
| `GET__request_21376_check-status.json` | `GET /request/21376/check-status` | FO Admin `403` · FO EndUser `200` |  |
| `GET__request_21376_check-workflow-request.json` | `GET /request/21376/check-workflow-request` | FO Admin `403` · FO EndUser `200` |  |
| `GET__request_21376_export.json` | `GET /request/21376/export` | FO EndUser `200` | Xuất kết quả một request ra file. |
| `GET__request_21376_get-ocr.json` | `GET /request/21376/get-ocr` | FO Admin `403` · FO EndUser `200` |  |
| `GET__request_21376_raw-file.json` | `GET /request/21376/raw-file` | FO EndUser `200` | Tải lại chính file gốc đã nộp — dùng thay cho deep link I-10. |
| `GET__request_21376_result.json` | `GET /request/21376/result` | FO Admin `403` · FO EndUser `200` |  |
| `GET__request_21383_result.json` | `GET /request/21383/result` | FO EndUser `200` | Kết quả SAU KHI đã nạp ánh xạ template-map — so với lần trước để thấy subDocumentType đổi từ null sang mã CL. |
| `GET__request_group_4_check-status.json` | `GET /request/group/4/check-status` | FO Admin `403` · FO EndUser `200` |  |
| `PATCH__request_id_cancel.json` | `PATCH /request/{id}/cancel` | FO EndUser `200` | Huỷ một request. Chỉ hợp lệ khi request đang chạy — request đã Completed thì trả 400 'Request is not running!'. |
| `PATCH__request_id_retry.json` | `PATCH /request/{id}/retry` | FO EndUser `200` | Chạy lại một request đã lỗi. |
| `POST__mbal_create-request.json` | `POST /mbal/create-request` | FO EndUser `201` | Đường tắt: backend tự tra docset-map để lấy templates |
| `POST__request_compare-multiple.json` | `POST /request/compare-multiple` | FO EndUser `201` | Đối chiếu nhiều request cùng lúc với một file ground-truth. |
| `POST__request_compare.json` | `POST /request/compare` | FO EndUser `201` | Đối chiếu kết quả trích xuất với một file Excel ground-truth. CHỈ NHẬN XLSX — dùng multerExcelOptions, gửi PDF hay PNG đều bị chặn. Đây là công cụ đo  |
| `POST__request_export-multiple-request.json` | `POST /request/export-multiple-request` | FO EndUser `201` | Xuất kết quả nhiều request ra một file. |
| `POST__request_from-url.json` | `POST /request/from-url` | FO EndUser `400` | Nộp THEO URL thay vì bytes — đường gần nhất với by-reference của D3 I-07. IDP tự GET url phía server, không gắn header nên pre-signed URL dùng được. |
| `POST__request_merge-multiple.json` | `POST /request/merge-multiple` | FO EndUser `201` | Gộp NHIỀU ẢNH thành một request (tối đa 10 file). Là multipart nhiều file 'document'. Gửi PDF thì lỗi 'Failed to merge images'. |
| `POST__request_sync.json` | `POST /request/sync` | FO EndUser `201` | Biến thể đồng bộ: chờ và trả kết quả trong cùng một response |
| `PUT__request_id_result.json` | `PUT /request/{id}/result` | FO EndUser `200` | Sửa kết quả trích xuất. Endpoint này là lý do kết quả của IDP KHÔNG bất biến — D3 §I-09 dựa vào tính bất biến để so raw-versus-corrected, nên adapter  |

## `05-danh-gia/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `GET__workflow_export.json` | `GET /workflow/export` | FO Admin `200` · FO EndUser `200` |  |
| `GET__workflow_forwarder_api_v1_auto_login.json` | `GET /workflow/forwarder/api/v1/auto_login` | FO Admin `400` · FO EndUser `400` | Luôn trả 400 — **cố ý**, hardcode trong controller: `Auto login is disabled. Please enable it in the settings`. Không phải lỗi, và hệ thống claim khôn |
| `GET__workflow_forwarder_api_v1_flows.json` | `GET /workflow/forwarder/api/v1/flows` | FO Admin `200` · FO EndUser `200` |  |
| `GET__workflow_forwarder_api_v1_flows_8d873709-4cac-4f26-82f1-b1f9d28c6e73.json` | `GET /workflow/forwarder/api/v1/flows/8d873709-4cac-4f26-82f1-b1f9d28c6e73` | FO Admin `200` · FO EndUser `200` | Chi tiết một flow — nhẹ hơn nhiều so với /flows trả cả danh sách. |
| `GET__workflow_forwarder_api_v1_flows_assigns_all.json` | `GET /workflow/forwarder/api/v1/flows/assigns/all?target_tenant_id=176` | FO Admin `200` · FO EndUser `200` | Tham số `target_tenant_id` là bắt buộc; thiếu nó thì parseInt(undefined) = NaN và trả 500. |
| `GET__workflow_forwarder_health_check.json` | `GET /workflow/forwarder/health_check` | FO Admin `200` · FO EndUser `200` |  |
| `GET__workflow_get-workflow-components.json` | `GET /workflow/get-workflow-components?flowId=8d873709-4cac-4f26-82f1-b1f9d28c6e73` | FO Admin `200` · FO EndUser `200` |  |
| `GET__workflow_request_1403.json` | `GET /workflow/request/1403` | FO Admin `400` · FO EndUser `200` |  |
| `GET__workflow_request_1403_result_json.json` | `GET /workflow/request/1403/result/json` | FO Admin `400` · FO EndUser `200` |  |
| `GET__workflow_request_1404.json` | `GET /workflow/request/1404` | FO EndUser `200` | Bản ghi đầy đủ của một yêu cầu đánh giá chạy bằng flow 'Claim 28/8'. |
| `GET__workflow_request_1404_result_json.json` | `GET /workflow/request/1404/result/json` | FO EndUser `200` | Kết quả của flow 'Claim 28/8' — flow được sửa để bỏ bước convert markdown. |
| `GET__workflow_request_search.json` | `GET /workflow/request/search?page=1&limit=5` | FO EndUser `200` · FO Admin `200` | Dữ liệu lọc theo tenantId — gọi bằng tài khoản End User (tenant 176) mới thấy bản ghi. |
| `GET__workflow_share-with-admin_8d873709-4cac-4f26-82f1-b1f9d28c6e73.json` | `GET /workflow/share-with-admin/8d873709-4cac-4f26-82f1-b1f9d28c6e73` | FO EndUser `200` |  |
| `GET__workflow_sync-bo-workflows.json` | `GET /workflow/sync-bo-workflows` | FO Admin `200` · FO EndUser `403` |  |

## `06-master-data/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `GET__metadata_forwarder_api_database_fields_table_313.json` | `GET /metadata/forwarder/api/database/fields/table/313/` | FO Admin `200` |  |
| `GET__metadata_forwarder_api_database_fields_table_314.json` | `GET /metadata/forwarder/api/database/fields/table/314/` | FO Admin `200` |  |
| `GET__metadata_forwarder_api_database_fields_table_316.json` | `GET /metadata/forwarder/api/database/fields/table/316/` | FO Admin `200` |  |
| `GET__metadata_forwarder_api_database_fields_table_321.json` | `GET /metadata/forwarder/api/database/fields/table/321/` | FO Admin `200` |  |
| `GET__metadata_forwarder_api_database_fields_table_322.json` | `GET /metadata/forwarder/api/database/fields/table/322/` | FO Admin `200` |  |
| `GET__metadata_forwarder_api_database_rows_table_313.json` | `GET /metadata/forwarder/api/database/rows/table/313/?user_field_names=true&size=200` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_api_database_rows_table_314.json` | `GET /metadata/forwarder/api/database/rows/table/314/?user_field_names=true&size=200` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_api_database_rows_table_316.json` | `GET /metadata/forwarder/api/database/rows/table/316/?user_field_names=true&size=200` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_api_database_rows_table_321.json` | `GET /metadata/forwarder/api/database/rows/table/321/?user_field_names=true&size=200` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_api_database_rows_table_322.json` | `GET /metadata/forwarder/api/database/rows/table/322/?user_field_names=true&size=200` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_api_database_rows_table_322_3.json` | `GET /metadata/forwarder/api/database/rows/table/322/3/?user_field_names=true` | FO Admin `200` · FO EndUser `200` | Đọc MỘT dòng master data — dùng khi đã biết id, rẻ hơn tra cả bảng. |
| `GET__metadata_forwarder_api_database_views_table_322.json` | `GET /metadata/forwarder/api/database/views/table/322/` | FO Admin `200` · FO EndUser `200` | Lấy VIEW ID của một bảng — cần cho endpoint table-data. |
| `GET__metadata_forwarder_database_list.json` | `GET /metadata/forwarder/database/list` | FO Admin `200` · FO EndUser `403` |  |
| `GET__metadata_forwarder_partner-table-list.json` | `GET /metadata/forwarder/partner-table-list` | FO Admin `200` · FO EndUser `403` |  |
| `GET__metadata_forwarder_table-data_313.json` | `GET /metadata/forwarder/table-data/313` | FO Admin `400` · FO EndUser `400` |  |
| `GET__metadata_forwarder_table-data_314.json` | `GET /metadata/forwarder/table-data/314` | FO Admin `400` · FO EndUser `400` |  |
| `GET__metadata_forwarder_table-data_316.json` | `GET /metadata/forwarder/table-data/316` | FO Admin `400` · FO EndUser `400` |  |
| `GET__metadata_forwarder_table-data_321.json` | `GET /metadata/forwarder/table-data/321` | FO Admin `400` · FO EndUser `400` |  |
| `GET__metadata_forwarder_table-data_322.json` | `GET /metadata/forwarder/table-data/322` | FO Admin `400` · FO EndUser `400` |  |
| `GET__metadata_forwarder_table-data_476.json` | `GET /metadata/forwarder/table-data/476` | FO Admin `200` · FO EndUser `200` | Endpoint này nhận VIEW ID chứ không phải TABLE ID. Đó là lý do gọi bằng table id luôn trả 400. |
| `GET__metadata_forwarder_table-fields_313.json` | `GET /metadata/forwarder/table-fields/313` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_table-fields_314.json` | `GET /metadata/forwarder/table-fields/314` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_table-fields_316.json` | `GET /metadata/forwarder/table-fields/316` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_table-fields_321.json` | `GET /metadata/forwarder/table-fields/321` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_table-fields_322.json` | `GET /metadata/forwarder/table-fields/322` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_table-list.json` | `GET /metadata/forwarder/table-list` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_forwarder_trash.json` | `GET /metadata/forwarder/trash` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_opensearch_table_322_index_async.json` | `GET /metadata/opensearch/table/322/index/async` | FO Admin `200` | Kích hoạt đánh index cho bảng master data — phải chạy trước khi search dùng được. |
| `GET__metadata_opensearch_table_322_index_status.json` | `GET /metadata/opensearch/table/322/index/status` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_opensearch_table_322_info.json` | `GET /metadata/opensearch/table/322/info` | FO Admin `200` · FO EndUser `200` |  |
| `GET__metadata_opensearch_table_322_search.json` | `GET /metadata/opensearch/table/322/search?searchTerm=N%E1%BB%99i%20ti%E1%BA%BFt&fields=hospital_name&size=5` | FO Admin `200` · FO EndUser `200` | Tra cứu toàn văn bảng bệnh viện 13.328 dòng — cách đúng thay vì tải cả bảng. |
| `POST__metadata_forwarder_table-data.json` | `POST /metadata/forwarder/table-data` | FO Admin `400` | Endpoint có trong swagger nhưng KHÔNG dùng được — phải đi qua proxy api/database/rows. |

## `07-van-hanh/`

| File | Endpoint | Kết quả gọi | Mô tả |
|---|---|---|---|
| `GET__tool.json` | `GET /tool` | FO Admin `200` · FO EndUser `200` | Danh sách tool vận hành. Các POST /tool/* là thao tác nguy hiểm, không gọi. |
