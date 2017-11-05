#coding:utf-8
import traceback
import os
import sys
reload(sys)
sys.setdefaultencoding("utf-8")

def export_bytecode64(import_root_path,export_root_path,import_path):
    if(import_root_path == import_path):
        import_root_path = os.path.abspath(import_root_path).replace("\\","/")
        export_root_path = os.path.abspath(export_root_path).replace("\\","/")
        import_path = import_root_path
        pypath = os.path.abspath(sys.argv[0])
        os.chdir(os.path.split(pypath)[0])
    
    for file in os.listdir(import_path):      
        abspath = import_path + "/" + file
        if(os.path.isdir(abspath)):  
            export_bytecode64(import_root_path,export_root_path,abspath)
        else: 
            if(file[-3:] == "lua"):
                cur_export_path = import_path.replace(import_root_path,export_root_path)
                if(not os.path.exists(cur_export_path)):
                    os.makedirs(cur_export_path)
                command = "luajit64 -b {0} {1}/{2}.bytes".format(abspath,cur_export_path,file[:-4])
                print(u"导出:" + file)
                os.system(command)            
    
if(__name__ == "__main__"):
    try:        
        export_bytecode64(sys.argv[1],sys.argv[2],sys.argv[1])
        print("build bytecode64 success")
    except:
        print("build bytecode64 failed")
        traceback.print_exc()
        raw_input()
    