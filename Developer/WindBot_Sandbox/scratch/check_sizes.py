import os

paths = [
    r"c:\Users\admin\Documents\EDOTh\cards.delta.cdb",
    r"c:\Users\admin\Documents\EDOTh\WindBot\cards.delta.cdb",
    r"c:\Users\admin\Documents\EDOTh\repositories\delta-bagooska\cards.delta.cdb"
]

for p in paths:
    if os.path.exists(p):
        print(f"Path: {p} | Size: {os.path.getsize(p)} bytes")
    else:
        print(f"Path: {p} | Does not exist")
