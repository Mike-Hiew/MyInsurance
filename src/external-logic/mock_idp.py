"""Mock BnK IDP chạy thẳng trên máy (SRD §12.3a) — 0 quota tenant 177.

    python mock_idp.py            # nghe http://localhost:8099

In ra đúng cái lên dây: request line, header, và từng part multipart
(Content-Disposition, Content-Type, kích thước). Bắt chước hai hành vi thật:
  - thiếu User-Agent            → 403 "error code: 1010" (Cloudflare)
  - part document không filename= → 500 "reading 'path'" (busboy/multer)
Token bị che khi in. Không ghi gì ra đĩa.
"""
import json
import re
import sys
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 8099


def split_parts(body: bytes, boundary: bytes):
    for chunk in body.split(b"--" + boundary)[1:]:
        if chunk.startswith(b"--"):
            break
        head, _, content = chunk[2:].partition(b"\r\n\r\n")
        yield head.decode("latin-1"), content[:-2]


class Handler(BaseHTTPRequestHandler):
    def do_POST(self):
        body = self.rfile.read(int(self.headers.get("Content-Length", 0)))
        print("=" * 72)
        print(self.requestline)
        for k, v in self.headers.items():
            if k.lower() == "authorization":
                v = v[:13] + "…(che)" if len(v) > 13 else v
            print(f"  {k}: {v}")

        if "User-Agent" not in self.headers:
            print("→ 403 error code: 1010  (thiếu User-Agent)")
            return self.reply(403, b"error code: 1010", "text/plain")

        m = re.search(r'boundary="?([^";]+)"?', self.headers.get("Content-Type", ""))
        if not m:
            print("→ 400 không có boundary")
            return self.reply(400, b'{"message":"no boundary"}')

        fields, file_part = {}, None
        for i, (head, content) in enumerate(split_parts(body, m.group(1).encode())):
            print(f"  --- part {i} ---")
            for line in head.split("\r\n"):
                print(f"    {line}")
            print(f"    <{len(content)} bytes>")
            name = re.search(r'name="?([^";]+)"?', head).group(1)
            if re.search(r"filename=", head):
                file_part = (name, re.search(r'filename="?([^";]+)"?', head).group(1), len(content))
            else:
                fields[name] = content.decode("utf-8", "replace")

        if not file_part:
            print("→ 500 reading 'path'  (part document KHÔNG có filename= — đúng bug ODC)")
            return self.reply(500, json.dumps({"statusCode": 500,
                "message": "Cannot read properties of undefined (reading 'path')"}).encode())

        print(f"  fields={fields}")
        print(f"→ 201  file={file_part[1]} ({file_part[2]} bytes)")
        resp = {"id": 1, "name": fields.get("name"), "status": "Completed", "pageCount": 1,
                "templates_data": [{"templateId": 6093, "mock": True}]}
        if self.path == "/request":
            resp = {"requestId": 1, "pageCount": 1}
        self.reply(201, json.dumps(resp).encode())

    def reply(self, status, payload, ctype="application/json"):
        self.send_response(status)
        self.send_header("Content-Type", ctype)
        self.send_header("Content-Length", str(len(payload)))
        self.end_headers()
        self.wfile.write(payload)
        sys.stdout.flush()

    def log_message(self, *_):
        pass


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    print(f"mock IDP nghe http://localhost:{PORT}", flush=True)
    ThreadingHTTPServer(("localhost", PORT), Handler).serve_forever()
