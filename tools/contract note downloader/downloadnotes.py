import csv
import os
import requests
from time import sleep

# --- Config ---
CSV_PATH = "contract_notes.csv"
OUTPUT_DIR = "downloads"
BASE_URL="https://online.hl.co.uk/my-accounts/contract_note_viewer/brg/{id}/prd/70/download/1"


HEADERS = {
    "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_0)",
    "Referer": "https://online.hl.co.uk/my-accounts",
    "Cookie": 'xxx'
}

os.makedirs(OUTPUT_DIR, exist_ok=True)

# --- Main loop ---
with open(CSV_PATH) as f:
    reader = csv.DictReader(f)
    for row in reader:
        note_id = row.get("contract_note_id") or row.get("id")
        if not note_id:
            print("⚠️  No contract_note_id or id column found in CSV row — skipping")
            continue

        url = BASE_URL.format(id=note_id)
        out_path = os.path.join(OUTPUT_DIR, f"{note_id}.pdf")

        print(f"Downloading {note_id}...")

        try:
            r = requests.get(url, headers=HEADERS)
        except Exception as e:
            print(f"✖ {note_id}: request failed: {e}")
            continue

        ctype = r.headers.get("content-type", "").lower()

        if r.status_code == 200 and "pdf" in ctype:
            with open(out_path, "wb") as out:
                out.write(r.content)
            print(f"✔ Saved to {out_path}")
        else:
            print(f"✖ {note_id}: HTTP {r.status_code}, content-type {ctype}")
            # Optionally dump first few characters for debugging:
            print(r.text[:2000])

