import os

def find_cdbs():
    for root, dirs, files in os.walk(r"c:\Users\admin\Documents\EDOTh"):
        for f in files:
            if f.endswith(".cdb"):
                print(os.path.join(root, f))

if __name__ == "__main__":
    find_cdbs()
