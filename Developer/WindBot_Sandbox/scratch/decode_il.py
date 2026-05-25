import json
import os

# Opcode map for MSIL (minimal map for decoding)
OPCODE_MAP = {
    0x00: "nop",
    0x02: "ldarg.0",
    0x03: "ldarg.1",
    0x04: "ldarg.2",
    0x05: "ldarg.3",
    0x06: "ldloc.0",
    0x07: "ldloc.1",
    0x08: "ldloc.2",
    0x09: "ldloc.3",
    0x0a: "stloc.0",
    0x0b: "stloc.1",
    0x0c: "stloc.2",
    0x0d: "stloc.3",
    0x14: "ldnull",
    0x16: "ldc.i4.0",
    0x17: "ldc.i4.1",
    0x18: "ldc.i4.2",
    0x19: "ldc.i4.3",
    0x1f: "ldc.i4.s",
    0x20: "ldc.i4",
    0x25: "dup",
    0x26: "pop",
    0x2a: "ret",
    0x2b: "br.s",
    0x2c: "brfalse.s",
    0x2d: "brtrue.s",
    0x2e: "beq.s",
    0x2f: "bge.s",
    0x30: "bgt.s",
    0x31: "ble.s",
    0x32: "blt.s",
    0x3b: "ldc.i4.8",
    0x6f: "callvirt",
    0x72: "ldsfld",
    0x7b: "ldfld",
    0x7d: "stfld",
    0x8d: "ldlen",
    0x8e: "ldelem.i4",
    0xa2: "stelem.i4",
    0xde: "castclass",
}

# The IL bytes obtained from powershell
il_bytes = [
    4, 111, 71, 0, 0, 10, 10, 43, 101, 6, 111, 72, 0, 0, 10, 11, 3, 3, 111, 91, 0, 0, 6, 111, 108,
    0, 0, 6, 7, 7, 111, 176, 0, 0, 6, 111, 108, 0, 0, 6, 2, 3, 7, 111, 83, 2, 0, 6, 44, 60, 3,
    111, 107, 0, 0, 6, 7, 111, 107, 0, 0, 6, 48, 30, 3, 111, 107, 0, 0, 6, 7, 111, 107, 0, 0,
    6, 50, 32, 3, 111, 137, 0, 0, 6, 44, 24, 7, 111, 165, 0, 0, 6, 44, 16, 2, 40, 59, 2, 0, 6,
    3, 7, 111, 110, 1, 0, 6, 12, 222, 44, 6, 111, 73, 0, 0, 10, 45, 147, 222, 10, 6, 44, 6,
    6, 111, 31, 0, 0, 10, 220, 3, 111, 131, 0, 0, 6, 44, 14, 2, 40, 59, 2, 0, 6, 3, 20, 111,
    110, 1, 0, 6, 42, 20, 42, 8, 42
]

i = 0
while i < len(il_bytes):
    b = il_bytes[i]
    op = OPCODE_MAP.get(b, f"0x{b:02x}")
    if op in ["callvirt", "ldsfld", "ldfld", "stfld", "castclass", "call"]:
        # 4 byte metadata token
        token = il_bytes[i+1] | (il_bytes[i+2] << 8) | (il_bytes[i+3] << 16) | (il_bytes[i+4] << 24)
        print(f"{i:04d}: {op} token:0x{token:08x}")
        i += 5
    elif op in ["br.s", "brfalse.s", "brtrue.s", "beq.s", "bge.s", "bgt.s", "ble.s", "blt.s", "ldc.i4.s"]:
        # 1 byte offset/value
        val = il_bytes[i+1]
        # handle sign extension
        if val > 127:
            val -= 256
        print(f"{i:04d}: {op} {val}")
        i += 2
    else:
        print(f"{i:04d}: {op}")
        i += 1
