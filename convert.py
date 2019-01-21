#!/usr/bin/env python

#
# Simple Python3 script that converts your existing accounts file (in the format of username:password)
# to a Titan-compatible accounts.json file automatically.
# Run this script using Python 3: python convert.py accounts.txt
#

from pathlib import Path
import json
import sys


def main():
    if len(sys.argv) != 3:  # The first argument is always the script itself
        print("Please provide a valid file and index amount to convert.")
        return -1

    if Path(sys.argv[1]).exists() is False:
        print("Please provide a EXISTING file to convert.")
        return -1

    if not int(sys.argv[2]):
        print("Please provide a valid amount of accounts per index!")
        return -1

    out = []

    try:
        targetFile = open("accounts.json", "w")

        with open(sys.argv[1], "rU", encoding="UTF-8") as sourceFile:
            for line in sourceFile:
                if line.find(":") != -1:
                    parts = line.split(":")
                    out.append({
                        "username": parts[0],
                        "password": parts[1].strip()
                    })

        data = {"indexes": [{"accounts": [{}]}]}
        cnt = len(out)

        slices = int(sys.argv[2])
        amt = int(cnt / slices)
        last = cnt % slices

        for i in range(0, amt, 1):

            if i < amt - 1:
                data["indexes"][i]["accounts"] = out[i * slices: (i + 1) * slices]
                data["indexes"].append({"accounts": [{}]})
            else:
                data["indexes"][i]["accounts"] = out[i * slices: i * slices + last]

        json.dump(data, targetFile, indent=4)

        targetFile.close()
    except:
        print("A error occured while trying to convert the existing file.")
        return -1

    return 0


if __name__ == "__main__":
    exit(main())

