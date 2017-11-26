#!/usr/bin/python
#coding:utf-8
import traceback
import os
import sys

def export_bytecode(import_root_path,export_root_path,import_path):
    if(import_root_path == import_path):
        import_root_path = os.path.abspath(import_root_path).replace("\\","/")
        export_root_path = os.path.abspath(export_root_path).replace("\\","/")
        import_path = import_root_path
        os.chdir(sys.path[0])
    
    for file in os.listdir(import_path):      
        abspath = import_path + "/" + file
        if(os.path.isdir(abspath)):  
            export_bytecode(import_root_path,export_root_path,abspath)
        else: 
            if(file[-3:] == "lua"):
                cur_export_path = import_path.replace(import_root_path,export_root_path)
                if(not os.path.exists(cur_export_path)):
                    os.makedirs(cur_export_path)
                print("EP64:" + file)
                os.system(sys.path[0] + "/luajit64 -b {0} {1}/{2}_64".format(abspath,cur_export_path,file[:-4]))    
                print("EP32:" + file)
                os.system(sys.path[0] + "/luajit32 -b {0} {1}/{2}_32".format(abspath,cur_export_path,file[:-4]))                 
    
if(__name__ == "__main__"):
    try:        
        export_bytecode(sys.argv[1],sys.argv[2],sys.argv[1])
        print("build bytecode success")
    except:
        print("build bytecode failed")
        traceback.print_exc()
        raw_input()
    