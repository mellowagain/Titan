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
    if len(sys.argv) != 2: # The first argument is always the script itself
        print("Please provide a valid file to convert.")
        return -1
    
    if Path(sys.argv[1]).exists() is False:
        print("Please provide a EXISTING file to convert.")
        return -1

    list = []
    
    try:
        targetFile = open("accounts.json", "w")
        
        with open(sys.argv[1], "rU", encoding = "UTF-8") as sourceFile:
            for line in sourceFile:
                if line.find(":") != -1:
                    parts = line.split(":")
                    list.append({ 
                        "username": parts[0], 
                        "password": parts[1].replace("\n", "") # https://www.python.org/dev/peps/pep-0278/
                    })
        
        # TODO: Add support for multiple indexes
        data = { "indexes": [ { "accounts": [ {} ] } ] }
        data["indexes"][0]["accounts"] = list
        
        json.dump(data, targetFile, indent = 4)
        
        targetFile.close()
    except:
        print("A error occured while trying to convert the existing file.")
        return -1
    
    return 0

if __name__ == "__main__":
    exit(main())
