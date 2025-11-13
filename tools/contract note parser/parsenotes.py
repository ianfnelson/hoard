import pdfplumber
import re
import os
import csv
from concurrent.futures import ThreadPoolExecutor, as_completed

FOLDER = "notes"
OUTPUT = "parsed.csv"

# Charge patterns
pattern_charge = re.compile(r"dealing charge", re.IGNORECASE)
pattern_stamp = re.compile(r"transfer stamp", re.IGNORECASE)
pattern_ptm = re.compile(r"P\.T\.M\s+Levy", re.IGNORECASE)
pattern_fx = re.compile(r"FX Charge", re.IGNORECASE)

# Instrument detail patterns
pattern_isin = re.compile(r"\b[A-Z]{2}[A-Z0-9]{9}[0-9]\b")
pattern_stockcode = re.compile(r"STOCK CODE:\s*(.*)", re.IGNORECASE)

def extract_amount(lines, index):
    """Return the decimal number appearing on the same line or the line after."""
    # Try same line
    numbers = re.findall(r"\d+\.\d{1,2}", lines[index])
    if numbers:
        return float(numbers[-1])

    # Try the next line
    if index + 1 < len(lines):
        numbers = re.findall(r"\d+\.\d{1,2}", lines[index + 1])
        if numbers:
            return float(numbers[-1])

    return None

def process_file(fullpath):
    filename = os.path.basename(fullpath)
    print("Processing:", filename)

    with pdfplumber.open(fullpath) as pdf:
        text = "\n".join(page.extract_text() for page in pdf.pages if page.extract_text())

    # Normalise line endings
    lines = [line.strip() for line in text.splitlines() if line.strip()]

    dealing = None
    stamp = None
    ptm = None
    fx = None

    isin = None
    name = None
    stock_code = None

    for i, line in enumerate(lines):
        # --- CHARGES ---------------------------------------------------------
        if pattern_charge.search(line):
            dealing = extract_amount(lines, i)

        if pattern_stamp.search(line):
            stamp = extract_amount(lines, i)

        if pattern_ptm.search(line):
            ptm = extract_amount(lines, i)

        if pattern_fx.search(line):
            fx = extract_amount(lines, i)

        # --- ISIN, Name ------------------------------------------------
        if isin is None:
            m = pattern_isin.search(line)
            if m:
                isin = m.group(0)
                if i + 1 < len(lines):
                    name = lines[i + 1].strip()

        # --- STOCK CODE -------------------------------------------------------
        m = pattern_stockcode.search(line)
        if m:
            stock_code = m.group(1).strip()

    # Store results for this note
    return {
        "filename": filename,
        "dealing": dealing,
        "stamp": stamp,
        "fx": fx,
        "ptm": ptm,
        "isin": isin,
        "name": name,
        "stock_code": stock_code
    }


results = []
files = [os.path.join(FOLDER, f) for f in os.listdir(FOLDER) if f.lower().endswith(".pdf")]

with ThreadPoolExecutor(max_workers=8) as executor:
    futures = {executor.submit(process_file, fullpath): fullpath for fullpath in files}
    for future in as_completed(futures):
        results.append(future.result())


# --- WRITE CSV ---------------------------------------------------------------

fieldnames = [
    "filename",
    "dealing",
    "stamp",
    "fx",
    "ptm",
    "isin",
    "name",
    "stock_code"
]

with open(OUTPUT, "w", newline="") as f:
    writer = csv.DictWriter(f, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(results)

print("Done! Output written to", OUTPUT)